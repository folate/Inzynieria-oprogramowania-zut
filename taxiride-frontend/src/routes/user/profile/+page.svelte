<script lang="ts">
  import { onMount } from 'svelte';
  import { userService, authService } from '$lib/api';
  import { authStore, logout } from '$lib/auth';
  import { goto } from '$app/navigation';
  
  let user: any = null;
  let loading = true;
  let error = '';
  let success = '';
  let editing = false;
  
  // Form fields
  let firstName = '';
  let lastName = '';
  let email = '';
  let phoneNumber = '';
  let preferredPaymentMethod = '';
  
  onMount(() => {
    // Sprawdź czy użytkownik jest zalogowany jako użytkownik
    const unsubscribe = authStore.subscribe(async state => {
      if (!state.isAuthenticated || state.userType !== 'user') {
        goto('/user/login');
        return;
      }
      
      const currentUser: any = state.user;
      if (currentUser) {
        try {
          user = await userService.getProfile(currentUser.id);
          // Fill form fields z obsługą camelCase/PascalCase
          firstName = user.firstName ?? user.FirstName ?? '';
          lastName = user.lastName ?? user.LastName ?? '';
          email = user.email ?? user.Email ?? '';
          phoneNumber = user.phoneNumber ?? user.phone ?? user.PhoneNumber ?? user.Phone ?? '';
          preferredPaymentMethod = user.preferredPaymentMethod ?? user.PreferredPaymentMethod ?? '';
        } catch (err) {
          error = 'Failed to load profile. Please try again.';
        } finally {
          loading = false;
        }
      }
    });
    
    return unsubscribe;
  });
  
  async function handleUpdateProfile(e: Event) {
    e.preventDefault();
    error = '';
    success = '';
    try {
      await userService.updateProfile(
        user.id ?? user.Id,
        firstName,
        lastName,
        email,
        phoneNumber,
        preferredPaymentMethod
      );
      // Refresh user data
      const updated = await userService.getProfile(user.id ?? user.Id);
      firstName = updated.firstName ?? updated.FirstName ?? '';
      lastName = updated.lastName ?? updated.LastName ?? '';
      email = updated.email ?? updated.Email ?? '';
      phoneNumber = updated.phoneNumber ?? updated.phone ?? updated.PhoneNumber ?? updated.Phone ?? '';
      preferredPaymentMethod = updated.preferredPaymentMethod ?? updated.PreferredPaymentMethod ?? '';
      success = 'Profile updated successfully!';
      editing = false;
    } catch (err) {
      error = (err && typeof err === 'object' && 'message' in err) ? String(err.message) : 'Failed to update profile. Please try again.';
    }
  }
  
  function handleLogout() {
    logout();
    goto('/');
  }
</script>

<svelte:head>
  <title>Profil użytkownika - TaxiRide</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 dark:bg-[#18181c] py-8 transition-colors duration-300">
  <div class="container mx-auto px-4 max-w-2xl">
    <div class="uber-card uber-fadein mb-8">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-4">Mój profil</h1>
      {#if loading}
        <div class="uber-skeleton h-32 w-full mb-4"></div>
      {:else}
        <form class="space-y-6" on:submit|preventDefault={handleUpdateProfile}>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Imię</label>
            <input type="text" bind:value={firstName} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nazwisko</label>
            <input type="text" bind:value={lastName} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">E-mail</label>
            <input type="email" bind:value={email} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Telefon</label>
            <input type="tel" bind:value={phoneNumber} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Preferowana metoda płatności</label>
            <select bind:value={preferredPaymentMethod} class="w-full px-4 py-3 border border-gray-700 dark:border-gray-600 rounded-xl bg-gray-100 dark:bg-[#232329] text-gray-900 dark:text-white focus:ring-2 focus:ring-[#1a1a1a] focus:border-transparent text-lg" required>
              <option value="">Wybierz...</option>
              <option value="Credit Card">Karta kredytowa</option>
              <option value="Cash">Gotówka</option>
              <option value="BLIK">BLIK</option>
              <option value="Apple Pay">Apple Pay</option>
              <option value="Google Pay">Google Pay</option>
            </select>
          </div>
          <button type="submit" class="uber-btn w-full">Zapisz zmiany</button>
        </form>
      {/if}
    </div>
  </div>
</div>
