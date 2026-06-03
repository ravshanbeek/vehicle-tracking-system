// ─── Domain types — mirror CarTracking.Application.DTOs ───────────────────────

export interface Vehicle {
  id: number;
  name: string;
  plateNumber: string;
  createdAt: string; // ISO 8601 UTC
}

export interface LocationDto {
  vehicleId: number;
  latitude: number;
  longitude: number;
  speed: number;
  recordedAt: string; // ISO 8601 UTC
}

export interface LocationHistoryDto extends LocationDto {
  id: number;
}

export interface CreateVehicleRequest {
  name: string;
  plateNumber: string;
}

export interface LocationUpdateRequest {
  vehicleId: number;
  latitude: number;
  longitude: number;
  speed: number;
  recordedAt: string; // ISO 8601 UTC
}

export interface LocationHistoryQuery {
  vehicleId: number;
  from: string; // ISO 8601
  to: string;   // ISO 8601
}

// ─── Error shape returned by the backend ──────────────────────────────────────

export interface ApiError {
  error: string;
}

// ─── SignalR connection state ─────────────────────────────────────────────────

export type SignalRStatus =
  | 'disconnected'
  | 'connecting'
  | 'connected'
  | 'reconnecting';
