import axios, { AxiosError } from 'axios';
import { API_CONFIG } from '../config/api';
import type {
  Vehicle,
  LocationDto,
  LocationHistoryDto,
  CreateVehicleRequest,
  LocationUpdateRequest,
  LocationHistoryQuery,
  ApiError,
} from '../types';

const http = axios.create({
  baseURL: API_CONFIG.baseUrl,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000,
});

// ─── Error normalizer ─────────────────────────────────────────────────────────

function extractMessage(err: unknown): string {
  if (err instanceof AxiosError) {
    const data = err.response?.data as ApiError | undefined;
    return data?.error ?? err.message;
  }
  return String(err);
}

// ─── Vehicles ─────────────────────────────────────────────────────────────────

export async function fetchVehicles(): Promise<Vehicle[]> {
  try {
    const { data } = await http.get<Vehicle[]>(API_CONFIG.endpoints.vehicles);
    return data;
  } catch (err) {
    throw new Error(`Failed to fetch vehicles: ${extractMessage(err)}`);
  }
}

export async function createVehicle(req: CreateVehicleRequest): Promise<Vehicle> {
  try {
    const { data } = await http.post<Vehicle>(API_CONFIG.endpoints.vehicles, req);
    return data;
  } catch (err) {
    throw new Error(`Failed to create vehicle: ${extractMessage(err)}`);
  }
}

// ─── Location ─────────────────────────────────────────────────────────────────

export async function fetchCurrentLocation(vehicleId: number): Promise<LocationDto | null> {
  try {
    const { data } = await http.get<LocationDto>(API_CONFIG.endpoints.locationCurrent, {
      params: { vehicleId },
    });
    return data;
  } catch (err) {
    if (err instanceof AxiosError && err.response?.status === 404) return null;
    throw new Error(`Failed to fetch location for vehicle ${vehicleId}: ${extractMessage(err)}`);
  }
}

export async function fetchLocationHistory(
  query: LocationHistoryQuery,
): Promise<LocationHistoryDto[]> {
  try {
    const { data } = await http.get<LocationHistoryDto[]>(
      API_CONFIG.endpoints.locationHistory,
      { params: query },
    );
    return data;
  } catch (err) {
    throw new Error(`Failed to fetch location history: ${extractMessage(err)}`);
  }
}

export async function pushLocationUpdate(req: LocationUpdateRequest): Promise<void> {
  try {
    await http.post(API_CONFIG.endpoints.locationUpdate, req);
  } catch (err) {
    throw new Error(`Failed to push location update: ${extractMessage(err)}`);
  }
}
