using System.Globalization;
using System.Text;
using System.Text.Json;

// ─── Configuration ──────────────────────────────────────────────────────────
// Backend base URL, in priority order: CLI arg > BACKEND_URL env var > default.
var baseUrl = args.FirstOrDefault()
              ?? Environment.GetEnvironmentVariable("BACKEND_URL")
              ?? "http://localhost:8080";

var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
var osrmClient = new HttpClient(); // Separate client for the public OSRM routing API
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

Console.WriteLine("🚀 Real-Road GPS Simulator starting...");
Console.WriteLine($"Backend: {baseUrl}");

// ─── Vehicle definitions (5 vehicles across Tashkent districts) ─────────────
var vehicleDefs = new (string Name, string Plate)[]
{
    ("Chilonzor-Patrol", "01-123-AAA"),
    ("Yunusobod-Taxi", "01-456-BBB"),
    ("Mirobod-Delivery", "01-789-CCC"),
    ("Sergeli-Express", "01-010-DDD"),
    ("Yakkasaroy-Bus", "01-555-EEE"),
};

// ─── Ensure the vehicles exist in the backend ───────────────────────────────
var existingVehicles = await GetExistingVehiclesAsync();
var vehicleIds = new long[vehicleDefs.Length];

for (var i = 0; i < vehicleDefs.Length; i++)
{
    var def = vehicleDefs[i];
    var existing = existingVehicles.FirstOrDefault(v =>
        string.Equals(v.PlateNumber, def.Plate, StringComparison.OrdinalIgnoreCase));

    if (existing is not null)
    {
        vehicleIds[i] = existing.Id;
        Console.WriteLine($"  [OK] {def.Name} (ID: {existing.Id})");
    }
    else
    {
        var created = await CreateVehicleAsync(def.Name, def.Plate);
        if (created != null) vehicleIds[i] = created.Id;
    }
}

// ─── Build real road routes via OSRM ────────────────────────────────────────
Console.WriteLine("\n🌐 Fetching real road paths from OSRM (Tashkent)...");
var routes = new Dictionary<int, List<(double lat, double lng)>>();

// Each route is a list of waypoints; OSRM expands them into a full driving path.
routes[0] = await GetRealRoadRouteAsync((41.2842, 69.2133), (41.2721, 69.2041), (41.2825, 69.1852)); // Chilonzor
routes[1] = await GetRealRoadRouteAsync((41.3541, 69.2884), (41.3751, 69.2821), (41.3672, 69.2905)); // Yunusobod
routes[2] = await GetRealRoadRouteAsync((41.2985, 69.2745), (41.2885, 69.2655), (41.2815, 69.2555)); // Mirobod
routes[3] = await GetRealRoadRouteAsync((41.2115, 69.2155), (41.2280, 69.2230), (41.1980, 69.2010)); // Sergeli
routes[4] = await GetRealRoadRouteAsync((41.2930, 69.2550), (41.2750, 69.2450), (41.2650, 69.2580)); // Yakkasaroy

Console.WriteLine("✅ Routes loaded. Starting simulation...\n");

// ─── Run the simulation ─────────────────────────────────────────────────────
// One cancellation source stops every vehicle loop at once (Enter or Ctrl+C).
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // Don't kill the process abruptly — cancel gracefully.
    cts.Cancel();
};

var simulationTasks = new List<Task>();

for (var i = 0; i < vehicleDefs.Length; i++)
{
    var vehicleId = vehicleIds[i];
    var vehicleName = vehicleDefs[i].Name;

    if (routes.TryGetValue(i, out var route) && vehicleId > 0)
    {
        Console.WriteLine($"[STARTING] {vehicleName} (route points: {route.Count})");
        simulationTasks.Add(SimulateVehicle(vehicleId, vehicleName, route, cts.Token));
    }
    else
    {
        Console.WriteLine($"[SKIPPED] {vehicleName}: route or vehicle ID missing.");
    }
}

