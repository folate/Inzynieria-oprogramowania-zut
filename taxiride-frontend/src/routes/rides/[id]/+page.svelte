<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { rideService, authService } from '$lib/api';
  import { authStore } from '$lib/auth';
  import { goto } from '$app/navigation';

  let ride: any = null;
  let loading = true;
  let error: string | null = null;
  let currentUser: any = null;

  onMount(() => {
    const unsubscribe = authStore.subscribe(state => {
      if (!state.isAuthenticated) {
        goto('/user/login');
      }
      currentUser = state.user || state.rider;
    });

    const loadRideDetails = async () => {
        if (!currentUser) {
          return;
        }

        try {
          const rideId = parseInt($page.params.id, 10);
          if (isNaN(rideId)) {
            throw new Error('Invalid ride ID');
          }
          ride = await rideService.getRideStatus(rideId);
        } catch (err: unknown) {
          error = err instanceof Error ? err.message : 'Failed to load ride details';
        } finally {
          loading = false;
        }
    }

    loadRideDetails();

    return () => {
        unsubscribe();
    };
  });

  function formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('pl-PL');
  }
</script>

<svelte:head>
  <title>Szczegóły przejazdu - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-2xl">
    <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-6">Szczegóły przejazdu</h1>

    {#if loading}
      <div class="uber-card"><div class="uber-skeleton h-48 w-full"></div></div>
    {:else if error}
      <div class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg p-4">
        <p class="text-red-700 dark:text-red-200">{error}</p>
      </div>
    {:else if ride}
      <div class="uber-card uber-fadein space-y-4">
        <div>
          <h2 class="text-lg font-semibold dark:text-white">Trasa</h2>
          <p class="text-gray-600 dark:text-gray-300">{ride.pickupLocation} → {ride.dropoffLocation}</p>
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <h3 class="font-medium dark:text-white">Status</h3>
            <p class="text-gray-600 dark:text-gray-300">{ride.status}</p>
          </div>
          <div>
            <h3 class="font-medium dark:text-white">Cena</h3>
            <p class="text-gray-600 dark:text-gray-300">{ride.price.toFixed(2)} zł</p>
          </div>
          <div>
            <h3 class="font-medium dark:text-white">Data zamówienia</h3>
            <p class="text-gray-600 dark:text-gray-300">{formatDate(ride.orderTime)}</p>
          </div>
          {#if ride.pickupTime}
            <div>
              <h3 class="font-medium dark:text-white">Czas odbioru</h3>
              <p class="text-gray-600 dark:text-gray-300">{formatDate(ride.pickupTime)}</p>
            </div>
          {/if}
          {#if ride.dropoffTime}
            <div>
              <h3 class="font-medium dark:text-white">Czas zakończenia</h3>
              <p class="text-gray-600 dark:text-gray-300">{formatDate(ride.dropoffTime)}</p>
            </div>
          {/if}
        </div>
        {#if ride.riderInfo}
          <div class="border-t border-gray-200 dark:border-gray-700 pt-4">
            <h2 class="text-lg font-semibold dark:text-white">Informacje o kierowcy</h2>
            <div class="grid grid-cols-2 gap-4 mt-2">
              <div>
                <h3 class="font-medium dark:text-white">Imię i nazwisko</h3>
                <p class="text-gray-600 dark:text-gray-300">{ride.riderInfo.name}</p>
              </div>
              <div>
                <h3 class="font-medium dark:text-white">Telefon</h3>
                <p class="text-gray-600 dark:text-gray-300">{ride.riderInfo.phone}</p>
              </div>
              <div>
                <h3 class="font-medium dark:text-white">Ocena</h3>
                <p class="text-gray-600 dark:text-gray-300">{ride.riderInfo.rating?.toFixed(1) ?? 'Brak'} ⭐</p>
              </div>
            </div>
          </div>
        {/if}
      </div>
      <div class="uber-card mt-6">
          <h2 class="text-lg font-semibold dark:text-white mb-4">Akcje</h2>
          <div class="flex space-x-4">
            <button class="uber-btn flex-1 bg-red-600 hover:bg-red-700">Zgłoś problem</button>
            <button class="uber-btn flex-1">Oceń przejazd</button>
          </div>
      </div>
    {:else}
      <div class="uber-card">
        <p class="text-gray-600 dark:text-gray-300">Nie znaleziono szczegółów przejazdu.</p>
      </div>
    {/if}
  </div>
</div>
