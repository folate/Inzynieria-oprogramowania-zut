<script lang="ts">
  import { onMount } from 'svelte';
  import { riderService, rideService } from '$lib/api';
  import { authStore } from '$lib/auth';
  import { goto } from '$app/navigation';

  let currentRider: any = null;
  let loading: boolean = false;
  let error: string | null = null;
  let success: string | null = null;

  // Rider data
  let isAvailable: boolean = false;
  let currentLatitude: number = 0;
  let currentLongitude: number = 0;
  let rideHistory: any[] = [];
  let pendingRides: any[] = [];
  let availableRides: any[] = []; // Rides available to accept
  let statistics: any = null;

  // Profile form
  let showProfileForm: boolean = false;
  let profileForm = {
    name: '',
    surname: '',
    phoneNumber: '',
    email: ''
  };

  onMount(() => {
    // Check if user is logged in as rider
    const unsubscribe = authStore.subscribe(state => {
      if (!state.isAuthenticated || state.userType !== 'rider') {
        goto('/rider/login');
        return;
      }
      
      currentRider = state.rider;
      if (currentRider) {
        loadRiderData();
      }
    });
    
    return unsubscribe;
  });

  async function loadRiderData() {
    loading = true;
    error = null;

    try {
      // Load profile
      const profile = await riderService.getProfile(currentRider.id);
      currentRider = { ...currentRider, ...profile };
      
      // Load statistics
      const stats = await riderService.getStatistics(currentRider.id);
      statistics = stats;
      
      // Load ride history
      rideHistory = await riderService.getRideHistory(currentRider.id);
      
      // Load pending rides
      pendingRides = await riderService.getPendingRides(currentRider.id);
      
      // Load available rides
      availableRides = await riderService.getAvailableRides(currentRider.id);
      
      // Set current availability
      isAvailable = currentRider.isAvailable || false;
      
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd ładowania danych';
    } finally {
      loading = false;
    }
  }

  async function updateAvailability() {
    if (!currentRider) return;
    
    loading = true;
    error = null;
    
    try {
      await riderService.updateAvailability(
        currentRider.id,
        isAvailable,
        currentLatitude,
        currentLongitude
      );
      
      success = `Status dostępności zaktualizowany: ${isAvailable ? 'Dostępny' : 'Niedostępny'}`;
      
      // Update current rider data
      currentRider.isAvailable = isAvailable;
      
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd aktualizacji dostępności';
    } finally {
      loading = false;
    }
  }

  async function updateLocation() {
    if (!currentRider) return;
    
    try {
      // Get current location from browser
      if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
          async (position) => {
            currentLatitude = position.coords.latitude;
            currentLongitude = position.coords.longitude;
            
            await riderService.updateLocation(
              currentRider.id,
              currentLatitude,
              currentLongitude
            );
            
            success = 'Lokalizacja zaktualizowana';
          },
          (geolocationError) => {
            console.error('Geolocation error:', geolocationError);
            error = 'Nie udało się pobrać lokalizacji';
          }
        );
      } else {
        error = 'Przeglądarka nie obsługuje geolokalizacji';
      }
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd aktualizacji lokalizacji';
    }
  }

  async function updateRideStatus(rideId: number, status: string) {
    try {
      if (status === 'Accepted') {
        // Use acceptRideOffer for accepting rides
        await rideService.acceptRideOffer(rideId, currentRider.id);
        success = 'Przejazd został przyjęty';
      } else {
        // Use updateRideStatus for other status changes
        await riderService.updateRideStatus(rideId, status);
        success = `Status przejazdu zaktualizowany na: ${status}`;
      }
      await loadRiderData(); // Reload data
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd aktualizacji statusu';
    }
  }

  async function updateProfile() {
    if (!currentRider) return;
    
    loading = true;
    error = null;
    
    try {
      await riderService.updateProfile(
        currentRider.id,
        profileForm.name,
        profileForm.surname,
        profileForm.phoneNumber,
        profileForm.email
      );
      
      success = 'Profil zaktualizowany pomyślnie';
      showProfileForm = false;
      await loadRiderData(); // Reload data
      
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd aktualizacji profilu';
    } finally {
      loading = false;
    }
  }

  function openProfileForm() {
    profileForm = {
      name: currentRider.name || '',
      surname: currentRider.surname || '',
      phoneNumber: currentRider.phoneNumber || '',
      email: currentRider.email || ''
    };
    showProfileForm = true;
  }

  function formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('pl-PL');
  }

  function formatStatus(status: string | undefined | null): string {
    if (!status) return "brak";
    return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
  }

  function getStatusColor(status: string): string {
    const colorMap: { [key: string]: string } = {
      'Pending': 'bg-yellow-100 text-yellow-800',
      'Accepted': 'bg-blue-100 text-blue-800',
      'InProgress': 'bg-green-100 text-green-800',
      'Completed': 'bg-green-100 text-green-800',
      'Cancelled': 'bg-red-100 text-red-800'
    };
    return colorMap[status] || 'bg-gray-100 text-gray-800';
  }