if (simulationTasks.Count == 0)
{
    Console.WriteLine("❌ No simulation started. Check that the backend is running and routes loaded.");
    return;
}

Console.WriteLine($"🚀 {simulationTasks.Count} vehicles are on the road!");
Console.WriteLine("Press Enter to stop...");

// Block until the operator presses Enter, then cancel every loop.
Console.ReadLine();
cts.Cancel();

try
{
    await Task.WhenAll(simulationTasks);
}
catch (OperationCanceledException)
{
    // Expected on shutdown.
}

Console.WriteLine("🏁 Simulation stopped.");

// ─── Simulation loop for a single vehicle ───────────────────────────────────
async Task SimulateVehicle(long vehicleId, string name, List<(double lat, double lng)> route, CancellationToken token)
{
    var index = 0;
    while (!token.IsCancellationRequested)
    {
        var point = route[index];
        var payload = new
        {
            vehicleId,
            latitude = point.lat,
            longitude = point.lng,
            speed = Random.Shared.Next(30, 60),
            recordedAt = DateTime.UtcNow
        };
        var json = JsonSerializer.Serialize(payload, jsonOptions);

        try
        {
            var resp = await http.PostAsync("/api/location/update",
                new StringContent(json, Encoding.UTF8, "application/json"), token);
            if (resp.IsSuccessStatusCode)
                Console.WriteLine($"[{name}] @ {point.lat:F5}, {point.lng:F5}");
        }
        catch (OperationCanceledException)
        {
            break; // Cancelled mid-request — exit cleanly.
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{name}] Error: {ex.Message}");
        }

        try
        {
            await Task.Delay(2000, token);
        }
        catch (OperationCanceledException)
        {
            break;
        }

        index = (index + 1) % route.Count; // Loop back to the start of the route.
    }
}

// ─── OSRM helper: expand waypoints into a real driving path ─────────────────
async Task<List<(double lat, double lng)>> GetRealRoadRouteAsync(params (double lat, double lng)[] points)
{
    var coords = string.Join(";",
        points.Select(p =>
            $"{p.lng.ToString(CultureInfo.InvariantCulture)},{p.lat.ToString(CultureInfo.InvariantCulture)}"));
    var url = $"http://router.project-osrm.org/route/v1/driving/{coords}?overview=full&geometries=geojson";

    var res = new List<(double, double)>();
    try
    {
        var json = await osrmClient.GetStringAsync(url);
        using var doc = JsonDocument.Parse(json);
        var coordsArray = doc.RootElement.GetProperty("routes")[0].GetProperty("geometry").GetProperty("coordinates");
        foreach (var c in coordsArray.EnumerateArray())
            res.Add((c[1].GetDouble(), c[0].GetDouble())); // GeoJSON is [lng, lat]
    }
    catch
    {
        res.AddRange(points); // Fallback: use the raw waypoints if OSRM is unreachable.
    }

    return res;
}

// ─── Backend API helpers ────────────────────────────────────────────────────
async Task<List<VehicleDto>> GetExistingVehiclesAsync()
{
    try
    {
        var json = await http.GetStringAsync("/api/vehicles");
        return JsonSerializer.Deserialize<List<VehicleDto>>(json, jsonOptions) ?? [];
    }
    catch
    {
        return [];
    }
}

async Task<VehicleDto?> CreateVehicleAsync(string name, string plateNumber)
{
    var payload = JsonSerializer.Serialize(new { name, plateNumber }, jsonOptions);
    var resp = await http.PostAsync("/api/vehicles", new StringContent(payload, Encoding.UTF8, "application/json"));
    if (!resp.IsSuccessStatusCode) return null;
    return JsonSerializer.Deserialize<VehicleDto>(await resp.Content.ReadAsStringAsync(), jsonOptions);
}

sealed record VehicleDto(long Id, string Name, string PlateNumber);
