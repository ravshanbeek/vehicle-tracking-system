// ─── Single source of truth for all backend integration points ────────────────
// Change VITE_API_URL in .env — everything here derives from it automatically.

const BASE_URL = (import.meta.env.VITE_API_URL ?? 'http://localhost:8080').replace(/\/$/, '');

export const API_CONFIG = {
  baseUrl: BASE_URL,

  // REST endpoints
  endpoints: {
    vehicles: `${BASE_URL}/api/vehicles`,
    locationCurrent: `${BASE_URL}/api/location/current`,
    locationHistory: `${BASE_URL}/api/location/history`,
    locationUpdate: `${BASE_URL}/api/location/update`,
  },

  // SignalR hub
  signalrHub: `${BASE_URL}/hubs/vehicles`,

  // Yandex Maps API key
  yandexMapsApiKey: import.meta.env.VITE_YANDEX_MAPS_API_KEY ?? '',
} as const;
