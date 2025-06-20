<script lang="ts">
import { onMount } from 'svelte';
import { supportService, authService } from '$lib/api';
import { authStore } from '$lib/auth';
import { goto } from '$app/navigation';

let tickets: any[] = [];
let loading: boolean = false;
let error: string | null = null;
let currentUser: any = null;

onMount(() => {
  // Sprawdź czy użytkownik jest zalogowany jako admin
  const unsubscribe = authStore.subscribe(async state => {
    if (!state.isAuthenticated || state.userType !== 'user') {
      goto('/');
      return;
    }
    
    currentUser = state.user;
    if (!currentUser || currentUser.login !== 'admin') {
      goto('/');
      return;
    }
    
    await loadPendingTickets();
  });
  
  return unsubscribe;
});

async function loadPendingTickets() {
  loading = true;
  error = null;
  try {
    const response = await supportService.getAllTickets();
    tickets = (response.Tickets ?? response.tickets ?? response ?? []).map((t: any) => ({
      ...t,
      subject: t.subject ?? t.Subject ?? '',
      status: t.status ?? t.Status ?? '',
      description: t.description ?? t.Description ?? '',
      createdAt: t.createdAt ?? t.created_date ?? t.CreatedDate ?? t.created ?? t.date ?? '',
      priority: t.priority ?? t.Priority ?? ''
    }));
  } catch (err: unknown) {
    error = err instanceof Error ? err.message : 'Błąd pobierania zgłoszeń';
  } finally {
    loading = false;
  }
}

function formatDate(dateString: any) {
  const date = new Date(dateString);
  return isNaN(date.getTime()) ? 'Brak daty' : date.toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}
</script>

<svelte:head>
  <title>Panel wsparcia - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-4xl">
    <div class="uber-card uber-fadein mb-8">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-4">Zgłoszenia oczekujące na obsługę</h1>
      {#if loading}
        <div class="uber-skeleton h-32 w-full mb-4"></div>
      {:else if error}
        <div class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 rounded-md mb-6" role="alert">
          <p>{error}</p>
        </div>
      {:else if tickets.length === 0}
        <div class="bg-gray-50 dark:bg-[#232329] rounded-lg p-8 text-center">
          <p class="text-gray-600 dark:text-gray-400">Brak zgłoszeń do obsługi.</p>
        </div>
      {:else}
        <ul class="divide-y divide-gray-200 dark:divide-gray-700">
          {#each tickets as ticket}
            <li class="py-4 flex items-center justify-between">
              <div>
                <p class="font-medium text-white">{ticket.subject}</p>
                <p class="text-sm text-gray-400">{formatDate(ticket.createdAt || ticket.createdDate || ticket.CreatedDate || ticket.created_at || ticket.created || ticket.date || '')}</p>
                <p class="text-xs text-gray-400">Priorytet: {ticket.priority} | Status: {ticket.status}</p>
              </div>
              <a href={`/support/${ticket.id}`} class="uber-btn">Szczegóły</a>
            </li>
          {/each}
        </ul>
      {/if}
    </div>
  </div>
</div> 