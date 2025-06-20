<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { riderService } from '$lib/api';
  import { loginRider } from '$lib/auth';
  import { authService } from '$lib/api';
  import { authStore } from '$lib/auth';

  let login: string = '';
  let password: string = '';
  let loading: boolean = false;
  let error: string | null = null;
  let success: string | null = null;

  onMount(() => {
    const unsubscribe = authStore.subscribe(state => {
      if (state.user) goto('/user/dashboard');
      if (state.rider) goto('/user/dashboard');
      if (state.owner) goto('/owner/dashboard');
      if (state.worker) goto('/worker/dashboard');
    });
    
    return unsubscribe;
  });

  async function handleLogin() {
    if (!login || !password) {
      error = 'Proszę wypełnić wszystkie pola';
      return;
    }

    loading = true;
    error = null;

    try {
      const response = await riderService.login(login, password);
      
      // Zapisz dane kierowcy w localStorage
      const riderData = {
        id: response.riderId,
        login: login,
        name: response.message?.split(' ')[1] || '',
        surname: response.message?.split(' ')[2]?.replace('!', '') || ''
      };
      
      // Użyj nowego systemu auth
      loginRider(riderData);
      
      success = 'Zalogowano pomyślnie!';
      
      // Przekieruj do panelu kierowcy
      setTimeout(() => {
        goto('/dashboard');
      }, 1000);
      
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd logowania';
    } finally {
      loading = false;
    }
  }

  function handleRegister() {
    goto('/rider/register');
  }
</script>

<svelte:head>
  <title>Logowanie kierowcy - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
  <div class="max-w-md w-full space-y-8">
    <div class="text-center">
      <div class="flex justify-center">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 text-[#1a1a1a] dark:text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="10"></circle>
          <path d="M8 14s1.5 2 4 2 4-2 4-2"></path>
          <line x1="9" y1="9" x2="9.01" y2="9"></line>
          <line x1="15" y1="9" x2="15.01" y2="9"></line>
        </svg>
      </div>
      <h2 class="mt-6 text-3xl font-bold text-gray-900 dark:text-white">Zaloguj się jako kierowca</h2>
      <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
        Dostęp do panelu kierowcy
      </p>
    </div>

    {#if error}
      <div class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg p-4">
        <p class="text-red-700 dark:text-red-200">{error}</p>
      </div>
    {/if}

    {#if success}
      <div class="bg-green-50 dark:bg-green-900/30 border border-green-200 dark:border-green-700 rounded-lg p-4">
        <p class="text-green-700 dark:text-green-200">{success}</p>
      </div>
    {/if}

    <form class="mt-8 space-y-6" on:submit|preventDefault={handleLogin}>
      <div class="space-y-4">
        <div>
          <label for="login" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Login
          </label>
          <input
            id="login"
            name="login"
            type="text"
            bind:value={login}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
            placeholder="Wprowadź swój login"
          />
        </div>
        <div>
          <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Hasło
          </label>
          <input
            id="password"
            name="password"
            type="password"
            bind:value={password}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
            placeholder="Wprowadź swoje hasło"
          />
        </div>
      </div>

      <div>
        <button
          type="submit"
          disabled={loading}
          class="uber-btn w-full flex justify-center items-center"
        >
          {#if loading}
            <span class="uber-skeleton w-6 h-6 rounded-full mr-3"></span>
            Logowanie...
          {:else}
            Zaloguj się
          {/if}
        </button>
      </div>

      <div class="text-center">
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Nie masz jeszcze konta?
          <button
            type="button"
            on:click={handleRegister}
            class="font-medium text-[#1a1a1a] dark:text-white hover:underline"
          >
            Zarejestruj się
          </button>
        </p>
      </div>
    </form>

    <div class="text-center">
      <a
        href="/user/login"
        class="text-sm text-gray-600 dark:text-gray-400 hover:text-[#1a1a1a] dark:hover:text-white"
      >
        ← Wróć do logowania użytkownika
      </a>
    </div>
  </div>
</div> 