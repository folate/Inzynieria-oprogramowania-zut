<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { authStore } from '../../../lib/auth';
  import { ownerService } from '../../../lib/api';

  let currentOwner: any = null;
  let drivers: any[] = [];
  let loading = true;
  let error: string | null = null;

  // Financial report form
  let showReportForm = false;
  let reportForm = {
    startDate: '',
    endDate: '',
    reportType: 'Monthly',
    includeVat: false,
    exportFormat: 'TXT'
  };
  let generatingReport = false;
  let generatedReport: any = null;

  onMount(() => {
    const unsubscribe = authStore.subscribe(async state => {
      if (!state.owner) {
        goto('/owner/login');
        return;
      }
      
      currentOwner = state.owner;
      if (currentOwner) {
        await loadData();
      }
    });

    return unsubscribe;
  });

  async function loadData() {
    if (!currentOwner) return;

    loading = true;
    error = null;

    try {
      // Load drivers for the owner's company
      const driversResponse = await ownerService.getDrivers(currentOwner.companyId || 1);
      drivers = driversResponse.drivers || [];
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd ładowania danych';
    } finally {
      loading = false;
    }
  }

  async function generateReport() {
    if (!currentOwner || !reportForm.startDate || !reportForm.endDate) {
      error = 'Wypełnij wszystkie wymagane pola';
      return;
    }

    generatingReport = true;
    error = null;

    try {
      const response = await ownerService.generateFinancialReport(
        currentOwner.id,
        currentOwner.companyId || 1,
        reportForm.startDate,
        reportForm.endDate,
        reportForm.reportType,
        reportForm.includeVat,
        reportForm.exportFormat
      );

      generatedReport = response.report;
      showReportForm = false;
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd generowania raportu';
    } finally {
      generatingReport = false;
    }
  }

  async function downloadReport(reportId: number) {
    try {
      const blob = await ownerService.downloadReport(reportId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `financial_report_${Date.now()}.${reportForm.exportFormat.toLowerCase()}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd pobierania raportu';
    }
  }

  function formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('pl-PL');
  }

  function formatCurrency(amount: number): string {
    return new Intl.NumberFormat('pl-PL', {
      style: 'currency',
      currency: 'PLN'
    }).format(amount);
  }

  function logout() {
    authStore.update(state => ({ ...state, owner: null }));
    goto('/owner/login');
  }
</script>

<svelte:head>
  <title>Panel Właściciela - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] transition-colors duration-300">
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-6xl mx-auto">
      <!-- Header -->
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Panel Właściciela</h1>
          <p class="text-gray-600 dark:text-gray-400">
            Witaj, {currentOwner?.name || 'Właścicielu'}!
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
        <!-- Financial Reports Section -->
        <div class="uber-card mb-8">
          <div class="flex justify-between items-center mb-6">
            <h2 class="text-xl font-bold dark:text-white">Raporty Finansowe</h2>
            <button
              on:click={() => showReportForm = !showReportForm}
              class="uber-btn"
            >
              {showReportForm ? 'Anuluj' : 'Generuj Raport'}
            </button>
          </div>

          {#if showReportForm}
            <div class="uber-fadein border-t border-gray-200 dark:border-gray-700 pt-6">
              <form on:submit|preventDefault={generateReport} class="space-y-4">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label for="startDate" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Data początkowa
                    </label>
                    <input
                      id="startDate"
                      type="date"
                      bind:value={reportForm.startDate}
                      required
                      class="uber-input"
                    />
                  </div>
                  <div>
                    <label for="endDate" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Data końcowa
                    </label>
                    <input
                      id="endDate"
                      type="date"
                      bind:value={reportForm.endDate}
                      required
                      class="uber-input"
                    />
                  </div>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <label for="reportType" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Typ raportu
                    </label>
                    <select
                      id="reportType"
                      bind:value={reportForm.reportType}
                      class="uber-input"
                    >
                      <option value="Daily">Dzienny</option>
                      <option value="Weekly">Tygodniowy</option>
                      <option value="Monthly">Miesięczny</option>
                      <option value="Yearly">Roczny</option>
                    </select>
                  </div>
                  <div>
                    <label for="exportFormat" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Format eksportu
                    </label>
                    <select
                      id="exportFormat"
                      bind:value={reportForm.exportFormat}
                      class="uber-input"
                    >
                      <option value="TXT">TXT</option>
                      <option value="CSV">CSV</option>
                      <option value="PDF">PDF</option>
                    </select>
                  </div>
                  <div class="flex items-center">
                    <label class="flex items-center">
                      <input
                        type="checkbox"
                        bind:checked={reportForm.includeVat}
                        class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                      />
                      <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                        Uwzględnij VAT
                      </span>
                    </label>
                  </div>
                </div>

                <div class="flex gap-4">
                  <button
                    type="submit"
                    disabled={generatingReport}
                    class="uber-btn flex-1"
                  >
                    {#if generatingReport}
                      <span class="uber-skeleton w-6 h-6 rounded-full mr-3"></span>
                    {/if}
                    {generatingReport ? 'Generowanie...' : 'Generuj Raport'}
                  </button>
                  <button
                    type="button"
                    on:click={() => showReportForm = false}
                    class="uber-btn flex-1 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600"
                  >
                    Anuluj
                  </button>
                </div>
              </form>
            </div>
          {/if}

          {#if generatedReport}
            <div class="uber-fadein border-t border-gray-200 dark:border-gray-700 pt-6 mt-6">
              <h3 class="text-lg font-semibold dark:text-white mb-4">Wygenerowany Raport</h3>
              <div class="bg-green-50 dark:bg-green-900/30 border border-green-200 dark:border-green-700 rounded-lg p-4">
                <div class="flex justify-between items-center">
                  <div>
                    <p class="text-green-700 dark:text-green-200 font-medium">
                      Raport został wygenerowany pomyślnie
                    </p>
                    <p class="text-green-600 dark:text-green-300 text-sm">
                      Okres: {formatDate(generatedReport.startDate)} - {formatDate(generatedReport.endDate)}
                    </p>
                    <p class="text-green-600 dark:text-green-300 text-sm">
                      Przychód: {formatCurrency(generatedReport.totalRevenue)}
                    </p>
                  </div>
                  <button
                    on:click={() => downloadReport(generatedReport.id)}
                    class="uber-btn bg-green-600 hover:bg-green-700"
                  >
                    Pobierz Raport
                  </button>
                </div>
              </div>
            </div>
          {/if}
        </div>

        <!-- Drivers Section -->
        <div class="uber-card">
          <h2 class="text-xl font-bold dark:text-white mb-6">Kierowcy ({drivers.length})</h2>
          
          {#if drivers.length === 0}
            <div class="text-center py-8">
              <p class="text-gray-600 dark:text-gray-400">Brak kierowców w firmie</p>
            </div>
          {:else}
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead class="bg-gray-50 dark:bg-gray-800">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Kierowca
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Status
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Ocena
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Przejazdy
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Zarobki
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700">
                  {#each drivers as driver}
                    <tr>
                      <td class="px-6 py-4 whitespace-nowrap">
                        <div>
                          <div class="text-sm font-medium text-gray-900 dark:text-white">
                            {driver.name} {driver.surname}
                          </div>
                          <div class="text-sm text-gray-500 dark:text-gray-400">
                            {driver.email}
                          </div>
                        </div>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap">
                        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {driver.isAvailable ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                          {driver.isAvailable ? 'Dostępny' : 'Niedostępny'}
                        </span>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                        {driver.starRating.toFixed(1)} ⭐
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                        {driver.totalRides}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                        {formatCurrency(driver.totalEarnings)}
                      </td>
                    </tr>
                  {/each}
                </tbody>
              </table>
            </div>
          {/if}
        </div>
      {/if}
    </div>
  </div>
</div> 