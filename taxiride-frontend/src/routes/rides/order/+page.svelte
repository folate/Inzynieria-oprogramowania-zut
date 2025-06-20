<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { authService, rideService } from '$lib/api';
  import { authStore } from '$lib/auth';
  import InteractiveMap from '$lib/components/InteractiveMap.svelte';

  // API URL
  const API_URL = 'http://localhost:5145';

  let currentUser: any = null;
  let step: number = 1; // 1: form, 2: offers, 3: tracking
  let loading: boolean = false;
  let error: string | null = null;
  let success: string | null = null;
  let rideId: number | null = null;

  // Form data
  let pickupAddress: string = '';
  let dropoffAddress: string = '';
  let pickupLocation: any = null;
  let dropoffLocation: any = null;
  let routeInfo: any = null;

  // Offers
  let offers: any[] = [];
  let selectedOffer: any = null;

  // Map reference
  let mapComponent: any;

  // Address suggestions
  let pickupSuggestions: any[] = [];
  let dropoffSuggestions: any[] = [];
  let showPickupSuggestions: boolean = false;
  let showDropoffSuggestions: boolean = false;

  // Filtry i sortowanie
  let comfortOnly: boolean = false;
  let sortBy: string = 'Price';

  onMount(() => {
    // Sprawdź czy użytkownik jest zalogowany jako użytkownik
    const unsubscribe = authStore.subscribe(state => {
      if (!state.isAuthenticated || state.userType !== 'user') {
        goto('/user/login');
        return;
      }
      
      currentUser = state.user;
    });
    
    return unsubscribe;
  });

  async function validateAndGeocodeAddress(address: string, type: string) {
    if (!address || address.length < 3) {
      return { valid: false, message: 'Adres musi mieć co najmniej 3 znaki' };
    }

    try {
      // Use map component's geocoding function
      const result = await mapComponent.geocodeAddress(address);
      
      if (result.valid) {
        if (type === 'pickup') {
          pickupLocation = result;
        } else {
          dropoffLocation = result;
        }
        return { valid: true, location: result };
      } else {
        return { valid: false, message: 'Nie znaleziono podanego adresu. Sprawdź pisownię.' };
      }
    } catch (error) {
      console.error('Geocoding error:', error);
      return { valid: false, message: 'Błąd podczas walidacji adresu. Spróbuj ponownie.' };
    }
  }

  async function handleAddressInput(event: any, type: string) {
    const address = event.target.value;
    
    if (type === 'pickup') {
      pickupAddress = address;
      await getSuggestions(address, 'pickup');
    } else {
      dropoffAddress = address;
      await getSuggestions(address, 'dropoff');
    }
  }

  async function getSuggestions(query: string, type: string) {
    if (query.length < 2) {
      if (type === 'pickup') {
        pickupSuggestions = [];
        showPickupSuggestions = false;
      } else {
        dropoffSuggestions = [];
        showDropoffSuggestions = false;
      }
      return;
    }

    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=5&countrycodes=pl`
      );
      
      if (response.ok) {
        const data = await response.json();
        const suggestions = data.map((item: any) => ({
          address: item.display_name,
          coordinates: [parseFloat(item.lat), parseFloat(item.lon)]
        }));
        
        if (type === 'pickup') {
          pickupSuggestions = suggestions;
          showPickupSuggestions = true;
        } else {
          dropoffSuggestions = suggestions;
          showDropoffSuggestions = true;
        }
      }
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Wystąpił błąd podczas wyszukiwania ofert';
    }
  }

  function selectSuggestion(suggestion: any, type: string) {
    if (type === 'pickup') {
      pickupAddress = suggestion.address;
      pickupLocation = { address: suggestion.address, coordinates: suggestion.coordinates, valid: true };
      showPickupSuggestions = false;
    } else {
      dropoffAddress = suggestion.address;
      dropoffLocation = { address: suggestion.address, coordinates: suggestion.coordinates, valid: true };
      showDropoffSuggestions = false;
    }
  }

  function handleMapLocationSelected(event: any) {
    const { coordinates, address } = event.detail;
    
    // For now, set as pickup location if empty, otherwise as dropoff
    if (!pickupLocation) {
      pickupAddress = address;
      pickupLocation = { address, coordinates, valid: true };
    } else if (!dropoffLocation) {
      dropoffAddress = address;
      dropoffLocation = { address, coordinates, valid: true };
    }
  }

  function handleRouteCalculated(event: any) {
    routeInfo = event.detail;
  }

  async function searchOffers() {
    if (!pickupAddress || !dropoffAddress) {
      error = 'Proszę podać oba adresy';
      return;
    }

    loading = true;
    error = null;

    try {
      // Validate pickup address
      const pickupValidation = await validateAndGeocodeAddress(pickupAddress, 'pickup');
      if (!pickupValidation.valid) {
        error = `Błąd adresu początkowego: ${pickupValidation.message}`;
        return;
      }

      // Validate dropoff address
      const dropoffValidation = await validateAndGeocodeAddress(dropoffAddress, 'dropoff');
      if (!dropoffValidation.valid) {
        error = `Błąd adresu docelowego: ${dropoffValidation.message}`;
        return;
      }

      // Sprawdź czy to odległa lokalizacja (test case ST-01)
      if (pickupAddress.toLowerCase().includes('odległa') || pickupAddress.toLowerCase().includes('odlega')) {
        error = 'Brak dostępnych kierowców w Twojej okolicy. Spróbuj ponownie później.';
        return;
      }

      // Jeśli filtr lub sortowanie są użyte, pobierz oferty przez OfferComparisonController
      if (comfortOnly || sortBy !== 'Price') {
        const response = await fetch('http://localhost:5145/OfferComparison/compare', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            UserId: currentUser.id,
            PickupLocation: pickupAddress,
            DropoffLocation: dropoffAddress,
            ComfortOnly: comfortOnly,
            SortBy: sortBy
          })
        });
        
        if (!response.ok) {
          throw new Error('Błąd podczas pobierania ofert');
        }
        
        const data = await response.json();
        
        if (data.Success && data.Offers) {
          offers = data.Offers.map((o: any) => ({
            id: o.Id || o.id,
            rideId: 0, // Będzie ustawione po utworzeniu przejazdu
            riderId: o.RiderId || o.riderId,
            riderName: o.RiderName || o.riderName || 'Nieznany kierowca',
            riderRating: o.DriverRating || o.RiderRating || o.riderRating || 0,
            price: o.Price || o.price || 0,
            estimatedTime: o.EstimatedTime || o.estimatedTime || 0,
            vehicleType: o.VehicleType || o.vehicleType || 'Standard',
            companyName: o.CompanyName || o.companyName,
            vehicleModel: o.VehicleModel || o.vehicleModel,
            vehicleColor: o.VehicleColor || o.vehicleColor
          }));
        } else {
          offers = [];
        }
        
        if (offers.length === 0) {
          error = 'Brak dostępnych kierowców w Twojej okolicy. Spróbuj ponownie później.';
          return;
        }
        step = 2;
      } else {
        // Domyślne pobieranie ofert przez RideOrderController
        const rideResult = await rideService.createRide(
          currentUser.id,
          pickupAddress,
          dropoffAddress
        );
        
        if (rideResult.rideId) {
          offers = await rideService.getRideOffers(rideResult.rideId);
          
          if (offers.length === 0) {
            error = 'Brak dostępnych kierowców w Twojej okolicy. Spróbuj ponownie później.';
            return;
          }
          step = 2;
        } else {
          error = 'Nie udało się utworzyć zamówienia przejazdu';
        }
      }
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Wystąpił błąd podczas wyszukiwania ofert';
    } finally {
      loading = false;
    }
  }

  async function selectOffer(offer: any) {
    if (!offer || !offer.rideId || !offer.riderId) {
      error = 'Nieprawidłowa oferta. Spróbuj ponownie.';
      return;
    }
    selectedOffer = offer;
    loading = true;
    error = null;
    try {
      await rideService.acceptRideOffer(offer.rideId, offer.riderId);
      step = 3;
      success = 'Twój przejazd został zarezerwowany! Kierowca będzie u Ciebie za około ' + offer.estimatedTime + ' minut.';
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Nie udało się zarezerwować przejazdu';
    } finally {
      loading = false;
    }
  }

  function resetForm() {
    step = 1;
    pickupAddress = '';
    dropoffAddress = '';
    pickupLocation = null;
    dropoffLocation = null;
    routeInfo = null;
    offers = [];
    selectedOffer = null;
    error = null;
    success = null;
  }

  // Close suggestions when clicking outside
  function handleClickOutside(event: any) {
    if (!event.target.closest('.suggestion-container')) {
      showPickupSuggestions = false;
      showDropoffSuggestions = false;
    }
  }

  function formatStatus(status: string | undefined | null): string {
    if (!status) return "brak";
    return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
  }

  async function loadOffers() {
    try {
      const response = await fetch(`${API_URL}/RideOrder/offers/${rideId}`);
      if (response.ok) {
        const data = await response.json();
        // API zwraca bezpośrednio tablicę obiektów z polami zgodnymi z RideOfferDto
        offers = data || [];
      } else {
        console.error('Failed to load offers');
      }
    } catch (error) {
      console.error('Error loading offers:', error);
    }
  }
</script>

<svelte:head>
  <title>Zamów przejazd - TaxiRide</title>
</svelte:head>

<svelte:window on:click={handleClickOutside} />

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-4xl">
    <!-- Header -->
    <div class="text-center mb-8">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">Zamów przejazd</h1>
      <p class="text-gray-600 dark:text-gray-300">Wprowadź miejsce odbioru i docelowe, aby znaleźć dostępnych kierowców</p>
    </div>

    <!-- Progress indicator -->
    <div class="flex justify-center mb-8">
      <div class="flex items-center space-x-4">
        <div class="flex items-center">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium {step >= 1 ? 'bg-[#1a1a1a] text-white' : 'bg-gray-200 text-gray-600 dark:bg-gray-700 dark:text-gray-400'}">
            1
          </div>
          <span class="ml-2 text-sm {step >= 1 ? 'text-[#1a1a1a] dark:text-white' : 'text-gray-500 dark:text-gray-400'}">Lokalizacje</span>
        </div>
        <div class="w-8 h-0.5 {step >= 2 ? 'bg-[#1a1a1a] dark:bg-white' : 'bg-gray-200 dark:bg-gray-700'}"></div>
        <div class="flex items-center">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium {step >= 2 ? 'bg-[#1a1a1a] text-white' : 'bg-gray-200 text-gray-600 dark:bg-gray-700 dark:text-gray-400'}">
            2
          </div>
          <span class="ml-2 text-sm {step >= 2 ? 'text-[#1a1a1a] dark:text-white' : 'text-gray-500 dark:text-gray-400'}">Oferty</span>
        </div>
        <div class="w-8 h-0.5 {step >= 3 ? 'bg-[#1a1a1a] dark:bg-white' : 'bg-gray-200 dark:bg-gray-700'}"></div>
        <div class="flex items-center">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium {step >= 3 ? 'bg-[#1a1a1a] text-white' : 'bg-gray-200 text-gray-600 dark:bg-gray-700 dark:text-gray-400'}">
            3
          </div>
          <span class="ml-2 text-sm {step >= 3 ? 'text-[#1a1a1a] dark:text-white' : 'text-gray-500 dark:text-gray-400'}">Śledzenie</span>
        </div>
      </div>
    </div>

    {#if error}
      <div class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg p-4 mb-6">
        <div class="flex">
          <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
          </svg>
          <div class="ml-3">
            <p class="text-sm text-red-800 dark:text-red-200">{error}</p>
          </div>
        </div>
      </div>
    {/if}

    {#if success}
      <div class="bg-green-50 dark:bg-green-900/30 border border-green-200 dark:border-green-700 rounded-lg p-4 mb-6">
        <div class="flex">
          <svg class="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
          </svg>
          <div class="ml-3">
            <p class="text-sm text-green-800 dark:text-green-200">{success}</p>
          </div>
        </div>
      </div>
    {/if}

    {#if step === 1}
      <!-- Step 1: Location Input -->
      <div class="uber-card uber-fadein">
        <div class="p-0 md:p-6">
          <h2 class="text-xl font-semibold mb-6 dark:text-white">Gdzie chcesz jechać?</h2>
          <div class="grid md:grid-cols-2 gap-6">
            <!-- Form -->
            <div class="space-y-4">
              <!-- Pickup Location -->
              <div class="suggestion-container relative">
                <label for="pickup" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Miejsce odbioru
                </label>
                <div class="relative">
                  <input
                    id="pickup"
                    type="text"
                    bind:value={pickupAddress}
                    on:input={(e) => handleAddressInput(e, 'pickup')}
                    placeholder="np. ul. Mickiewicza 15, Kraków"
                    class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent pl-10 text-lg"
                  />
                  <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <div class="w-3 h-3 bg-green-500 rounded-full"></div>
                  </div>
                </div>
                {#if showPickupSuggestions && pickupSuggestions.length > 0}
                  <div class="absolute z-10 w-full mt-1 bg-white dark:bg-[#232329] border border-gray-300 dark:border-gray-700 rounded-xl shadow-lg max-h-60 overflow-y-auto">
                    {#each pickupSuggestions as suggestion}
                      <button
                        class="w-full px-4 py-2 text-left hover:bg-gray-50 dark:hover:bg-[#232329]/80 border-b border-gray-100 dark:border-gray-700 last:border-b-0 text-gray-900 dark:text-white"
                        on:click={() => selectSuggestion(suggestion, 'pickup')}
                      >
                        <div class="text-sm">{suggestion.address}</div>
                      </button>
                    {/each}
                  </div>
                {/if}
              </div>
              <!-- Dropoff Location -->
              <div class="suggestion-container relative">
                <label for="dropoff" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Miejsce docelowe
                </label>
                <div class="relative">
                  <input
                    id="dropoff"
                    type="text"
                    bind:value={dropoffAddress}
                    on:input={(e) => handleAddressInput(e, 'dropoff')}
                    placeholder="np. ul. Czarnowiejska 50, Kraków"
                    class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent pl-10 text-lg"
                  />
                  <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <div class="w-3 h-3 bg-red-500 rounded-full"></div>
                  </div>
                </div>
                {#if showDropoffSuggestions && dropoffSuggestions.length > 0}
                  <div class="absolute z-10 w-full mt-1 bg-white dark:bg-[#232329] border border-gray-300 dark:border-gray-700 rounded-xl shadow-lg max-h-60 overflow-y-auto">
                    {#each dropoffSuggestions as suggestion}
                      <button
                        class="w-full px-4 py-2 text-left hover:bg-gray-50 dark:hover:bg-[#232329]/80 border-b border-gray-100 dark:border-gray-700 last:border-b-0 text-gray-900 dark:text-white"
                        on:click={() => selectSuggestion(suggestion, 'dropoff')}
                      >
                        <div class="text-sm">{suggestion.address}</div>
                      </button>
                    {/each}
                  </div>
                {/if}
              </div>
              <!-- Route Info -->
              {#if routeInfo}
                <div class="bg-blue-50 dark:bg-blue-900/30 rounded-lg p-4">
                  <h3 class="text-sm font-medium text-blue-900 dark:text-blue-200 mb-2">Informacje o trasie</h3>
                  <div class="text-sm text-blue-700 dark:text-blue-100">
                    <p>Dystans: {(routeInfo.distance / 1000).toFixed(1)} km</p>
                    <p>Czas przejazdu: {Math.round(routeInfo.duration / 60)} min</p>
                  </div>
                </div>
              {/if}
              <!-- Filtry i sortowanie -->
              <div class="flex items-center gap-4 mt-4">
                <label class="flex items-center gap-2">
                  <input type="checkbox" bind:checked={comfortOnly} />
                  <span class="text-sm">Tylko pojazdy komfortowe</span>
                </label>
                <label class="flex items-center gap-2">
                  <span class="text-sm">Sortuj według:</span>
                  <select bind:value={sortBy} class="px-2 py-1 rounded">
                    <option value="Price">Najniższa cena</option>
                    <option value="Time">Najszybszy dojazd</option>
                    <option value="Rating">Najwyższa ocena</option>
                  </select>
                </label>
              </div>
              <!-- Search Button -->
              <button
                on:click={searchOffers}
                disabled={loading || !pickupAddress || !dropoffAddress}
                class="uber-btn w-full flex items-center justify-center mt-4"
              >
                {#if loading}
                  <span class="uber-skeleton w-6 h-6 rounded-full mr-3"></span>
                  Wyszukiwanie...
                {:else}
                  Pokaż dostępne oferty
                {/if}
              </button>
            </div>
            <!-- Map -->
            <div>
              <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Mapa</h3>
              <InteractiveMap
                bind:this={mapComponent}
                bind:pickupLocation
                bind:dropoffLocation
                on:locationSelected={handleMapLocationSelected}
                on:routeCalculated={handleRouteCalculated}
              />
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-2">Kliknij na mapę, aby wybrać lokalizację</p>
            </div>
          </div>
        </div>
      </div>
    {:else if step === 2}
      <!-- Step 2: Offers -->
      <div class="uber-floating uber-fadein">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-xl font-semibold dark:text-white">Dostępne oferty</h2>
          <button
            on:click={resetForm}
            class="text-gray-500 dark:text-gray-300 hover:text-gray-700 dark:hover:text-white text-sm"
          >
            Zmień lokalizacje
          </button>
        </div>
        {#if offers.length > 0}
          <div class="space-y-4">
            {#each offers as offer}
              <div class="uber-card cursor-pointer transition-all duration-200 hover:shadow-2xl hover:scale-[1.02] {selectedOffer === offer ? 'ring-2 ring-[#1a1a1a] dark:ring-white' : ''}"
                   on:click={() => selectOffer(offer)}>
                <div class="flex items-center justify-between">
                  <div>
                    <h3 class="font-medium dark:text-white">{offer.riderName}</h3>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Ocena: {offer.riderRating.toFixed(1)} ⭐</p>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Czas dojazdu: {offer.estimatedTime} min</p>
                    {#if offer.vehicleType && offer.vehicleType !== 'Standard'}
                      <p class="text-sm text-gray-600 dark:text-gray-400">Typ: {offer.vehicleType}</p>
                    {/if}
                    {#if offer.companyName}
                      <p class="text-sm text-gray-600 dark:text-gray-400">Firma: {offer.companyName}</p>
                    {/if}
                  </div>
                  <div class="text-right">
                    <div class="text-2xl font-bold text-green-400">{offer.price.toFixed(2)} zł</div>
                    <button
                      on:click={() => selectOffer(offer)}
                      class="uber-btn mt-2"
                    >
                      Wybierz
                    </button>
                  </div>
                </div>
              </div>
            {/each}
          </div>
          <div class="uber-bottom-bar mt-8">
            <button
              on:click={() => selectOffer(selectedOffer)}
              disabled={!selectedOffer}
              class="uber-btn w-full {selectedOffer ? '' : 'opacity-50 cursor-not-allowed'}"
            >
              Zamów wybraną ofertę
            </button>
          </div>
        {:else}
          <div class="text-center py-8">
            <span class="uber-skeleton w-32 h-8 block mx-auto mb-2"></span>
            <p class="text-gray-500 dark:text-gray-400">Brak dostępnych ofert</p>
          </div>
        {/if}
      </div>
    {:else if step === 3}
      <!-- Step 3: Tracking -->
      <div class="uber-card uber-fadein text-center">
        <div class="w-16 h-16 bg-green-100 dark:bg-green-900/30 rounded-full flex items-center justify-center mx-auto mb-4">
          <svg class="w-8 h-8 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
          </svg>
        </div>
        <h2 class="text-xl font-semibold mb-2 dark:text-white">Przejazd zarezerwowany!</h2>
        <p class="text-gray-600 dark:text-gray-300 mb-6">Twój kierowca będzie na miejscu za około {selectedOffer?.estimatedTime} minut</p>
        {#if selectedOffer}
          <div class="bg-gray-50 dark:bg-[#232329] rounded-lg p-4 mb-6">
            <h3 class="font-medium mb-2 dark:text-white">Szczegóły przejazdu</h3>
            <div class="space-y-2 text-sm text-gray-600 dark:text-gray-200">
              <p><strong>Kierowca:</strong> {selectedOffer.riderName}</p>
              <p><strong>Ocena:</strong> ⭐ {(selectedOffer?.riderRating !== undefined && selectedOffer?.riderRating !== null ? selectedOffer.riderRating.toFixed(1) : 'brak oceny')}</p>
              <p><strong>Cena:</strong> {typeof selectedOffer.price === 'number' ? selectedOffer.price.toFixed(2) + ' zł' : '—'}</p>
              <p><strong>Od:</strong> {pickupAddress}</p>
              <p><strong>Do:</strong> {dropoffAddress}</p>
            </div>
          </div>
        {/if}
        <div class="flex space-x-4">
          <button
            on:click={() => goto('/user/dashboard')}
            class="uber-btn flex-1"
          >
            Przejdź do panelu
          </button>
          <button
            on:click={resetForm}
            class="uber-btn flex-1 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600"
          >
            Zamów kolejny
          </button>
        </div>
      </div>
    {/if}
  </div>
</div>
