<script lang="ts">
  import { onMount } from 'svelte';
  import { userService, authService } from '$lib/api';
  import { authStore } from '$lib/auth';
  import { goto } from '$app/navigation';

  // API URL
  const API_URL = 'http://localhost:5145';

  interface Ride {
    id: number;
    pickupLocation: string;
    dropoffLocation: string;
    orderTime: string;
    pickupTime?: string;
    dropoffTime?: string;
    price: number;
    status: string;
    rating?: number;
    riderName: string;
    isPaid: boolean;
    paymentMethod?: string;
  }
  
  let rides: Ride[] = [];
  let loading: boolean = true;
  let error: string = '';
  let ratingRide: Ride | null = null;
  let rating: number = 5;
  let comment: string = '';

  // Complaint form
  let complaintRide: Ride | null = null;
  let complaintDescription: string = '';

  // Payment form
  let paymentRide: Ride | null = null;
  let paymentMethod: string = 'Credit Card';

  let userId: number | null = null;
  let rideHistory: Ride[] = [];
  let selectedRide: Ride | null = null;
  let showRatingModal = false;
  let showComplaintModal = false;
  let showPaymentModal = false;

  onMount(() => {
    // Sprawdź czy użytkownik jest zalogowany jako użytkownik
    const unsubscribe = authStore.subscribe(async state => {
      if (!state.isAuthenticated || state.userType !== 'user') {
        goto('/user/login');
        return;
      }
      
      const currentUser: any = state.user;
      if (currentUser && currentUser.id) {
        try {
          rides = await userService.getRideHistory(currentUser.id);
        } catch (err) {
          error = 'Nie udało się załadować historii przejazdów. Spróbuj ponownie.';
        } finally {
          loading = false;
        }
      }
    });
    
    return unsubscribe;
  });

  async function handleRateRide(): Promise<void> {
    try {
      await userService.rateRide(ratingRide!.id, rating, comment);

      // Update ride in the list
      const index = rides.findIndex(r => r.id === ratingRide!.id);
      if (index !== -1) {
        rides[index].rating = rating;
        rides = [...rides]; // Trigger update
      }

      ratingRide = null;
      rating = 5;
      comment = '';
    } catch (err) {
      error = 'Failed to submit rating. Please try again.';
    }
  }

  async function handleFileComplaint(): Promise<void> {
    try {
      await userService.fileComplaint(complaintRide!.id, complaintDescription);
      complaintRide = null;
      complaintDescription = '';
    } catch (err) {
      error = 'Failed to file complaint. Please try again.';
    }
  }

  async function handleProcessPayment(): Promise<void> {
    try {
      await userService.processPayment(paymentRide!.id, paymentMethod);

      // Update ride in the list
      const index = rides.findIndex(r => r.id === paymentRide!.id);
      if (index !== -1) {
        rides[index].isPaid = true;
        rides[index].paymentMethod = paymentMethod;
        rides = [...rides]; // Trigger update
      }

      paymentRide = null;
    } catch (err) {
      error = 'Failed to process payment. Please try again.';
    }
  }

  function formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString();
  }

  function formatStatus(status: string | undefined | null): string {
    if (!status) return "brak";
    return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
  }

  function getStatusColor(status: string): string {
    switch(status.toLowerCase()) {
      case 'pending': return 'bg-yellow-100 text-yellow-800';
      case 'accepted': return 'bg-blue-100 text-blue-800';
      case 'completed': return 'bg-green-100 text-green-800';
      case 'cancelled': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  async function loadRideHistory() {
    try {
      const response = await fetch(`${API_URL}/User/rides/${userId}`);
      if (response.ok) {
        const data = await response.json();
        // API zwraca bezpośrednio tablicę obiektów z polami zgodnymi z RideHistoryDto
        rideHistory = data || [];
      } else {
        console.error('Failed to load ride history');
      }
    } catch (error) {
      console.error('Error loading ride history:', error);
    }
  }
</script>

<svelte:head>
  <title>Moje przejazdy - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-4xl">
    <div class="uber-card uber-fadein mb-8">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-4">Moje przejazdy</h1>
      {#if loading}
        <div class="uber-skeleton h-32 w-full mb-4"></div>
      {:else if error}
        <div class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 rounded-md mb-6" role="alert">
          <p>{error}</p>
        </div>
      {:else if rides.length === 0}
        <div class="bg-gray-50 dark:bg-[#232329] rounded-lg p-8 text-center">
          <p class="text-gray-600 dark:text-gray-400">Nie masz jeszcze żadnych przejazdów.</p>
        </div>
      {:else}
        <ul class="divide-y divide-gray-200 dark:divide-gray-700">
          {#each rides as ride}
            <li class="py-4 flex items-center justify-between">
              <div class="flex items-center space-x-4">
                <div class="w-12 h-12 bg-gray-700 rounded-full flex items-center justify-center">
                  <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
                  </svg>
                </div>
                <div>
                  <h2 class="font-medium text-white">{ride.riderName || 'Nieznany kierowca'}</h2>
                  <div class="flex items-center space-x-2 text-sm text-gray-300">
                    <span>⭐ {typeof ride.rating === 'number' ? ride.rating.toFixed(1) : 'brak oceny'}</span>
                    <span>•</span>
                    <span>{ride.pickupLocation} → {ride.dropoffLocation}</span>
                  </div>
                  <div class="text-sm text-gray-400">{formatDate(ride.orderTime)}</div>
                </div>
              </div>
              <div class="text-right">
                <div class="text-lg font-semibold text-green-400">{typeof ride.price === 'number' ? ride.price.toFixed(2) + ' zł' : '—'}</div>
                <div class="text-sm text-gray-400">{formatStatus(ride.status)}</div>
                <div class="text-sm text-gray-400">Cena: {ride.price ? `${ride.price.toFixed(2)} zł` : 'Brak'}</div>
                <div class="text-sm text-gray-400">Kierowca: {ride.riderName || 'Brak'}</div>
                <div class="text-sm text-gray-400">Płatność: {ride.isPaid ? 'Opłacone' : 'Nieopłacone'}</div>
                {#if ride.paymentMethod}
                    <div class="text-sm text-gray-400">Metoda: {ride.paymentMethod}</div>
                {/if}
                <a href={`/rides/${ride.id}`} class="uber-btn mt-2 block">Szczegóły</a>
              </div>
            </li>
          {/each}
        </ul>
      {/if}
    </div>
  </div>
</div>

<!-- Rating Modal -->
{#if ratingRide}
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 flex justify-center items-center z-50">
    <div class="bg-white rounded-xl shadow-lg p-6 max-w-md w-full mx-4">
      <h2 class="text-xl font-bold mb-4">Rate your ride</h2>
      <p class="text-gray-600 mb-4">{ratingRide.pickupLocation} → {ratingRide.dropoffLocation}</p>
      
      <div class="mb-4">
        <label class="block text-gray-700 mb-2">Rating</label>
        <div class="flex space-x-2">
          {#each Array(5) as _, i}
            <button 
              on:click={() => rating = i + 1} 
              class="focus:outline-none"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" viewBox="0 0 20 20" fill={i < rating ? 'currentColor' : 'none'} stroke="currentColor" class:text-yellow-400={i < rating} class:text-gray-300={i >= rating}>
                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
              </svg>
            </button>
          {/each}
        </div>
      </div>
      
      <div class="mb-4">
        <label for="comment" class="block text-gray-700 mb-2">Comment (optional)</label>
        <textarea 
          id="comment" 
          bind:value={comment} 
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-black focus:border-black"
          rows="3"
        ></textarea>
      </div>
      
      <div class="flex justify-end space-x-3">
        <button 
          on:click={() => ratingRide = null} 
          class="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-100 transition"
        >
          Cancel
        </button>
        <button 
          on:click={handleRateRide} 
          class="px-4 py-2 bg-black text-white rounded-md hover:bg-gray-800 transition"
        >
          Submit Rating
        </button>
      </div>
    </div>
  </div>
{/if}

<!-- Complaint Modal -->
{#if complaintRide}
  <div class="fixed inset-0 bg-gray-600 bg-opacity-10 flex justify-center items-center z-50">
    <div class="bg-white rounded-xl shadow-lg p-6 max-w-md w-full mx-4">
      <h2 class="text-xl font-bold mb-4">Need help with this trip?</h2>
      <p class="text-gray-600 mb-4">{complaintRide.pickupLocation} → {complaintRide.dropoffLocation}</p>
      
      <div class="mb-4">
        <label for="complaintDescription" class="block text-gray-700 mb-2">Describe your issue</label>
        <textarea 
          id="complaintDescription" 
          bind:value={complaintDescription} 
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-black focus:border-black"
          rows="4"
        ></textarea>
      </div>
      
      <div class="flex justify-end space-x-3">
        <button 
          on:click={() => complaintRide = null} 
          class="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-100 transition"
        >
          Cancel
        </button>
        <button 
          on:click={handleFileComplaint} 
          class="px-4 py-2 bg-black text-white rounded-md hover:bg-gray-800 transition"
        >
          Submit
        </button>
      </div>
    </div>
  </div>
{/if}

<!-- Payment Modal -->
{#if paymentRide}
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 flex justify-center items-center z-50">
    <div class="bg-white rounded-xl shadow-lg p-6 max-w-md w-full mx-4">
      <h2 class="text-xl font-bold mb-4">Pay for ride</h2>
      <p class="text-gray-600 mb-2">{paymentRide.pickupLocation} → {paymentRide.dropoffLocation}</p>
      <p class="text-2xl font-bold mb-4">${paymentRide.price.toFixed(2)}</p>
      
      <div class="mb-4">
        <label for="paymentMethod" class="block text-gray-700 mb-2">Payment Method</label>
        <select 
          id="paymentMethod" 
          bind:value={paymentMethod} 
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-black focus:border-black"
        >
          <option value="Credit Card">Credit Card</option>
          <option value="PayPal">PayPal</option>
          <option value="Apple Pay">Apple Pay</option>
          <option value="Google Pay">Google Pay</option>
        </select>
      </div>
      
      <div class="flex justify-end space-x-3">
        <button 
          on:click={() => paymentRide = null} 
          class="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-100 transition"
        >
          Cancel
        </button>
        <button 
          on:click={handleProcessPayment} 
          class="px-4 py-2 bg-black text-white rounded-md hover:bg-gray-800 transition"
        >
          Pay Now
        </button>
      </div>
    </div>
  </div>
{/if}