</script>

<svelte:head>
  <title>Panel kierowcy - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-6xl">
    <!-- Header -->
    <div class="text-center mb-8">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">Panel kierowcy</h1>
      <p class="text-gray-600 dark:text-gray-300">Witaj, {currentRider?.name} {currentRider?.surname}</p>
    </div>

    {#if error}
      <div class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg p-4 mb-6">
        <p class="text-red-700 dark:text-red-200">{error}</p>
      </div>
    {/if}

    {#if success}
      <div class="bg-green-50 dark:bg-green-900/30 border border-green-200 dark:border-green-700 rounded-lg p-4 mb-6">
        <p class="text-green-700 dark:text-green-200">{success}</p>
      </div>
    {/if}

    {#if loading}
      <div class="text-center py-8">
        <div class="uber-skeleton w-32 h-8 block mx-auto mb-2"></div>
        <p class="text-gray-500 dark:text-gray-400">Ładowanie...</p>
      </div>
    {:else}
      <!-- Quick Actions -->
      <div class="grid md:grid-cols-2 gap-6 mb-8">
        <!-- Availability Card -->
        <div class="uber-card">
          <h2 class="text-xl font-semibold mb-4 dark:text-white">Status dostępności</h2>
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <span class="text-gray-700 dark:text-gray-300">Dostępny:</span>
              <label class="relative inline-flex items-center cursor-pointer">
                <input type="checkbox" bind:checked={isAvailable} class="sr-only peer">
                <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
              </label>
            </div>
            <button
              on:click={updateAvailability}
              disabled={loading}
              class="uber-btn w-full"
            >
              {loading ? 'Aktualizowanie...' : 'Zaktualizuj status'}
            </button>
            <button
              on:click={updateLocation}
              class="uber-btn w-full bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600"
            >
              Aktualizuj lokalizację
            </button>
          </div>
        </div>

        <!-- Statistics Card -->
        <div class="uber-card">
          <h2 class="text-xl font-semibold mb-4 dark:text-white">Statystyki</h2>
          {#if statistics}
            <div class="grid grid-cols-2 gap-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-blue-600 dark:text-blue-400">{statistics.TotalRides || 0}</div>
                <div class="text-sm text-gray-600 dark:text-gray-400">Wszystkie przejazdy</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-green-600 dark:text-green-400">{statistics.TotalEarnings?.toFixed(2) || '0.00'} zł</div>
                <div class="text-sm text-gray-600 dark:text-gray-400">Łączne zarobki</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-yellow-600 dark:text-yellow-400">{statistics.AverageRating?.toFixed(1) || '0.0'}</div>
                <div class="text-sm text-gray-600 dark:text-gray-400">Średnia ocena</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-purple-600 dark:text-purple-400">{statistics.CompletedRides || 0}</div>
                <div class="text-sm text-gray-600 dark:text-gray-400">Zakończone</div>
              </div>
            </div>
          {:else}
            <p class="text-gray-500 dark:text-gray-400">Brak danych statystycznych</p>
          {/if}
        </div>
      </div>

      <!-- Available Rides to Accept -->
      {#if availableRides.length > 0}
        <div class="uber-card mb-8">
          <h2 class="text-xl font-semibold mb-4 dark:text-white">Dostępne przejazdy do przyjęcia</h2>
          <div class="space-y-4">
            {#each availableRides as ride}
              <div class="border border-gray-200 dark:border-gray-700 rounded-lg p-4">
                <div class="flex items-center justify-between">
                  <div>
                    <h3 class="font-medium dark:text-white">{ride.pickupLocation} → {ride.dropoffLocation}</h3>
                    <p class="text-sm text-gray-600 dark:text-gray-400">{formatDate(ride.orderTime)}</p>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Status: {formatStatus(ride.status)}</p>
                  </div>
                  <div class="flex space-x-2">
                    <button
                      on:click={() => updateRideStatus(ride.id, 'Accepted')}
                      class="uber-btn bg-green-600 hover:bg-green-700"
                    >
                      Przyjmij
                    </button>
                  </div>
                </div>
              </div>
            {/each}
          </div>
        </div>
      {/if}

      <!-- Pending Rides (Already Accepted) -->
      {#if pendingRides.length > 0}
        <div class="uber-card mb-8">
          <h2 class="text-xl font-semibold mb-4 dark:text-white">Twoje aktywne przejazdy</h2>
          <div class="space-y-4">
            {#each pendingRides as ride}
              <div class="border border-gray-200 dark:border-gray-700 rounded-lg p-4">
                <div class="flex items-center justify-between">
                  <div>
                    <h3 class="font-medium dark:text-white">{ride.pickupLocation} → {ride.dropoffLocation}</h3>
                    <p class="text-sm text-gray-600 dark:text-gray-400">{formatDate(ride.orderTime)}</p>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Cena: {ride.price?.toFixed(2) || '0.00'} zł</p>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Status: {formatStatus(ride.status)}</p>
                  </div>
                  <div class="flex space-x-2">
                    {#if ride.status === 'Accepted'}
                      <button
                        on:click={() => updateRideStatus(ride.id, 'InProgress')}
                        class="uber-btn bg-blue-600 hover:bg-blue-700"
                      >
                        Rozpocznij
                      </button>
                    {:else if ride.status === 'InProgress'}
                      <button
                        on:click={() => updateRideStatus(ride.id, 'Completed')}
                        class="uber-btn bg-green-600 hover:bg-green-700"
                      >
                        Zakończ
                      </button>
                    {/if}
                  </div>
                </div>
              </div>
            {/each}
          </div>
        </div>
      {/if}

      <!-- Recent Ride History -->
      <div class="uber-card mb-8">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-xl font-semibold dark:text-white">Ostatnie przejazdy</h2>
          <button
            on:click={openProfileForm}
            class="uber-btn"
          >
            Edytuj profil
          </button>
        </div>
        {#if rideHistory.length > 0}
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead>
                <tr class="border-b border-gray-200 dark:border-gray-700">
                  <th class="text-left py-2 text-gray-700 dark:text-gray-300">Data</th>
                  <th class="text-left py-2 text-gray-700 dark:text-gray-300">Trasa</th>
                  <th class="text-left py-2 text-gray-700 dark:text-gray-300">Status</th>
                  <th class="text-left py-2 text-gray-700 dark:text-gray-300">Cena</th>
                  <th class="text-left py-2 text-gray-700 dark:text-gray-300">Ocena</th>
                </tr>
              </thead>
              <tbody>
                {#each rideHistory.slice(0, 10) as ride}
                  <tr class="border-b border-gray-100 dark:border-gray-800">
                    <td class="py-2 text-sm text-gray-600 dark:text-gray-400">{formatDate(ride.orderTime)}</td>
                    <td class="py-2 text-sm dark:text-white">
                      {ride.pickupLocation} → {ride.dropoffLocation}
                    </td>
                    <td class="py-2">
                      <span class="px-2 py-1 text-xs rounded-full {getStatusColor(ride.status)}">
                        {formatStatus(ride.status)}
                      </span>
                    </td>
                    <td class="py-2 text-sm dark:text-white">{ride.price?.toFixed(2) || '0.00'} zł</td>
                    <td class="py-2 text-sm dark:text-white">
                      {ride.rating ? `⭐ ${ride.rating}` : 'Brak oceny'}
                    </td>
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        {:else}
          <p class="text-gray-500 dark:text-gray-400">Brak historii przejazdów</p>
        {/if}
      </div>
    {/if}
  </div>
</div>

<!-- Profile Edit Modal -->
{#if showProfileForm}
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 flex justify-center items-center z-50">
    <div class="bg-white dark:bg-[#232329] rounded-xl shadow-lg p-6 max-w-md w-full mx-4">
      <h2 class="text-xl font-bold mb-4 dark:text-white">Edytuj profil</h2>
      <form on:submit|preventDefault={updateProfile} class="space-y-4">
        <div>
          <label for="name" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Imię</label>
          <input
            id="name"
            type="text"
            bind:value={profileForm.name}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
          />
        </div>
        <div>
          <label for="surname" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nazwisko</label>
          <input
            id="surname"
            type="text"
            bind:value={profileForm.surname}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
          />
        </div>
        <div>
          <label for="phone" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Telefon</label>
          <input
            id="phone"
            type="tel"
            bind:value={profileForm.phoneNumber}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
          />
        </div>
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Email</label>
          <input
            id="email"
            type="email"
            bind:value={profileForm.email}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
          />
        </div>
        <div class="flex gap-2">
          <button
            type="submit"
            disabled={loading}
            class="uber-btn flex-1"
          >
            {loading ? 'Zapisywanie...' : 'Zapisz'}
          </button>
          <button
            type="button"
            on:click={() => showProfileForm = false}
            class="uber-btn flex-1 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600"
          >
            Anuluj
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}
