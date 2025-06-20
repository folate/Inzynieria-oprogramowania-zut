<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { riderService, authService } from '$lib/api';
  import { authStore } from '$lib/auth';

  let formData = {
    login: '',
    password: '',
    name: '',
    surname: '',
    phoneNumber: '',
    email: '',
    licenseNumber: '',
    vehicleInfo: ''
  };
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

  async function handleRegister() {
    if (!formData.login || !formData.password || !formData.name || !formData.surname || 
        !formData.phoneNumber || !formData.email) {
      error = 'Proszę wypełnić wszystkie pola';
      return;
    }

    loading = true;
    error = null;

    try {
      const response = await riderService.register(
        formData.login,
        formData.password,
        formData.name,
        formData.surname,
        formData.phoneNumber,
        formData.email
      );
      
      success = 'Konto zostało utworzone pomyślnie! Możesz się teraz zalogować.';
      
      // Przekieruj do logowania po 2 sekundach
      setTimeout(() => {
        goto('/rider/login');
      }, 2000);
      
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd rejestracji';
    } finally {
      loading = false;
    }
  }

  function handleLogin() {
    goto('/rider/login');
  }
</script>

<svelte:head>
  <title>Rejestracja kierowcy - TaxiRide</title>
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
      <h2 class="mt-6 text-3xl font-bold text-gray-900 dark:text-white">Zarejestruj się jako kierowca</h2>
      <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
        Dołącz do naszej floty kierowców
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

    <form class="mt-8 space-y-6" on:submit|preventDefault={handleRegister}>
      <div class="space-y-4">
        <div>
          <label for="login" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Login *
          </label>
          <input
            id="login"
            name="login"
            type="text"
            bind:value={formData.login}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
            placeholder="Wprowadź login"
          />
        </div>
        <div>
          <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Hasło *
          </label>
          <input
            id="password"
            name="password"
            type="password"
            bind:value={formData.password}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
            placeholder="Wprowadź hasło"
          />
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label for="name" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Imię *
            </label>
            <input
              id="name"
              name="name"
              type="text"
              bind:value={formData.name}
              required
              class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
              placeholder="Imię"
            />
          </div>
          <div>
            <label for="surname" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Nazwisko *
            </label>
            <input
              id="surname"
              name="surname"
              type="text"
              bind:value={formData.surname}
              required
              class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
              placeholder="Nazwisko"
            />
          </div>
        </div>
        <div>
          <label for="phoneNumber" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Numer telefonu *
          </label>
          <input
            id="phoneNumber"
            name="phoneNumber"
            type="tel"
            bind:value={formData.phoneNumber}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
            placeholder="+48 123 456 789"
          />
        </div>
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Email *
          </label>
          <input
            id="email"
            name="email"
            type="email"
            bind:value={formData.email}
            required
            class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
            placeholder="email@example.com"
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
            Rejestracja...
          {:else}
            Zarejestruj się
          {/if}
        </button>
      </div>

      <div class="text-center">
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Masz już konto?
          <button
            type="button"
            on:click={handleLogin}
            class="font-medium text-[#1a1a1a] dark:text-white hover:underline"
          >
            Zaloguj się
          </button>
        </p>
      </div>
    </form>

    <div class="text-center">
      <a
        href="/user/register"
        class="text-sm text-gray-600 dark:text-gray-400 hover:text-[#1a1a1a] dark:hover:text-white"
      >
        ← Wróć do rejestracji użytkownika
      </a>
    </div>
  </div>
</div> 