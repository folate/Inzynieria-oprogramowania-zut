<script>
  import { onMount } from 'svelte';
  import { createEventDispatcher } from 'svelte';

  const dispatch = createEventDispatcher();

  export let pickupLocation = null;
  export let dropoffLocation = null;

  let map;
  let mapContainer;
  let pickupMarker = null;
  let dropoffMarker = null;
  let routeLayer = null;
  let L; // Leaflet library

  // Default center - Kraków
  const defaultCenter = [50.0647, 19.9450];
  
  onMount(async () => {
    try {
      // Dynamically import Leaflet
      const leaflet = await import('leaflet');
      L = leaflet.default;
      
      // Initialize map
      map = L.map(mapContainer, {
        center: defaultCenter,
        zoom: 13
      });

      // Add tile layer
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
      }).addTo(map);

      // Add click event listener
      map.on('click', handleMapClick);

      // Set initial markers if locations are provided
      if (pickupLocation) {
        updatePickupMarker(pickupLocation);
      }
      if (dropoffLocation) {
        updateDropoffMarker(dropoffLocation);
      }
    } catch (error) {
      console.error('Map initialization error:', error);
    }

    return () => {
      if (map) {
        map.remove();
      }
    };
  });

  async function handleMapClick(e) {
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

  async function reverseGeocode(lat, lng) {
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

  export async function geocodeAddress(address) {
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

  function updatePickupMarker(location) {
    if (!map || !L) return;
    
    // Remove existing marker
    if (pickupMarker) {
      map.removeLayer(pickupMarker);
    }

    if (location && location.coordinates) {
      const [lat, lng] = location.coordinates;
      
      // Create green marker for pickup
      const greenIcon = L.divIcon({
        className: 'custom-marker pickup-marker',
        html: `<div style="background-color: #10B981; width: 20px; height: 20px; border-radius: 50%; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);"></div>`,
        iconSize: [20, 20],
        iconAnchor: [10, 10]
      });

      pickupMarker = L.marker([lat, lng], { icon: greenIcon })
        .addTo(map)
        .bindPopup(`<b>Miejsce odbioru:</b><br>${location.address}`);
      
      // Update route if both locations are set
      if (dropoffLocation && dropoffLocation.coordinates) {
        updateRoute();
      }
    }
  }

  function updateDropoffMarker(location) {
    if (!map || !L) return;
    
    // Remove existing marker
    if (dropoffMarker) {
      map.removeLayer(dropoffMarker);
    }

    if (location && location.coordinates) {
      const [lat, lng] = location.coordinates;
      
      // Create red marker for dropoff
      const redIcon = L.divIcon({
        className: 'custom-marker dropoff-marker',
        html: `<div style="background-color: #EF4444; width: 20px; height: 20px; border-radius: 50%; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);"></div>`,
        iconSize: [20, 20],
        iconAnchor: [10, 10]
      });

      dropoffMarker = L.marker([lat, lng], { icon: redIcon })
        .addTo(map)
        .bindPopup(`<b>Miejsce docelowe:</b><br>${location.address}`);
      
      // Update route if both locations are set
      if (pickupLocation && pickupLocation.coordinates) {
        updateRoute();
      }
    }
  }

  async function updateRoute() {
    if (!map || !L || !pickupLocation?.coordinates || !dropoffLocation?.coordinates) return;
    
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
        routeLayer = L.polyline(coordinates, {
          color: '#3B82F6',
          weight: 4,
          opacity: 0.7
        }).addTo(map);
        
        // Fit map to show both markers and route
        const group = L.featureGroup([pickupMarker, dropoffMarker, routeLayer]);
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
        const group = L.featureGroup([pickupMarker, dropoffMarker]);
        map.fitBounds(group.getBounds().pad(0.1));
      }
    }
  }

  // Reactive updates
  $: if (pickupLocation) {
    updatePickupMarker(pickupLocation);
  }
  
  $: if (dropoffLocation) {
    updateDropoffMarker(dropoffLocation);
  }
</script>

<svelte:head>
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
</svelte:head>

<div class="map-container" bind:this={mapContainer}></div>

<style>
  .map-container {
    width: 100%;
    height: 400px;
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid #e5e7eb;
  }

  :global(.leaflet-container) {
    font-family: inherit;
  }

  :global(.custom-marker) {
    background: none !important;
    border: none !important;
  }
</style>
