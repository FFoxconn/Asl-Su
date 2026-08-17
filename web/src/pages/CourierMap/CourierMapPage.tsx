import { useEffect, useRef, useState } from 'react';
import { importLibrary, setOptions } from '@googlemaps/js-api-loader';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { getCouriers } from '../../api/couriers';
import type { Courier } from '../../types/courier';

const API_KEY = import.meta.env.VITE_GOOGLE_MAPS_API_KEY as string | undefined;
const REFRESH_INTERVAL_MS = 20_000;
const STALE_AFTER_MS = 5 * 60_000;
const TURKEY_CENTER = { lat: 39.0, lng: 35.0 };

function timeAgo(iso: string): string {
  const diffMs = Date.now() - new Date(iso).getTime();
  const minutes = Math.floor(diffMs / 60_000);
  if (minutes < 1) return 'az önce';
  if (minutes < 60) return `${minutes} dk önce`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} sa önce`;
  return `${Math.floor(hours / 24)} gün önce`;
}

export function CourierMapPage() {
  const mapDivRef = useRef<HTMLDivElement | null>(null);
  const mapRef = useRef<google.maps.Map | null>(null);
  const markersRef = useRef<google.maps.marker.AdvancedMarkerElement[]>([]);
  const [couriers, setCouriers] = useState<Courier[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [mapsError, setMapsError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!API_KEY) {
      setMapsError('Google Maps API anahtarı ayarlanmamış (VITE_GOOGLE_MAPS_API_KEY).');
      setIsLoading(false);
      return;
    }

    let cancelled = false;
    setOptions({ key: API_KEY, v: 'weekly' });

    Promise.all([importLibrary('maps'), importLibrary('marker')])
      .then(([{ Map }]) => {
        if (cancelled || !mapDivRef.current) return;
        mapRef.current = new Map(mapDivRef.current, {
          center: TURKEY_CENTER,
          zoom: 6,
          mapId: 'ASLSU_COURIER_MAP',
        });
        // Map can measure a stale 0-size container on first paint (it's
        // nested under a Paper/overlay whose layout may not have settled
        // yet); forcing a resize on the next frame makes it re-measure and
        // start loading tiles instead of staying blank forever.
        requestAnimationFrame(() => {
          if (cancelled || !mapRef.current) return;
          google.maps.event.trigger(mapRef.current, 'resize');
          mapRef.current.setCenter(TURKEY_CENTER);
        });
      })
      .catch(() => {
        if (!cancelled) setMapsError('Google Maps yüklenemedi. API anahtarını kontrol edin.');
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    let cancelled = false;

    function refresh() {
      getCouriers()
        .then((data) => {
          if (!cancelled) setCouriers(data);
        })
        .catch((e) => {
          if (!cancelled) setError(e instanceof Error ? e.message : 'Kuryeler yüklenemedi.');
        });
    }

    refresh();
    const interval = setInterval(refresh, REFRESH_INTERVAL_MS);
    return () => {
      cancelled = true;
      clearInterval(interval);
    };
  }, []);

  useEffect(() => {
    if (!mapRef.current || !window.google?.maps?.marker) return;

    markersRef.current.forEach((m) => (m.map = null));
    markersRef.current = [];

    const located = couriers.filter((c) => c.latitude != null && c.longitude != null);

    located.forEach((courier) => {
      const isStale = courier.locationUpdatedAt
        ? Date.now() - new Date(courier.locationUpdatedAt).getTime() > STALE_AFTER_MS
        : true;

      const pin = new google.maps.marker.PinElement({
        background: isStale ? '#9aa3ad' : '#0b6e99',
        borderColor: '#ffffff',
        glyphColor: '#ffffff',
      });

      const marker = new google.maps.marker.AdvancedMarkerElement({
        map: mapRef.current,
        position: { lat: courier.latitude!, lng: courier.longitude! },
        title: courier.name,
        content: pin.element,
      });

      const infoWindow = new google.maps.InfoWindow({
        content: `<div style="font-family:system-ui,sans-serif;font-size:13px">
          <strong>${courier.name}</strong><br/>
          ${courier.phone ?? ''}<br/>
          <span style="color:${isStale ? '#b5730c' : '#1f9254'}">
            ${courier.locationUpdatedAt ? timeAgo(courier.locationUpdatedAt) : 'konum yok'}
          </span>
        </div>`,
      });

      marker.addListener('click', () => infoWindow.open({ map: mapRef.current, anchor: marker }));
      markersRef.current.push(marker);
    });
  }, [couriers]);

  const located = couriers.filter((c) => c.latitude != null && c.longitude != null);
  const notLocated = couriers.filter((c) => c.latitude == null || c.longitude == null);

  return (
    <Box>
      <Typography variant="h1" gutterBottom>
        Kurye Haritası
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {mapsError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {mapsError}
        </Alert>
      )}

      <Stack direction="row" spacing={1} sx={{ mb: 2 }} flexWrap="wrap">
        <Chip size="small" label={`${located.length} kurye konumda`} color="primary" variant="outlined" />
        {notLocated.length > 0 && (
          <Chip size="small" label={`${notLocated.length} konum bildirmiyor`} variant="outlined" />
        )}
      </Stack>

      <Paper variant="outlined" sx={{ position: 'relative', height: 560, overflow: 'hidden' }}>
        {isLoading && (
          <Box
            sx={{
              position: 'absolute',
              inset: 0,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: 'background.paper',
              zIndex: 1,
            }}
          >
            <CircularProgress size={28} />
          </Box>
        )}
        <Box ref={mapDivRef} sx={{ width: '100%', height: '100%' }} />
      </Paper>

      {!isLoading && !mapsError && located.length === 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          Henüz konum bildiren bir kurye yok. Kuryeler mobil uygulamaya giriş yapıp konum iznini
          verdiğinde burada görünecekler.
        </Typography>
      )}
    </Box>
  );
}
