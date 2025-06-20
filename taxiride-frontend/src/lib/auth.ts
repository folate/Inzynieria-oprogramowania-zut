import { writable } from 'svelte/store';
import { authService } from './api';

// Create a store for authentication state
export const authStore = writable({
  isAuthenticated: false,
  user: null,
  rider: null,
  owner: null,
  worker: null,
  userType: null as string | null // 'user', 'rider', 'owner', or 'worker'
});

// Initialize the store from sessionStorage on app load
export function initAuth() {
  // Subscribe to all auth stores to detect authentication
  const unsubscribeUser = authService.user.subscribe((user: any) => {
    if (user) {
      authStore.set({
        isAuthenticated: true,
        user,
        rider: null,
        owner: null,
        worker: null,
        userType: 'user'
      });
    }
  });

  const unsubscribeRider = authService.rider.subscribe((rider: any) => {
    if (rider) {
      authStore.set({
        isAuthenticated: true,
        user: null,
        rider,
        owner: null,
        worker: null,
        userType: 'rider'
      });
    }
  });

  const unsubscribeOwner = authService.owner.subscribe((owner: any) => {
    if (owner) {
      authStore.set({
        isAuthenticated: true,
        user: null,
        rider: null,
        owner,
        worker: null,
        userType: 'owner'
      });
    }
  });

  const unsubscribeWorker = authService.worker.subscribe((worker: any) => {
    if (worker) {
      authStore.set({
        isAuthenticated: true,
        user: null,
        rider: null,
        owner: null,
        worker,
        userType: 'worker'
      });
    }
  });

  // Return unsubscribe functions
  return () => {
    unsubscribeUser();
    unsubscribeRider();
    unsubscribeOwner();
    unsubscribeWorker();
  };
}

// Login functions
export function loginUser(userData: any) {
  authService.user.setUser(userData);
  authStore.set({
    isAuthenticated: true,
    user: userData,
    rider: null,
    owner: null,
    worker: null,
    userType: 'user'
  });
}

export function loginRider(riderData: any) {
  authService.rider.setUser(riderData);
  authStore.set({
    isAuthenticated: true,
    user: null,
    rider: riderData,
    owner: null,
    worker: null,
    userType: 'rider'
  });
}

export function loginOwner(ownerData: any) {
  authService.owner.setUser(ownerData);
  authStore.set({
    isAuthenticated: true,
    user: null,
    rider: null,
    owner: ownerData,
    worker: null,
    userType: 'owner'
  });
}

export function loginWorker(workerData: any) {
  authService.worker.setUser(workerData);
  authStore.set({
    isAuthenticated: true,
    user: null,
    rider: null,
    owner: null,
    worker: workerData,
    userType: 'worker'
  });
}

// Logout function
export function logout() {
  authService.user.removeUser();
  authService.rider.removeUser();
  authService.owner.removeUser();
  authService.worker.removeUser();
  authStore.set({
    isAuthenticated: false,
    user: null,
    rider: null,
    owner: null,
    worker: null,
    userType: null
  });
}
