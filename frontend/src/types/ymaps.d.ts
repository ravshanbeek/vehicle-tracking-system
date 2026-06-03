// Minimal Yandex Maps JS API v2.1 declarations.
// Only the subset used in this project is typed — expand as needed.

declare namespace ymaps {
  function ready(callback: () => void): void;

  // Marker interface — anything that can be added to the map
  interface MapObject {
    _mapObjectBrand: never;
  }

  class Map {
    constructor(
      container: HTMLElement | string,
      state: { center: [number, number]; zoom: number },
      options?: Record<string, unknown>,
    );
    geoObjects: GeoObjectCollection;
    setCenter(
      coords: [number, number],
      zoom?: number,
      options?: Record<string, unknown>,
    ): Promise<void>;
    destroy(): void;
  }

  class GeoObjectCollection {
    add(obj: Placemark | Polyline): void;
    remove(obj: Placemark | Polyline): void;
  }

  class Placemark {
    constructor(
      geometry: [number, number],
      properties?: Record<string, unknown>,
      options?: Record<string, unknown>,
    );
    geometry: PlacemarkGeometry;
    properties: DataManager;
    events: EventManager;
  }

  class Polyline {
    constructor(
      geometry: [number, number][],
      properties?: Record<string, unknown>,
      options?: Record<string, unknown>,
    );
    geometry: PolylineGeometry;
    properties: DataManager;
    events: EventManager;
  }

  interface PlacemarkGeometry {
    setCoordinates(coords: [number, number]): void;
  }

  interface PolylineGeometry {
    setCoordinates(coords: [number, number][]): void;
  }

  interface DataManager {
    set(key: string, value: unknown): void;
    get(key: string): unknown;
  }

  interface EventManager {
    add(type: string, handler: (e: unknown) => void): void;
    remove(type: string, handler: (e: unknown) => void): void;
  }
}

interface Window {
  ymaps: typeof ymaps;
}
