<script lang="ts">
  import { onMount } from 'svelte';
  import { createEventDispatcher } from 'svelte';

  const dispatch = createEventDispatcher();

  export let pickupLocation: any = null;
  export let dropoffLocation: any = null;
  export let showRoute: boolean = false;

  let map: any;
  let mapContainer: any;
  let pickupMarker: any = null;
  let dropoffMarker: any = null;
  let routeLayer: any = null;

  // Default center - Kraków
  const defaultCenter = [50.0647, 19.9450];
  
  onMount(() => {
    initializeMap();
  });

  async function initializeMap() {
    try {
      // Dynamically import Leaflet
      const L = await import('leaflet');
      
      // Initialize map
      map = L.default.map(mapContainer, {
        center: defaultCenter,
        zoom: 13
      });

      // Add tile layer
      L.default.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        crossOrigin: true
      }).addTo(map);

      // Add click event listener
      map.on('click', handleMapClick);

      // Set initial markers if locations are provided
      if (pickupLocation) {
        await updatePickupMarker(pickupLocation);
      }
      if (dropoffLocation) {
        await updateDropoffMarker(dropoffLocation);
      }
    } catch (error) {
      console.error('Map initialization error:', error);
    }
  }

  async function handleMapClick(e: any) {
    const { lat, lng } = e.latlng;
    
    try {
      // Reverse geocode to get address
      const address = await reverseGeocode(lat, lng);
      
      // Dispatch event with coordinates and address
      dispatch('locationSelected', {
        coordinates: [lat, lng],
        address: address
      });
    } catch (error) {
      console.error('Geocoding error:', error);
      dispatch('locationSelected', {
        coordinates: [lat, lng],
        address: `${lat.toFixed(6)}, ${lng.toFixed(6)}`
      });
    }
  }

  async function reverseGeocode(lat: any, lng: any) {
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`
      );
      
      if (!response.ok) {
        throw new Error('Geocoding failed');
      }
      
      const data = await response.json();
      return data.display_name || `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
    } catch (error) {
      console.error('Reverse geocoding error:', error);
      return `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
    }
  }

  export async function geocodeAddress(address: any) {
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&limit=1&countrycodes=pl`
      );
      
      if (!response.ok) {
        throw new Error('Geocoding failed');
      }
      
      const data = await response.json();
      if (data.length > 0) {
        const result = data[0];
        return {
          coordinates: [parseFloat(result.lat), parseFloat(result.lon)],
          address: result.display_name,
          valid: true
        };
      } else {
        return {
          coordinates: null,
          address: null,
          valid: false
        };
      }
    } catch (error) {
      console.error('Geocoding error:', error);
      return {
        coordinates: null,
        address: null,
        valid: false
      };
    }
  }

  export async function updatePickupMarker(location: any) {
    if (!map) return;
    
    const L = await import('leaflet');
    
    // Remove existing marker
    if (pickupMarker) {
      map.removeLayer(pickupMarker);
    }

    if (location && location.coordinates) {
      const [lat, lng] = location.coordinates;
      
      // Create green marker for pickup
      const greenIcon = L.default.icon({
        iconUrl: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjQiIGhlaWdodD0iMjQiIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTEyIDJDOC4xMyAyIDUgNS4xMyA1IDlDNSAxNC4yNSAxMiAyMiAxMiAyMkMxMiAyMiAxOSAxNC4yNSAxOSA5QzE5IDUuMTMgMTUuODcgMiAxMiAyWk0xMiAxMS41QzEwLjYyIDExLjUgOS41IDEwLjM4IDkuNSA5QzkuNSA3LjYyIDEwLjYyIDYuNSAxMiA2LjVDMTMuMzggNi41IDE0LjUgNy42MiAxNC41IDlDMTQuNSAxMC4zOCAxMy4zOCAxMS41IDEyIDExLjVaIiBmaWxsPSIjMTBCOTgxIi8+Cjwvc3ZnPgo=',
        iconSize: [24, 24],
        iconAnchor: [12, 24],
        popupAnchor: [0, -24]
      });

      pickupMarker = L.default.marker([lat, lng], { icon: greenIcon })
        .addTo(map)
        .bindPopup(`<b>Miejsce odbioru:</b><br>${location.address}`);
      
      // Update route if both locations are set
      if (dropoffLocation && dropoffLocation.coordinates) {
        await updateRoute();
      }
    }
  }

  export async function updateDropoffMarker(location: any) {
    if (!map) return;
    
    const L = await import('leaflet');
    
    // Remove existing marker
    if (dropoffMarker) {
      map.removeLayer(dropoffMarker);
    }

    if (location && location.coordinates) {
      const [lat, lng] = location.coordinates;
      
      // Create red marker for dropoff
      const redIcon = L.default.icon({
        iconUrl: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjQiIGhlaWdodD0iMjQiIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTEyIDJDOC4xMyAyIDUgNS4xMyA1IDlDNSAxNC4yNSAxMiAyMiAxMiAyMkMxMiAyMiAxOSAxNC4yNSAxOSA5QzE5IDUuMTMgMTUuODcgMiAxMiAyWk0xMiAxMS41QzEwLjYyIDExLjUgOS41IDEwLjM4IDkuNSA5QzkuNSA3LjYyIDEwLjYyIDYuNSAxMiA2LjVDMTMuMzggNi41IDE0LjUgNy42MiAxNC41IDlDMTQuNSAxMC4zOCAxMy4zOCAxMS41IDEyIDExLjVaIiBmaWxsPSIjRUY0NDQ0Ii8+Cjwvc3ZnPgo=',
        iconSize: [24, 24],
        iconAnchor: [12, 24],
        popupAnchor: [0, -24]
      });

      dropoffMarker = L.default.marker([lat, lng], { icon: redIcon })
        .addTo(map)
        .bindPopup(`<b>Miejsce docelowe:</b><br>${location.address}`);
      
      // Update route if both locations are set
      if (pickupLocation && pickupLocation.coordinates) {
        await updateRoute();
      }
    }
  }

  async function updateRoute() {
    if (!map || !pickupLocation?.coordinates || !dropoffLocation?.coordinates) return;
    
    const L = await import('leaflet');
    
    // Remove existing route
    if (routeLayer) {
      map.removeLayer(routeLayer);
    }

    try {
      const [pickupLat, pickupLng] = pickupLocation.coordinates;
      const [dropoffLat, dropoffLng] = dropoffLocation.coordinates;
      
      // Get route from OSRM
      const response = await fetch(
        `https://router.project-osrm.org/route/v1/driving/${pickupLng},${pickupLat};${dropoffLng},${dropoffLat}?overview=full&geometries=geojson`
      );
      
      if (!response.ok) {
        throw new Error('Route calculation failed');
      }
      
      const data = await response.json();
      
      if (data.routes && data.routes.length > 0) {
        const route = data.routes[0];
        const coordinates = route.geometry.coordinates.map(coord => [coord[1], coord[0]]);
        
        // Create route polyline
        routeLayer = L.default.polyline(coordinates, {
          color: '#3B82F6',
          weight: 4,
          opacity: 0.7
        }).addTo(map);
        
        // Fit map to show both markers and route
        const group = L.default.featureGroup([pickupMarker, dropoffMarker, routeLayer]);
        map.fitBounds(group.getBounds().pad(0.1));
        
        // Dispatch route information
        dispatch('routeCalculated', {
          distance: route.distance,
          duration: route.duration,
          geometry: route.geometry
        });
      }
    } catch (error) {
      console.error('Route calculation error:', error);
      
      // Fallback: just fit bounds to markers
      if (pickupMarker && dropoffMarker) {
        const group = L.default.featureGroup([pickupMarker, dropoffMarker]);
        map.fitBounds(group.getBounds().pad(0.1));
      }
    }
  }

  // Reactive updates
  $: if (pickupLocation && map) {
    updatePickupMarker(pickupLocation);
  }
  
  $: if (dropoffLocation && map) {
    updateDropoffMarker(dropoffLocation);
  }
</script>

<style>
  .map-container {
    width: 100%;
    height: 400px;
    border-radius: 8px;
    overflow: hidden;
  }

  :global(.leaflet-container) {
    font-family: inherit;
  }
</style>

<div class="map-container" bind:this={mapContainer}></div>
