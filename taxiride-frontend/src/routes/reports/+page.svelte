<script lang="ts">
  import { onMount } from 'svelte';
  import { reportsService } from '$lib/api';
  import { authStore } from '$lib/auth';
  
  let reports: any[] = [];
  let loading = false;
  let error: string | null = null;
  let showNewReportForm = false;
  
  // New report form data
  let newReport = {
    startDate: '',
    endDate: '',
    reportType: 'Detailed',
    includeVat: false,
    exportFormat: 'PDF'
  };
  
  let currentUser: any = null;
  
  onMount(() => {
    // Check if user is logged in
    const unsubscribe = authStore.subscribe(async state => {
      if (!state.isAuthenticated) {
        return;
      }
      
      currentUser = state.user || state.rider;
      if (currentUser) {
        await loadReports();
        // Set default dates (last 30 days)
        const today = new Date();
        const thirtyDaysAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);
        newReport.endDate = today.toISOString().split('T')[0];
        newReport.startDate = thirtyDaysAgo.toISOString().split('T')[0];
      }
    });
    
    return unsubscribe;
  });
  
  async function loadReports() {
    if (!currentUser) return;
    
    loading = true;
    error = null;
    
    try {
      const response = await reportsService.getUserReports(currentUser.id);
      reports = response.reports || [];
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd pobierania raportów';
      reports = [];
    } finally {
      loading = false;
    }
  }
  
  async function generateReport() {
    if (!currentUser) return;
    loading = true;
    error = null;
    try {
      await reportsService.generateFinancialReport(
        currentUser.id,
        newReport.startDate,
        newReport.endDate,
        newReport.reportType,
        newReport.includeVat,
        newReport.exportFormat
      );
      showNewReportForm = false;
      await loadReports();
    } catch (err: any) {
      error = err.message;
    } finally {
      loading = false;
    }
  }
  
  async function downloadReport(reportId: number) {
    try {
      const blob = await reportsService.downloadReport(reportId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `report_${reportId}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err: any) {
      console.error('Download error:', err);
      if (err.message) {
        error = err.message;
      } else if (err.status === 404) {
        error = 'Raport nie został znaleziony';
      } else {
        error = 'Błąd podczas pobierania raportu';
      }
    }
  }
  
  function formatDate(dateString: string) {
    return new Date(dateString).toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
  
  function getReportTypeLabel(type: string) {
    switch (type) {
      case 'Summary': return 'Podsumowanie';
      case 'Detailed': return 'Szczegółowy';
      case 'Financial': return 'Finansowy';
      default: return type;
    }
  }
</script>

<svelte:head>
  <title>Raporty finansowe - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-3xl">
    <div class="flex justify-between items-center mb-8">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Raporty finansowe</h1>
      <button on:click={() => showNewReportForm = !showNewReportForm} class="uber-btn">
        {showNewReportForm ? 'Anuluj' : 'Nowy raport'}
      </button>
    </div>
    {#if error}
      <div class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg p-4 mb-6">
        <p class="text-red-700 dark:text-red-200">{error}</p>
      </div>
    {/if}
    {#if showNewReportForm}
      <div class="uber-card uber-fadein mb-8">
        <h2 class="text-xl font-bold mb-4 dark:text-white">Nowy raport</h2>
        <form on:submit|preventDefault={generateReport} class="space-y-4">
          <div class="flex gap-4">
            <div>
              <label for="startDate" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Data od</label>
              <input id="startDate" type="date" bind:value={newReport.startDate} required class="w-full px-4 py-2 border rounded-xl" />
            </div>
            <div>
              <label for="endDate" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Data do</label>
              <input id="endDate" type="date" bind:value={newReport.endDate} required class="w-full px-4 py-2 border rounded-xl" />
            </div>
          </div>
          <div>
            <label for="reportType" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Typ raportu</label>
            <select id="reportType" bind:value={newReport.reportType} class="w-full px-4 py-2 border rounded-xl">
              <option value="Detailed">Szczegółowy raport przychodów</option>
              <option value="Summary">Podsumowanie</option>
              <option value="Financial">Finansowy</option>
            </select>
          </div>
          <div class="flex items-center gap-2">
            <input id="includeVat" type="checkbox" bind:checked={newReport.includeVat} />
            <label for="includeVat" class="text-sm">Uwzględnij podatek VAT</label>
          </div>
          <div>
            <label for="exportFormat" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Format eksportu</label>
            <select id="exportFormat" bind:value={newReport.exportFormat} class="w-full px-4 py-2 border rounded-xl">
              <option value="PDF">PDF</option>
              <option value="CSV">CSV</option>
            </select>
          </div>
          <button type="submit" class="uber-btn w-full">Generuj raport</button>
        </form>
      </div>
    {/if}
    <div class="uber-card">
      <h2 class="text-xl font-bold mb-4 dark:text-white">Twoje raporty</h2>
      {#if loading}
        <div class="uber-skeleton h-16 w-full mb-4"></div>
      {:else if reports.length === 0}
        <p class="text-gray-500 dark:text-gray-300">Brak raportów do wyświetlenia.</p>
      {:else}
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead>
            <tr>
              <th class="px-4 py-2 text-left">Data wygenerowania</th>
              <th class="px-4 py-2 text-left">Zakres</th>
              <th class="px-4 py-2 text-left">Typ</th>
              <th class="px-4 py-2 text-left">Akcje</th>
            </tr>
          </thead>
          <tbody>
            {#each reports as report}
              <tr>
                <td class="px-4 py-2">{formatDate(report.generatedDate)}</td>
                <td class="px-4 py-2">{formatDate(report.startDate)} - {formatDate(report.endDate)}</td>
                <td class="px-4 py-2">{getReportTypeLabel(report.reportType)}</td>
                <td class="px-4 py-2">
                  <button class="uber-btn" on:click={() => downloadReport(report.id)}>Pobierz</button>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      {/if}
    </div>
  </div>
</div>
