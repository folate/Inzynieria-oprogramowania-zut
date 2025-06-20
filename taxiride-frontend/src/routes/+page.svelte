<script lang="ts">
  import { onMount } from 'svelte';
  import { authService } from '$lib/api';
  import { authStore } from '$lib/auth';
  import { goto } from '$app/navigation';

  let currentUser: any = null;
  let userType: string | null = null;

  onMount(() => {
    // Subskrybuj do zmian w auth store
    const unsubscribe = authStore.subscribe(state => {
      currentUser = state.user || state.rider;
      userType = state.userType;
      
      // Jeśli użytkownik jest zalogowany, przekieruj do odpowiedniego panelu
      if (state.isAuthenticated && currentUser) {
        if (userType === 'user') {
          goto('/user/dashboard');
        } else if (userType === 'rider') {
          goto('/dashboard');
        }
      }
    });

    // Cleanup subscription
    return unsubscribe;
  });

  function handleBookNow() {
    window.location.href = '/rides/order';
  }
  function handleShowRideDetails(rideId: number) {
    window.location.href = `/rides/${rideId}`;
  }
</script>

<svelte:head>
  <title>Strona Główna - TaxiRide</title>
</svelte:head>

<!-- ZAWSZE publiczny widok, bez warunków na isAuthenticated -->
<!-- Hero section for all users -->
<div class="relative h-screen max-h-[800px] overflow-hidden">
  <!-- Background image with slider effect -->
  <div class="absolute inset-0 z-0">
    <div class="relative h-full">
      <img 
        src="https://images.unsplash.com/photo-1527525443983-6e60c75fff46?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2070&q=80" 
        alt="Taxi w mieście" 
        class="w-full h-full object-cover"
      />
      <div class="absolute inset-0 bg-gradient-to-r from-black via-black/70 to-transparent"></div>
    </div>
  </div>

  <!-- Content overlay -->
  <div class="container mx-auto px-4 h-full relative z-10">
    <div class="flex items-center h-full">
      <div class="max-w-2xl text-white">
        <h1 class="text-5xl md:text-6xl font-bold mb-6">
          <span class="block">Przejazdy gdy</span>
          <span class="block">ich potrzebujesz</span>
        </h1>
        <p class="text-xl mb-10 text-gray-200">Zamów przejazd, wsiądź i zrelaksuj się. Twój cel jest tylko jednym dotknięciem.</p>
        
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 max-w-2xl">
          <a href="/user/login" class="bg-white text-black font-medium px-6 py-3 rounded-lg hover:bg-gray-100 transition flex items-center justify-center">
            <span>Zaloguj się</span>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 ml-2" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M12.293 5.293a1 1 0 011.414 0l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-2.293-2.293a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </a>
          <a href="/user/register" class="bg-blue-600 text-white font-medium px-6 py-3 rounded-lg hover:bg-blue-700 transition flex items-center justify-center">
            <span>Zarejestruj się</span>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 ml-2" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-11a1 1 0 10-2 0v2H7a1 1 0 100 2h2v2a1 1 0 102 0v-2h2a1 1 0 100-2h-2V7z" clip-rule="evenodd" />
            </svg>
          </a>
          <a href="/rider/login" class="bg-black text-white border border-white font-medium px-6 py-3 rounded-lg hover:bg-gray-900 transition flex items-center justify-center">
            <span>Kieruj z nami</span>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 ml-2" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M12.293 5.293a1 1 0 011.414 0l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-2.293-2.293a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </a>
        </div>
        
        <!-- Additional navigation for owners and workers -->
        <div class="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4 max-w-2xl">
          <a href="/owner/login" class="bg-green-600 text-white font-medium px-6 py-3 rounded-lg hover:bg-green-700 transition flex items-center justify-center">
            <span>Panel właściciela</span>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 ml-2" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clip-rule="evenodd" />
            </svg>
          </a>
          <a href="/worker/login" class="bg-purple-600 text-white font-medium px-6 py-3 rounded-lg hover:bg-purple-700 transition flex items-center justify-center">
            <span>Panel pracownika</span>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 ml-2" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-6-3a2 2 0 11-4 0 2 2 0 014 0zm-2 4a5 5 0 00-4.546 2.916A5.986 5.986 0 0010 16a5.986 5.986 0 004.546-2.084A5 5 0 0010 11z" clip-rule="evenodd" />
            </svg>
          </a>
        </div>
      </div>
    </div>
  </div>
  
  <!-- Bottom wave shape -->
  <div class="absolute bottom-0 left-0 right-0">
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1440 120" class="w-full">
      <path fill="#ffffff" fill-opacity="1" d="M0,64L80,69.3C160,75,320,85,480,80C640,75,800,53,960,48C1120,43,1280,53,1360,58.7L1440,64L1440,120L1360,120C1280,120,1120,120,960,120C800,120,640,120,480,120C320,120,160,120,80,120L0,120Z"></path>
    </svg>
  </div>
</div>

