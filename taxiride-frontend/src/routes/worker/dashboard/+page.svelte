<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { authStore } from '../../../lib/auth';
  import { workerService } from '../../../lib/api';

  let currentWorker: any = null;
  let assignedTickets: any[] = [];
  let unassignedTickets: any[] = [];
  let statistics: any = null;
  let loading = true;
  let error: string | null = null;
  let activeTab = 'assigned';

  onMount(() => {
    const unsubscribe = authStore.subscribe(async state => {
      if (!state.worker) {
        goto('/worker/login');
        return;
      }
      
      currentWorker = state.worker;
      if (currentWorker) {
        await loadData();
      }
    });

    return unsubscribe;
  });

  async function loadData() {
    if (!currentWorker) return;

    loading = true;
    error = null;

    try {
      // Load assigned tickets
      const assignedResponse = await workerService.getAssignedTickets(currentWorker.id);
      assignedTickets = assignedResponse.tickets || [];

      // Load unassigned tickets
      const unassignedResponse = await workerService.getUnassignedTickets();
      unassignedTickets = unassignedResponse.tickets || [];

      // Load statistics
      const statsResponse = await workerService.getStatistics();
      statistics = statsResponse.statistics;
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd ładowania danych';
    } finally {
      loading = false;
    }
  }

  async function assignTicket(ticketId: number) {
    try {
      await workerService.assignTicket(ticketId, currentWorker.id);
      await loadData(); // Reload data
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd przypisywania zgłoszenia';
    }
  }

  function formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('pl-PL', {
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

  function logout() {
    authStore.update(state => ({ ...state, worker: null }));
    goto('/worker/login');
  }
</script>

<svelte:head>
  <title>Panel Pracownika - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] transition-colors duration-300">
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-6xl mx-auto">
      <!-- Header -->
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Panel Pracownika</h1>
          <p class="text-gray-600 dark:text-gray-400">
            Witaj, {currentWorker?.name || 'Pracowniku'}! ({currentWorker?.department || 'Wsparcie'})
          </p>
        </div>
        <button on:click={logout} class="uber-btn bg-red-600 hover:bg-red-700">
          Wyloguj się
        </button>
      </div>

      {#if error}
        <div class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg p-4 mb-6">
          <p class="text-red-700 dark:text-red-200">{error}</p>
        </div>
      {/if}

      {#if loading}
        <div class="flex justify-center py-8">
          <span class="uber-skeleton w-12 h-12 rounded-full"></span>
        </div>
      {:else}
        <!-- Statistics -->
        {#if statistics}
          <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
            <div class="uber-card text-center">
              <div class="text-2xl font-bold text-blue-600 dark:text-blue-400">{statistics.totalTickets}</div>
              <div class="text-sm text-gray-600 dark:text-gray-400">Wszystkie zgłoszenia</div>
            </div>
            <div class="uber-card text-center">
              <div class="text-2xl font-bold text-yellow-600 dark:text-yellow-400">{statistics.openTickets}</div>
              <div class="text-sm text-gray-600 dark:text-gray-400">Otwarte zgłoszenia</div>
            </div>
            <div class="uber-card text-center">
              <div class="text-2xl font-bold text-green-600 dark:text-green-400">{statistics.resolvedTickets}</div>
              <div class="text-sm text-gray-600 dark:text-gray-400">Rozwiązane zgłoszenia</div>
            </div>
            <div class="uber-card text-center">
              <div class="text-2xl font-bold text-red-600 dark:text-red-400">{statistics.highPriorityTickets}</div>
              <div class="text-sm text-gray-600 dark:text-gray-400">Wysoki priorytet</div>
            </div>
          </div>
        {/if}

        <!-- Tabs -->
        <div class="uber-card mb-8">
          <div class="border-b border-gray-200 dark:border-gray-700">
            <nav class="-mb-px flex space-x-8">
              <button
                on:click={() => activeTab = 'assigned'}
                class="py-2 px-1 border-b-2 font-medium text-sm {activeTab === 'assigned' ? 'border-blue-500 text-blue-600 dark:text-blue-400' : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300'}"
              >
                Przypisane do mnie ({assignedTickets.length})
              </button>
              <button
                on:click={() => activeTab = 'unassigned'}
                class="py-2 px-1 border-b-2 font-medium text-sm {activeTab === 'unassigned' ? 'border-blue-500 text-blue-600 dark:text-blue-400' : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300'}"
              >
                Nieprzypisane ({unassignedTickets.length})
              </button>
            </nav>
          </div>

          <div class="mt-6">
            {#if activeTab === 'assigned'}
              <!-- Assigned Tickets -->
              {#if assignedTickets.length === 0}
                <div class="text-center py-8">
                  <p class="text-gray-600 dark:text-gray-400">Brak przypisanych zgłoszeń</p>
                </div>
              {:else}
                <div class="space-y-4">
                  {#each assignedTickets as ticket}
                    <div class="border border-gray-200 dark:border-gray-700 rounded-lg p-4 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                      <div class="flex justify-between items-start">
                        <div class="flex-1">
                          <div class="flex items-center gap-2 mb-2">
                            <h3 class="font-medium dark:text-white">{ticket.subject}</h3>
                            <span class="text-sm text-gray-500 dark:text-gray-400">#{ticket.ticketNumber}</span>
                          </div>
                          <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">
                            Od: {ticket.userName} ({ticket.userEmail})
                          </p>
                          <div class="flex items-center gap-2 text-sm">
                            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {getStatusClass(ticket.status)}">
                              {ticket.status}
                            </span>
                            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {getPriorityClass(ticket.priority)}">
                              {ticket.priority}
                            </span>
                            <span class="text-gray-500 dark:text-gray-400">
                              Utworzono: {formatDate(ticket.createdDate)}
                            </span>
                          </div>
                        </div>
                        <a
                          href="/support/{ticket.id}"
                          class="uber-btn bg-blue-600 hover:bg-blue-700 text-sm"
                        >
                          Otwórz
                        </a>
                      </div>
                    </div>
                  {/each}
                </div>
              {/if}
            {:else}
              <!-- Unassigned Tickets -->
              {#if unassignedTickets.length === 0}
                <div class="text-center py-8">
                  <p class="text-gray-600 dark:text-gray-400">Brak nieprzypisanych zgłoszeń</p>
                </div>
              {:else}
                <div class="space-y-4">
                  {#each unassignedTickets as ticket}
                    <div class="border border-gray-200 dark:border-gray-700 rounded-lg p-4 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                      <div class="flex justify-between items-start">
                        <div class="flex-1">
                          <div class="flex items-center gap-2 mb-2">
                            <h3 class="font-medium dark:text-white">{ticket.subject}</h3>
                            <span class="text-sm text-gray-500 dark:text-gray-400">#{ticket.ticketNumber}</span>
                          </div>
                          <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">
                            Od: {ticket.userName} ({ticket.userEmail})
                          </p>
                          <div class="flex items-center gap-2 text-sm">
                            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {getStatusClass(ticket.status)}">
                              {ticket.status}
                            </span>
                            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {getPriorityClass(ticket.priority)}">
                              {ticket.priority}
                            </span>
                            <span class="text-gray-500 dark:text-gray-400">
                              Utworzono: {formatDate(ticket.createdDate)}
                            </span>
                          </div>
                        </div>
                        <div class="flex gap-2">
                          <button
                            on:click={() => assignTicket(ticket.id)}
                            class="uber-btn bg-green-600 hover:bg-green-700 text-sm"
                          >
                            Przypisz do mnie
                          </button>
                          <a
                            href="/support/{ticket.id}"
                            class="uber-btn bg-blue-600 hover:bg-blue-700 text-sm"
                          >
                            Otwórz
                          </a>
                        </div>
                      </div>
                    </div>
                  {/each}
                </div>
              {/if}
            {/if}
          </div>
        </div>
      {/if}
    </div>
  </div>
</div> 