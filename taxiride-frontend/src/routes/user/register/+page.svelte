<script lang="ts">
  import { userService, authService } from '$lib/api';
  import { loginUser, authStore } from '$lib/auth';
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  
  let username = '';
  let password = '';
  let confirmPassword = '';
  let email = '';
  let phoneNumber = '';
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
  
  async function handleRegister(e: Event) {
    e.preventDefault();
    // Validation
    if (!username || !password) {
      error = 'Username and password are required';
      return;
    }
    
    if (password !== confirmPassword) {
      error = 'Passwords do not match';
      return;
    }
    
    error = '';
    loading = true;
    
    try {
      const response = await userService.register(username, password, email, phoneNumber);
      // Use the auth system to properly set the user state
      loginUser({ id: response.userId, login: username });
      goto('/user/dashboard');
    } catch (err) {
      error = (err && typeof err === 'object' && 'message' in err) ? String(err.message) : 'Registration failed. Please try again.';
    } finally {
      loading = false;
    }
  }
</script>

<svelte:head>
  <title>Rejestracja - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] flex items-center justify-center transition-colors duration-300">
  <div class="uber-card uber-fadein w-full max-w-md">
    <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-6 text-center">Rejestracja</h1>
    <form class="space-y-6" on:submit|preventDefault={handleRegister}>
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nazwa użytkownika</label>
        <input type="text" bind:value={username} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" required />
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">E-mail</label>
        <input type="email" bind:value={email} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" required />
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Telefon</label>
        <input type="tel" bind:value={phoneNumber} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" />
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Hasło</label>
        <input type="password" bind:value={password} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" required />
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Powtórz hasło</label>
        <input type="password" bind:value={confirmPassword} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" required />
      </div>
      {#if error}
        <div class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 rounded-md mt-4 text-center" role="alert">
          <p>{error}</p>
        </div>
      {/if}
      <button type="submit" class="uber-btn w-full" disabled={loading}>{loading ? 'Rejestracja...' : 'Zarejestruj się'}</button>
    </form>
    <div class="mt-6 text-center">
      <a href="/user/login" class="text-blue-500 hover:underline">Masz już konto? Zaloguj się</a>
    </div>
  </div>
</div>
