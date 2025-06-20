<script lang="ts">
  import "../app.css";
  import { onMount } from 'svelte';
  import { authStore, initAuth, logout } from '$lib/auth';
  import { goto } from '$app/navigation';
  
  let isAuthenticated = false;
  let userType: string | null = null;
  let mobileMenuOpen = false;
  let currentUser: any = null;
  
  // Subscribe to auth store changes
  authStore.subscribe(state => {
    isAuthenticated = state.isAuthenticated;
    userType = state.userType;
    currentUser = state.user || state.rider;
  });
  
  // Initialize auth state on component mount
  onMount(() => {
    initAuth();
  });
  
  function handleLogout() {
    logout();
    goto('/');
  }
  
  function toggleMobileMenu() {
    mobileMenuOpen = !mobileMenuOpen;
  }
</script>

<div class="min-h-screen bg-gray-50 flex flex-col">
  <nav class="bg-black text-white shadow-lg sticky top-0 z-50">
    <div class="container mx-auto px-4">
      <div class="flex justify-between items-center py-4">
        <div class="flex items-center space-x-2">
          <a href="/" class="flex items-center">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 mr-2" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"></circle>
              <path d="M8 14s1.5 2 4 2 4-2 4-2"></path>
              <line x1="9" y1="9" x2="9.01" y2="9"></line>
              <line x1="15" y1="9" x2="15.01" y2="9"></line>
            </svg>
            <span class="text-2xl font-bold tracking-tight">TaxiRide</span>
          </a>
        </div>
        
        <div class="hidden md:flex items-center space-x-8">
          <a href="/" class="hover:text-gray-300 transition font-medium">Strona główna</a>
          
          {#if isAuthenticated}
            {#if userType === 'user'}
              <a href="/user/dashboard" class="hover:text-gray-300 transition font-medium">Panel użytkownika</a>
              <a href="/rides/order" class="hover:text-gray-300 transition font-medium">Zamów przejazd</a>
              <a href="/user/rides" class="hover:text-gray-300 transition font-medium">Moje przejazdy</a>
              <a href="/support" class="hover:text-gray-300 transition font-medium">Wsparcie</a>
              <a href="/user/profile" class="hover:text-gray-300 transition font-medium">Profil</a>
              {#if currentUser && currentUser.login === 'admin'}
                <a href="/support/admin" class="hover:text-red-400 transition font-medium">Panel wsparcia</a>
              {/if}
            {:else if userType === 'rider'}
              <a href="/dashboard" class="hover:text-gray-300 transition font-medium">Panel kierowcy</a>
              <a href="/reports" class="hover:text-gray-300 transition font-medium">Raporty</a>
              <a href="/support" class="hover:text-gray-300 transition font-medium">Wsparcie</a>
            {/if}
            <button on:click={handleLogout} class="bg-white text-black font-medium px-4 py-2 rounded-full hover:bg-gray-200 transition">Wyloguj</button>
          {:else}
            <a href="/user/login" class="hover:text-gray-300 transition font-medium">Jazda</a>
            <a href="/rider/login" class="hover:text-gray-300 transition font-medium">Kierowanie</a>
            <a href="/user/login" class="bg-white text-black font-medium px-4 py-2 rounded-full hover:bg-gray-200 transition">Zaloguj</a>
          {/if}
        </div>
        
        <div class="md:hidden">
          <button class="focus:outline-none" on:click={toggleMobileMenu} aria-label="Przełącz menu mobilne">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>
      </div>
      
      {#if mobileMenuOpen}
        <div class="md:hidden py-3 pb-4 border-t border-gray-700">
          <a href="/" class="block py-2 hover:text-gray-300 transition">Strona główna</a>
          
          {#if isAuthenticated}
            {#if userType === 'user'}
              <a href="/user/dashboard" class="block py-2 hover:text-gray-300 transition">Panel użytkownika</a>
              <a href="/rides/order" class="block py-2 hover:text-gray-300 transition">Zamów przejazd</a>
              <a href="/user/rides" class="block py-2 hover:text-gray-300 transition">Moje przejazdy</a>
              <a href="/support" class="block py-2 hover:text-gray-300 transition">Wsparcie</a>
              <a href="/user/profile" class="block py-2 hover:text-gray-300 transition">Profil</a>
              {#if currentUser && currentUser.login === 'admin'}
                <a href="/support/admin" class="block py-2 hover:text-red-400 transition">Panel wsparcia</a>
              {/if}
            {:else if userType === 'rider'}
              <a href="/dashboard" class="block py-2 hover:text-gray-300 transition">Panel kierowcy</a>
              <a href="/reports" class="block py-2 hover:text-gray-300 transition">Raporty</a>
              <a href="/support" class="block py-2 hover:text-gray-300 transition">Wsparcie</a>
            {/if}
            <button on:click={handleLogout} class="block py-2 w-full text-left hover:text-gray-300 transition">Wyloguj</button>
          {:else}
            <a href="/user/login" class="block py-2 hover:text-gray-300 transition">Jazda</a>
            <a href="/rider/login" class="block py-2 hover:text-gray-300 transition">Kierowanie</a>
            <a href="/user/login" class="block py-2 hover:text-gray-300 transition font-bold">Zaloguj</a>
          {/if}
        </div>
      {/if}
    </div>
  </nav>

  <main class="flex-grow">
    <slot />
  </main>

  <footer class="bg-black text-white py-12">
    <div class="container mx-auto px-4">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
        <div>
          <h3 class="text-lg font-bold mb-4">TaxiRide</h3>
          <ul class="space-y-2">
            <li><a href="/about" class="text-gray-400 hover:text-white transition">O nas</a></li>
            <li><a href="/offers" class="text-gray-400 hover:text-white transition">Nasze usługi</a></li>
            <li><a href="/careers" class="text-gray-400 hover:text-white transition">Kariera</a></li>
          </ul>
        </div>
        <div>
          <h3 class="text-lg font-bold mb-4">Produkty</h3>
          <ul class="space-y-2">
            <li><a href="/rides" class="text-gray-400 hover:text-white transition">Przejazd</a></li>
            <li><a href="/drive" class="text-gray-400 hover:text-white transition">Kierowanie</a></li>
            <li><a href="/business" class="text-gray-400 hover:text-white transition">Biznes</a></li>
          </ul>
        </div>
        <div>
          <h3 class="text-lg font-bold mb-4">Wsparcie</h3>
          <ul class="space-y-2">
            <li><a href="/help" class="text-gray-400 hover:text-white transition">Centrum pomocy</a></li>
            <li><a href="/safety" class="text-gray-400 hover:text-white transition">Bezpieczeństwo</a></li>
            <li><a href="/contact" class="text-gray-400 hover:text-white transition">Skontaktuj się z nami</a></li>
          </ul>
        </div>
        <div>
          <h3 class="text-lg font-bold mb-4">Śledź nas</h3>
          <div class="flex space-x-4">
            <a href="#" class="text-gray-400 hover:text-white transition">
              <svg class="h-6 w-6" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path fill-rule="evenodd" d="M22 12c0-5.523-4.477-10-10-10S2 6.477 2 12c0 4.991 3.657 9.128 8.438 9.878v-6.987h-2.54V12h2.54V9.797c0-2.506 1.492-3.89 3.777-3.89 1.094 0 2.238.195 2.238.195v2.46h-1.26c-1.243 0-1.63.771-1.63 1.562V12h2.773l-.443 2.89h-2.33v6.988C18.343 21.128 22 16.991 22 12z" clip-rule="evenodd" />
              </svg>
            </a>
            <a href="#" class="text-gray-400 hover:text-white transition">
              <svg class="h-6 w-6" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path fill-rule="evenodd" d="M12.315 2c2.43 0 2.784.013 3.808.06 1.064.049 1.791.218 2.427.465a4.902 4.902 0 011.772 1.153 4.902 4.902 0 011.153 1.772c.247.636.416 1.363.465 2.427.048 1.067.06 1.407.06 4.123v.08c0 2.643-.012 2.987-.06 4.043-.049 1.064-.218 1.791-.465 2.427a4.902 4.902 0 01-1.153 1.772 4.902 4.902 0 01-1.772 1.153c-.636.247-1.363.416-2.427.465-1.067.048-1.407.06-4.123.06h-.08c-2.643 0-2.987-.012-4.043-.06-1.064-.049-1.791-.218-2.427-.465a4.902 4.902 0 01-1.772-1.153 4.902 4.902 0 01-1.153-1.772c-.247-.636-.416-1.363-.465-2.427-.047-1.024-.06-1.379-.06-3.808v-.63c0-2.43.013-2.784.06-3.808.049-1.064.218-1.791.465-2.427a4.902 4.902 0 011.153-1.772A4.902 4.902 0 015.45 2.525c.636-.247 1.363-.416 2.427-.465C8.901 2.013 9.256 2 11.685 2h.63zm-.081 1.802h-.468c-2.456 0-2.784.011-3.807.058-.975.045-1.504.207-1.857.344-.467.182-.8.398-1.15.748-.35.35-.566.683-.748 1.15-.137.353-.3.882-.344 1.857-.047 1.023-.058 1.351-.058 3.807v.468c0 2.456.011 2.784.058 3.807.045.975.207 1.504.344 1.857.182.466.399.8.748 1.15.35.35.683.566 1.15.748.353.137.882.3 1.857.344 1.054.048 1.37.058 4.041.058h.08c2.597 0 2.917-.01 3.96-.058.976-.045 1.505-.207 1.858-.344.466-.182.8-.398 1.15-.748.35-.35.566-.683.748-1.15.137-.353.3-.882.344-1.857.048-1.055.058-1.37.058-4.041v-.08c0-2.597-.01-2.917-.058-3.96-.045-.976-.207-1.505-.344-1.858a3.097 3.097 0 00-.748-1.15 3.098 3.098 0 00-1.15-.748c-.353-.137-.882-.3-1.857-.344-1.023-.047-1.351-.058-3.807-.058zM12 6.865a5.135 5.135 0 110 10.27 5.135 5.135 0 010-10.27zm0 1.802a3.333 3.333 0 100 6.666 3.333 3.333 0 000-6.666zm5.338-3.205a1.2 1.2 0 110 2.4 1.2 1.2 0 010-2.4z" clip-rule="evenodd" />
              </svg>
            </a>
            <a href="#" class="text-gray-400 hover:text-white transition">
              <svg class="h-6 w-6" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path d="M8.29 20.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0022 5.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.072 4.072 0 012.8 9.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 012 18.407a11.616 11.616 0 006.29 1.84" />
              </svg>
            </a>
          </div>
        </div>
      </div>
      <div class="mt-12 pt-8 border-t border-gray-800 text-center text-gray-400">
        <p>© 2025 TaxiRide. Wszystkie prawa zastrzeżone.</p>
      </div>
    </div>
  </footer>
</div>
