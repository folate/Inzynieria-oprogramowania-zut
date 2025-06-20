<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { authStore } from '../../../lib/auth';
  import { ownerService } from '../../../lib/api';

  let login = '';
  let password = '';
  let loading = false;
  let error: string | null = null;
  let showRegister = false;

  // Registration form data
  let registerData = {
    login: '',
    password: '',
    confirmPassword: '',
    name: '',
    surname: '',
    phoneNumber: '',
    email: ''
  };

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
      error = 'Wypełnij wszystkie pola';
      return;
    }

    loading = true;
    error = null;

    try {
      await ownerService.login(login, password);
      goto('/owner/dashboard');
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd logowania';
    } finally {
      loading = false;
    }
  }

  async function handleRegister() {
    if (!registerData.login || !registerData.password || !registerData.confirmPassword || 
        !registerData.name || !registerData.surname || !registerData.phoneNumber || !registerData.email) {
      error = 'Wypełnij wszystkie pola';
      return;
    }

    if (registerData.password !== registerData.confirmPassword) {
      error = 'Hasła nie są identyczne';
      return;
    }

    loading = true;
    error = null;

    try {
      await ownerService.register(
        registerData.login,
        registerData.password,
        registerData.name,
        registerData.surname,
        registerData.phoneNumber,
        registerData.email
      );
      
      // Auto-login after successful registration
      await ownerService.login(registerData.login, registerData.password);
      goto('/owner/dashboard');
    } catch (err: unknown) {
      error = err instanceof Error ? err.message : 'Błąd rejestracji';
    } finally {
      loading = false;
    }
  }
</script>

<svelte:head>
  <title>Logowanie Właściciela - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
  <div class="max-w-md w-full space-y-8">
    <div>
      <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900 dark:text-white">
        {showRegister ? 'Rejestracja Właściciela' : 'Logowanie Właściciela'}
      </h2>
      <p class="mt-2 text-center text-sm text-gray-600 dark:text-gray-400">
        {showRegister ? 'Utwórz konto właściciela' : 'Zaloguj się do panelu właściciela'}
      </p>
    </div>

    {#if error}
      <div class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg p-4">
        <p class="text-red-700 dark:text-red-200">{error}</p>
      </div>
    {/if}

    {#if showRegister}
      <!-- Registration Form -->
      <form class="uber-card space-y-6" on:submit|preventDefault={handleRegister}>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label for="name" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Imię</label>
            <input
              id="name"
              type="text"
              bind:value={registerData.name}
              required
              class="uber-input"
              placeholder="Jan"
            />
          </div>
          <div>
            <label for="surname" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Nazwisko</label>
            <input
              id="surname"
              type="text"
              bind:value={registerData.surname}
              required
              class="uber-input"
              placeholder="Kowalski"
            />
          </div>
        </div>

        <div>
          <label for="reg-login" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Login</label>
          <input
            id="reg-login"
            type="text"
            bind:value={registerData.login}
            required
            class="uber-input"
            placeholder="jan.kowalski"
          />
        </div>

        <div>
          <label for="reg-email" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Email</label>
          <input
            id="reg-email"
            type="email"
            bind:value={registerData.email}
            required
            class="uber-input"
            placeholder="jan@example.com"
          />
        </div>

        <div>
          <label for="reg-phone" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Telefon</label>
          <input
            id="reg-phone"
            type="tel"
            bind:value={registerData.phoneNumber}
            required
            class="uber-input"
            placeholder="+48 123 456 789"
          />
        </div>

        <div>
          <label for="reg-password" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Hasło</label>
          <input
            id="reg-password"
            type="password"
            bind:value={registerData.password}
            required
            class="uber-input"
            placeholder="••••••••"
          />
        </div>

        <div>
          <label for="confirm-password" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Potwierdź hasło</label>
          <input
            id="confirm-password"
            type="password"
            bind:value={registerData.confirmPassword}
            required
            class="uber-input"
            placeholder="••••••••"
          />
        </div>

        <div class="flex gap-4">
          <button
            type="submit"
            disabled={loading}
            class="uber-btn flex-1"
          >
            {#if loading}
              <span class="uber-skeleton w-6 h-6 rounded-full mr-3"></span>
            {/if}
            {loading ? 'Rejestracja...' : 'Zarejestruj się'}
          </button>
          <button
            type="button"
            on:click={() => showRegister = false}
            class="uber-btn flex-1 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600"
          >
            Anuluj
          </button>
        </div>
      </form>
    {:else}
      <!-- Login Form -->
      <form class="uber-card space-y-6" on:submit|preventDefault={handleLogin}>
        <div>
          <label for="login" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Login</label>
          <input
            id="login"
            type="text"
            bind:value={login}
            required
            class="uber-input"
            placeholder="jan.kowalski"
          />
        </div>

        <div>
          <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Hasło</label>
          <input
            id="password"
            type="password"
            bind:value={password}
            required
            class="uber-input"
            placeholder="••••••••"
          />
        </div>

        <div class="flex gap-4">
          <button
            type="submit"
            disabled={loading}
            class="uber-btn flex-1"
          >
            {#if loading}
              <span class="uber-skeleton w-6 h-6 rounded-full mr-3"></span>
            {/if}
            {loading ? 'Logowanie...' : 'Zaloguj się'}
          </button>
          <button
            type="button"
            on:click={() => showRegister = true}
            class="uber-btn flex-1 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600"
          >
            Rejestracja
          </button>
        </div>
      </form>
    {/if}

    <div class="text-center">
      <a href="/" class="text-sm text-blue-600 dark:text-blue-400 hover:underline">
        Powrót do strony głównej
      </a>
    </div>
  </div>
</div> 