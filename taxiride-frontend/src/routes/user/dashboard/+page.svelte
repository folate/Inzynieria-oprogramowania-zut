<script lang="ts">
  import { onMount } from 'svelte';
  import { userService, authService } from '$lib/api';
  import { authStore } from '$lib/auth';
  import { goto } from '$app/navigation';

  // API URL
  const API_URL = 'http://localhost:5145';

  let currentUser: any = null;
  let loading: boolean = false;
  let error: string | null = null;
  let success: string | null = null;

  // User data
  let rideHistory: any[] = [];
  let statistics: any = {
    totalRides: 0,
    totalSpent: 0,
    averageRating: 0,
    completedRides: 0
  };

  // Profile form
  let showProfileForm: boolean = false;
  let profileForm = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    preferredPaymentMethod: ''
  };

  onMount(() => {
    // Sprawdź czy użytkownik jest zalogowany jako użytkownik
    const unsubscribe = authStore.subscribe(state => {
      if (!state.isAuthenticated || state.userType !== 'user') {
        goto('/user/login');
        return;
      }
      
      currentUser = state.user;
      if (currentUser) {
        loadUserData();
      }
    });
    
    return unsubscribe;
  });

  async function loadUserData() {
    loading = true;
    error = null;

    try {
      // Load profile
      const profile = await userService.getProfile(currentUser.id);
      currentUser = { ...currentUser, ...profile };
      
      // Load ride history
      await loadRideHistory();
      
      // Calculate basic statistics
      statistics = {
        totalRides: rideHistory.length,
        completedRides: rideHistory.filter((r: any) => r.status === 'Completed').length,
        totalSpent: rideHistory.reduce((sum: number, r: any) => sum + (r.price || 0), 0),
        averageRating: rideHistory.filter((r: any) => r.rating).length > 0 
          ? rideHistory.filter((r: any) => r.rating).reduce((sum: number, r: any) => sum + (r.rating || 0), 0) / rideHistory.filter((r: any) => r.rating).length
          : 0
      };
      
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd ładowania danych';
    } finally {
      loading = false;
    }
  }

  async function loadRideHistory() {
    try {
      // Use the API service instead of direct fetch
      rideHistory = await userService.getRideHistory(currentUser.id);
    } catch (error) {
      console.error('Error loading ride history:', error);
      rideHistory = [];
    }
  }

  async function updateProfile() {
    if (!currentUser) return;
    
    loading = true;
    error = null;
    
    try {
      await userService.updateProfile(
        currentUser.id,
        profileForm.firstName,
        profileForm.lastName,
        profileForm.email,
        profileForm.phoneNumber,
        profileForm.preferredPaymentMethod
      );
      
      success = 'Profil zaktualizowany pomyślnie';
      showProfileForm = false;
      await loadUserData(); // Reload data
      
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd aktualizacji profilu';
    } finally {
      loading = false;
    }
  }

  function openProfileForm() {
    profileForm = {
      firstName: currentUser.firstName || '',
      lastName: currentUser.lastName || '',
      email: currentUser.email || '',
      phoneNumber: currentUser.phoneNumber || '',
      preferredPaymentMethod: currentUser.preferredPaymentMethod || ''
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
  <title>Panel użytkownika - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-6xl">
    <!-- Header -->
    <div class="text-center mb-8">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">Panel użytkownika</h1>
      <p class="text-gray-600 dark:text-gray-300">Witaj, {currentUser?.firstName} {currentUser?.lastName}</p>
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
        <!-- Quick Actions Card -->
        <div class="uber-card">
          <h2 class="text-xl font-semibold mb-4 dark:text-white">Szybkie akcje</h2>
          <div class="space-y-4">
            <a href="/rides/order" class="uber-btn w-full block text-center">
              Zamów przejazd
            </a>
            <a href="/user/rides" class="uber-btn w-full block text-center bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600">
              Historia przejazdów
            </a>
            <a href="/support" class="uber-btn w-full block text-center bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600">
              Wsparcie
            </a>
            <button
              on:click={openProfileForm}
              class="uber-btn w-full bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600"
            >
              Edytuj profil
            </button>
          </div>
        </div>

        <!-- Statistics Card -->
        <div class="uber-card">
          <h2 class="text-xl font-semibold mb-4 dark:text-white">Statystyki</h2>
          {#if statistics}
            <div class="grid grid-cols-2 gap-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-blue-600 dark:text-blue-400">{statistics.totalRides}</div>
                <div class="text-sm text-gray-600 dark:text-gray-400">Wszystkie przejazdy</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-green-600 dark:text-green-400">{statistics.totalSpent.toFixed(2)} zł</div>
                <div class="text-sm text-gray-600 dark:text-gray-400">Łącznie wydane</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-yellow-600 dark:text-yellow-400">{statistics.averageRating.toFixed(1)}</div>
                <div class="text-sm text-gray-600 dark:text-gray-400">Średnia ocena</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-purple-600 dark:text-purple-400">{statistics.completedRides}</div>
                <div class="text-sm text-gray-600 dark:text-gray-400">Zakończone</div>
              </div>
            </div>
          {:else}
            <p class="text-gray-500 dark:text-gray-400">Brak danych statystycznych</p>
          {/if}
        </div>
      </div>

      <!-- Recent Ride History -->
      <div class="uber-card mb-8">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-xl font-semibold dark:text-white">Ostatnie przejazdy</h2>
          <a href="/user/rides" class="text-blue-600 dark:text-blue-400 hover:underline">Zobacz wszystkie</a>
        </div>
        {#if rideHistory.length > 0}
          <div class="space-y-4">
            {#each rideHistory.slice(0, 5) as ride}
              <div class="border border-gray-200 dark:border-gray-700 rounded-lg p-4">
                <div class="flex items-center justify-between">
                  <div>
                    <h3 class="font-medium dark:text-white">{ride.pickupLocation} → {ride.dropoffLocation}</h3>
                    <p class="text-sm text-gray-600 dark:text-gray-400">{formatDate(ride.orderTime)}</p>
                    <div class="text-sm text-gray-400">{formatStatus(ride.status)}</div>
                    <div class="text-sm text-gray-400">Cena: {ride.price ? `${ride.price.toFixed(2)} zł` : 'Brak'}</div>
                    <div class="text-sm text-gray-400">Kierowca: {ride.riderName || 'Brak'}</div>
                    <div class="text-sm text-gray-400">Płatność: {ride.isPaid ? 'Opłacone' : 'Nieopłacone'}</div>
                  </div>
                  <div class="text-right">
                    <div class="text-lg font-semibold text-green-400">{typeof ride.price === 'number' ? ride.price.toFixed(2) + ' zł' : '—'}</div>
                    <span class="px-2 py-1 text-xs rounded-full {getStatusColor(ride.status)}">
                      {formatStatus(ride.status)}
                    </span>
                    {#if typeof ride.rating === 'number'}
                      <div class="text-sm text-gray-400 mt-1">⭐ {ride.rating.toFixed(1)}</div>
                    {:else}
                      <div class="text-sm text-gray-400 mt-1">brak oceny</div>
                    {/if}
                  </div>
                </div>
              </div>
            {/each}
          </div>
        {:else}
          <div class="text-center py-8">
            <p class="text-gray-500 dark:text-gray-400">Nie masz jeszcze żadnych przejazdów</p>
            <a href="/rides/order" class="uber-btn mt-4 inline-block">Zamów pierwszy przejazd</a>
          </div>
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
          <label for="firstName" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Imię</label>
          <input
            id="firstName"
            type="text"
            bind:value={profileForm.firstName}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
          />
        </div>
        <div>
          <label for="lastName" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nazwisko</label>
          <input
            id="lastName"
            type="text"
            bind:value={profileForm.lastName}
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
          <label for="paymentMethod" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Preferowana metoda płatności</label>
          <select
            id="paymentMethod"
            bind:value={profileForm.preferredPaymentMethod}
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
          >
            <option value="">Wybierz metodę</option>
            <option value="Credit Card">Karta kredytowa</option>
            <option value="Cash">Gotówka</option>
            <option value="PayPal">PayPal</option>
            <option value="Bank Transfer">Przelew bankowy</option>
          </select>
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