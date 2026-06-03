import { useState, useEffect } from 'react';
import { API_CONFIG } from '../config/api';

type YMapsStatus = 'idle' | 'loading' | 'ready' | 'error';

let loadPromise: Promise<void> | null = null;

function loadScript(): Promise<void> {
  if (loadPromise) return loadPromise;

  loadPromise = new Promise<void>((resolve, reject) => {
    // Already loaded (e.g. HMR reload)
    if (window.ymaps) {
      window.ymaps.ready(resolve);
      return;
    }

    const apiKey = API_CONFIG.yandexMapsApiKey;
    const src = apiKey
      ? `https://api-maps.yandex.ru/2.1/?apikey=${apiKey}&lang=ru_RU`
      : 'https://api-maps.yandex.ru/2.1/?lang=ru_RU';

    const script = document.createElement('script');
    script.src = src;
    script.async = true;
    script.onload = () => window.ymaps.ready(resolve);
    script.onerror = () => reject(new Error('Failed to load Yandex Maps script'));
    document.head.appendChild(script);
  });

  return loadPromise;
}

/**
 * Loads the Yandex Maps JS API once and returns the readiness status.
 * Multiple callers share the same promise — the script is injected only once.
 */
export function useYandexMaps(): YMapsStatus {
  const [status, setStatus] = useState<YMapsStatus>('idle');

  useEffect(() => {
    setStatus('loading');
    loadScript()
      .then(() => setStatus('ready'))
      .catch(() => setStatus('error'));
  }, []);

  return status;
}
