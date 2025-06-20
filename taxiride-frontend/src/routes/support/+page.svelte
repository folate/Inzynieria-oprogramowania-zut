<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { authStore } from '../../lib/auth';
  import { supportService } from '../../lib/api';
  
  // API URL
  const API_URL = 'http://localhost:5145';
  
  let currentUser: any = null;
  let tickets: any[] = [];
  let loading = true;
  let error: string | null = null;
  let showNewTicketForm: boolean = false;
  
  // New ticket form data
  let newTicket = {
    subject: '',
    description: '',
    priority: 'Medium'
  };
  
  onMount(() => {
    // Sprawdź czy użytkownik jest zalogowany
    const unsubscribe = authStore.subscribe(async state => {
      if (!state.isAuthenticated) {
        return;
      }
      
      currentUser = state.user || state.rider;
      if (currentUser) {
        await loadTickets();
      }
    });
    
    return unsubscribe;
  });
  
  async function loadTickets() {
    if (!currentUser) return;
    
    loading = true;
    error = null;
    
    try {
      tickets = await supportService.getUserTickets(currentUser.id);
    } catch (error) {
      console.error('Error loading tickets:', error);
      tickets = [];
    } finally {
      loading = false;
    }
  }
  
  async function createTicket() {
    if (!currentUser) return;
    
    loading = true;
    error = null;
    
    try {
      await supportService.createTicket(
        currentUser.id,
        currentUser.name ? 'Rider' : 'User',
        newTicket.subject,
        newTicket.description,
        newTicket.priority
      );
      
      // Reset form
      newTicket = { subject: '', description: '', priority: 'Medium' };
      showNewTicketForm = false;
      
      // Reload tickets
      await loadTickets();
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd wysyłania zgłoszenia';
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
  
  function getStatusClass(status: any) {
    switch (status) {
      case 'New': return 'bg-yellow-100 text-yellow-800';
      case 'Open': return 'bg-yellow-100 text-yellow-800';
      case 'InProgress': return 'bg-blue-100 text-blue-800';
      case 'Resolved': return 'bg-green-100 text-green-800';
      case 'Closed': return 'bg-gray-100 text-gray-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
  
  function getPriorityClass(priority: any) {
    switch (priority) {
      case 'High': return 'bg-red-100 text-red-800';
      case 'Medium': return 'bg-yellow-100 text-yellow-800';
      case 'Low': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
</script>

<svelte:head>
  <title>Wsparcie - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] transition-colors duration-300">
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Centrum Wsparcia</h1>
        <button
          on:click={() => showNewTicketForm = !showNewTicketForm}
          class="uber-btn"
        >
          {showNewTicketForm ? 'Anuluj' : 'Nowe zgłoszenie'}
        </button>
      </div>
      {#if error}
        <div class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg p-4 mb-6">
          <p class="text-red-700 dark:text-red-200">{error}</p>
        </div>
      {/if}
      {#if showNewTicketForm}
        <div class="uber-card uber-fadein mb-8">
          <h2 class="text-xl font-bold mb-4 dark:text-white">Nowe zgłoszenie</h2>
          <form on:submit|preventDefault={createTicket} class="space-y-4">
            <div>
              <label for="subject" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Temat</label>
              <input
                id="subject"
                type="text"
                bind:value={newTicket.subject}
                required
                class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
                placeholder="Opisz problem w kilku słowach"
              />
            </div>
            <div>
              <label for="priority" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Priorytet</label>
              <select
                id="priority"
                bind:value={newTicket.priority}
                class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
              >
                <option value="Low">Niski</option>
                <option value="Medium">Średni</option>
                <option value="High">Wysoki</option>
              </select>
            </div>
            <div>
              <label for="description" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Opis problemu</label>
              <textarea
                id="description"
                bind:value={newTicket.description}
                required
                rows="4"
                class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
                placeholder="Opisz szczegółowo problem, który wystąpił..."
              ></textarea>
            </div>
            <div class="flex gap-2">
              <button
                type="submit"
                disabled={loading}
                class="uber-btn flex-1"
              >
                {#if loading}
                  <span class="uber-skeleton w-6 h-6 rounded-full mr-3"></span>
                {/if}
                {loading ? 'Wysyłanie...' : 'Wyślij zgłoszenie'}
              </button>
              <button
                type="button"
                on:click={() => showNewTicketForm = false}
                class="uber-btn flex-1 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600"
              >
                Anuluj
              </button>
            </div>
          </form>
        </div>
      {/if}
      {#if loading && tickets.length === 0}
        <div class="flex justify-center py-8">
          <span class="uber-skeleton w-12 h-12 rounded-full"></span>
        </div>
      {:else if tickets.length === 0}
        <div class="bg-gray-50 dark:bg-[#232329] rounded-lg p-8 text-center">
          <p class="text-gray-600 dark:text-gray-400">Brak zgłoszeń. Gdy napotkasz problem, możesz utworzyć nowe zgłoszenie.</p>
        </div>
      {:else}
        <div class="uber-card uber-fadein">
          <h2 class="text-xl font-bold mb-4 dark:text-white">Twoje zgłoszenia</h2>
          <ul class="divide-y divide-gray-200 dark:divide-gray-700">
            {#each tickets as ticket}
              <li class="py-4">
                <div class="flex items-center justify-between">
                  <div>
                    <a href="/support/{ticket.id}" class="font-medium dark:text-white hover:underline">{ticket.subject}</a>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Numer: {ticket.ticketNumber}</p>
                  </div>
                  <div class="text-right">
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {getStatusClass(ticket.status)}">
                      {ticket.status}
                    </span>
                  </div>
                </div>
                <div class="mt-2 flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
                  <p>Utworzono: {formatDate(ticket.createdDate)}</p>
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {getPriorityClass(ticket.priority)}">
                    Priorytet: {ticket.priority}
                  </span>
                </div>
              </li>
            {/each}
          </ul>
        </div>
      {/if}
    </div>
  </div>
</div>
