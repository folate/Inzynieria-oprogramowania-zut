<script lang="ts">
  import { userService } from '$lib/api';
  import { loginUser } from '$lib/auth';
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  import { authService } from '$lib/api';
  import { authStore } from '$lib/auth';
  
  let login = '';
  let password = '';
  let error = '';
  let loading = false;
  
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
      error = 'Please enter both username and password';
      return;
    }
    
    error = '';
    loading = true;
    
    try {
      const response = await userService.login(login, password);
      loginUser({ id: response.userId, login: login });
      goto('/user/dashboard');
    } catch (err) {
      error = (err && typeof err === 'object' && 'message' in err) ? String(err.message) : 'Login failed. Please check your credentials.';
    } finally {
      loading = false;
    }
  }
</script>

<svelte:head>
  <title>Logowanie - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] flex items-center justify-center transition-colors duration-300">
  <div class="uber-card uber-fadein w-full max-w-md">
    <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-6 text-center">Logowanie</h1>
    <form on:submit|preventDefault={handleLogin} class="space-y-6">
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">E-mail</label>
        <input
          id="login"
          name="login"
          type="text"
          required
          bind:value={login}
          class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
          placeholder="Username"
        />
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Hasło</label>
        <input
          id="password"
          name="password"
          type="password"
          required
          bind:value={password}
          class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg"
          placeholder="Password"
        />
      </div>
      <button type="submit" class="uber-btn w-full">Zaloguj się</button>
    </form>
    {#if error}
      <div class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 rounded-md mt-4 text-center" role="alert">
        <p>{error}</p>
      </div>
    {/if}
    <div class="mt-6 text-center">
      <a href="/user/register" class="text-blue-500 hover:underline">Nie masz konta? Zarejestruj się</a>
    </div>
  </div>
</div>
