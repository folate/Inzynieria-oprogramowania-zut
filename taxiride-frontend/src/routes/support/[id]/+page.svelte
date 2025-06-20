<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { supportService, authService } from '$lib/api';
  import { authStore } from '$lib/auth';
  
  // API URL
  const API_URL = 'http://localhost:5145';
  
  let ticket: any = null;
  let messages: any[] = [];
  let loading = false;
  let error: any = null;
  let newMessage = '';
  let currentUser: any = null;
  
  $: ticketId = $page.params.id;
  
  onMount(() => {
    // Sprawdź czy użytkownik jest zalogowany
    const unsubscribe = authStore.subscribe(async state => {
      if (!state.isAuthenticated && !state.worker) {
        return;
      }
      // Support all user types
      currentUser = state.user || state.rider || state.worker;
      if (currentUser && ticketId) {
        await loadTicket();
      }
    });
    
    return unsubscribe;
  });
  
  async function loadTicket() {
    loading = true;
    error = null;
    try {
      const response = await supportService.getTicketDetails(parseInt(ticketId));
      if (response && (response.success || response.Success)) {
        ticket = response.ticket || response.Ticket || response;
        messages = response.messages || response.Messages || [];
      } else {
        error = 'Nie udało się załadować zgłoszenia';
      }
    } catch (err: any) {
      console.error('Error loading ticket:', err);
      error = err.message || 'Błąd podczas ładowania zgłoszenia';
    } finally {
      loading = false;
    }
  }
  
  async function addMessage() {
    if (!newMessage.trim() || !currentUser) return;
    
    loading = true;
    error = null;
    
    try {
      await supportService.replyToTicket(parseInt(ticketId), currentUser.id, newMessage, false);
      newMessage = '';
      await loadTicket(); // Reload to get updated messages
    } catch (err: any) {
      error = err.message;
    } finally {
      loading = false;
    }
  }
  
  async function closeTicket() {
    if (!confirm('Czy na pewno chcesz zamknąć to zgłoszenie?')) return;
    
    loading = true;
    error = null;
    
    try {
      error = 'Funkcja zamykania zgłoszeń nie jest obecnie dostępna';
    } catch (err: any) {
      error = err.message;
    } finally {
      loading = false;
    }
  }
  
  function formatDate(dateString: string) {
    return new Date(dateString).toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
  
  function getStatusClass(status: string) {
    switch (status) {
      case 'New': return 'bg-yellow-100 text-yellow-800';
      case 'Open': return 'bg-yellow-100 text-yellow-800';
      case 'InProgress': return 'bg-blue-100 text-blue-800';
      case 'Resolved': return 'bg-green-100 text-green-800';
      case 'Closed': return 'bg-gray-100 text-gray-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
  
  function getPriorityClass(priority: string) {
    switch (priority) {
      case 'High': return 'bg-red-100 text-red-800';
      case 'Medium': return 'bg-yellow-100 text-yellow-800';
      case 'Low': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
</script>

<svelte:head>
  <title>Szczegóły reklamacji - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-2xl">
    <div class="uber-card uber-fadein mb-8">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-4">Szczegóły reklamacji</h1>
      {#if loading}
        <div class="uber-skeleton h-32 w-full mb-4"></div>
      {:else if error}
        <div class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 rounded-md mb-6" role="alert">
          <p>{error}</p>
        </div>
      {:else if ticket}
        <div class="space-y-4">
          <div class="flex items-center space-x-4">
            <div class="w-12 h-12 bg-gray-700 rounded-full flex items-center justify-center">
              <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
              </svg>
            </div>
            <div>
              <h2 class="font-medium text-white">{ticket.subject || 'Brak tematu'}</h2>
              <div class="text-sm text-gray-300">{ticket.status || 'Nieznany status'}</div>
            </div>
          </div>
          <div class="text-sm text-gray-400">{ticket.description || 'Brak opisu'}</div>
          <div class="text-sm text-gray-400">Zgłoszono: {ticket.createdDate ? formatDate(ticket.createdDate) : 'Brak daty'}</div>
        </div>
      {:else}
        <div class="bg-gray-50 dark:bg-[#232329] rounded-lg p-8 text-center">
          <p class="text-gray-600 dark:text-gray-400">Nie znaleziono zgłoszenia.</p>
        </div>
      {/if}
    </div>
    <!-- Wiadomości i akcje -->
    <div class="uber-card uber-fadein mt-8">
      <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Wiadomości</h2>
      {#if loading}
        <div class="uber-skeleton h-16 w-full mb-4"></div>
      {:else if messages && messages.length > 0}
        <ul class="divide-y divide-gray-200 dark:divide-gray-700">
          {#each messages as message}
            <li class="py-2">
              <div class="flex items-center space-x-2">
                <span class="font-medium text-white">{message.isFromSupport ? 'Wsparcie' : 'Ty'}</span>
                <span class="text-xs text-gray-400">{message.sentDate ? formatDate(message.sentDate) : 'Brak daty'}</span>
              </div>
              <div class="text-gray-300">{message.message || 'Brak wiadomości'}</div>
            </li>
          {/each}
        </ul>
      {:else}
        <div class="text-gray-400 text-center py-4">Brak wiadomości</div>
      {/if}
      <!-- Formularz odpowiedzi -->
      <form class="mt-6 space-y-4" on:submit|preventDefault={addMessage}>
        <textarea 
          bind:value={newMessage}
          class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" 
          rows="3" 
          placeholder="Napisz odpowiedź..."
        ></textarea>
        <button type="submit" class="uber-btn w-full" disabled={loading || !ticket}>Wyślij</button>
      </form>
    </div>
  </div>
</div>