<!-- Features section -->
<div class="py-20 bg-white">
  <div class="container mx-auto px-4">
    <h2 class="text-3xl md:text-4xl font-bold text-center mb-16">Jak działa TaxiRide</h2>
    
    <div class="grid md:grid-cols-3 gap-12">
      <div class="text-center">
        <div class="bg-black text-white p-6 inline-block rounded-full mb-6">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </div>
        <h3 class="text-xl font-bold mb-3">Zamów przejazd</h3>
        <p class="text-gray-600">Wybierz miejsce odbioru i cel, a następnie zamów przejazd.</p>
      </div>
      
      <div class="text-center">
        <div class="bg-black text-white p-6 inline-block rounded-full mb-6">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h3 class="text-xl font-bold mb-3">Śledź kierowcę</h3>
        <p class="text-gray-600">Obserwuj przybycie kierowcy w czasie rzeczywistym i ciesz się komfortową jazdą.</p>
      </div>
      
      <div class="text-center">
        <div class="bg-black text-white p-6 inline-block rounded-full mb-6">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
          </svg>
        </div>
        <h3 class="text-xl font-bold mb-3">Zapłać i idź</h3>
        <p class="text-gray-600">Płatność jest obsługiwana automatycznie, a paragon jest wysyłany do Ciebie.</p>
      </div>
    </div>
  </div>
</div>

<!-- App showcase section -->
<div class="py-20 bg-gray-50">
  <div class="container mx-auto px-4">
    <div class="flex flex-col md:flex-row items-center">
      <div class="md:w-1/2 mb-10 md:mb-0 md:pr-10">
        <h2 class="text-3xl md:text-4xl font-bold mb-6">Pobierz aplikację TaxiRide</h2>
        <p class="text-xl text-gray-600 mb-8">Uzyskaj pełne wrażenia na swoim smartfonie. Zamawiaj przejazdy, śledź kierowcę i zarządzaj kontem wszystko z jednej aplikacji.</p>
        
        <div class="flex flex-wrap gap-4">
          <a href="#" class="bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-900 inline-flex items-center">
            <svg class="h-6 w-6 mr-2" viewBox="0 0 24 24" fill="currentColor">
              <path d="M17.707,9.293l-5-5c-0.391-0.391-1.023-0.391-1.414,0l-5,5C5.898,9.688,6.19,10.3,6.707,10.293H10v9 c0,0.553,0.447,1,1,1h2c0.553,0,1-0.447,1-1v-9h3.293C17.807,10.3,18.098,9.688,17.707,9.293z"></path>
            </svg>
            Pobierz teraz
          </a>
        </div>
      </div>
      <div class="md:w-1/2">
        <img 
          src="https://images.unsplash.com/photo-1614064641938-3bbee52942c7?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=987&q=80" 
          alt="Aplikacja mobilna TaxiRide" 
          class="rounded-xl shadow-xl"
        />
      </div>
    </div>
  </div>
</div>

<!-- Testimonials section -->
<div class="py-20 bg-white">
  <div class="container mx-auto px-4">
    <h2 class="text-3xl md:text-4xl font-bold text-center mb-16">Co mówią nasi klienci</h2>
    
    <div class="grid md:grid-cols-3 gap-8">
      <div class="bg-white p-8 rounded-lg shadow-sm border border-gray-100">
        <div class="flex items-center mb-4">
          <div class="text-yellow-400 flex">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
          </div>
        </div>
        <p class="text-gray-600 mb-4">"TaxiRide to przełom w moich codziennych dojazdach. Kierowcy są zawsze profesjonalni i punktualni."</p>
        <div class="flex items-center">
          <div class="h-10 w-10 rounded-full bg-gray-300 mr-3 overflow-hidden">
            <img src="https://randomuser.me/api/portraits/women/32.jpg" alt="Klientka" class="h-full w-full object-cover" />
          </div>
          <p class="font-medium">Emilia Kowalska</p>
        </div>
      </div>
      
      <div class="bg-white p-8 rounded-lg shadow-sm border border-gray-100">
        <div class="flex items-center mb-4">
          <div class="text-yellow-400 flex">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
          </div>
        </div>
        <p class="text-gray-600 mb-4">"Jako kierowca mogę zarabiać dodatkowe pieniądze według własnego harmonogramu. Platforma jest prosta i wydajna."</p>
        <div class="flex items-center">
          <div class="h-10 w-10 rounded-full bg-gray-300 mr-3 overflow-hidden">
            <img src="https://randomuser.me/api/portraits/men/44.jpg" alt="Kierowca" class="h-full w-full object-cover" />
          </div>
          <p class="font-medium">Michał Nowak</p>
        </div>
      </div>
      
      <div class="bg-white p-8 rounded-lg shadow-sm border border-gray-100">
        <div class="flex items-center mb-4">
          <div class="text-yellow-400 flex">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
          </div>
        </div>
        <p class="text-gray-600 mb-4">"Czuję się bezpiecznie i komfortowo za każdym razem, gdy korzystam z TaxiRide. Aplikacja jest intuicyjna, a obsługa klienta doskonała."</p>
        <div class="flex items-center">
          <div class="h-10 w-10 rounded-full bg-gray-300 mr-3 overflow-hidden">
            <img src="https://randomuser.me/api/portraits/women/68.jpg" alt="Klientka" class="h-full w-full object-cover" />
          </div>
          <p class="font-medium">Anna Wiśniewska</p>
        </div>
      </div>
    </div>
  </div>
</div>
