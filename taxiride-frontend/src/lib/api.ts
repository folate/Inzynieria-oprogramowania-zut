import { writable } from 'svelte/store';

// API service for making HTTP requests to the TaxiRide API
const API_URL = 'http://localhost:5145'; // Adjust this to match your API URL

// --- Helper Functions ---

// Helper for making fetch requests
async function fetchAPI(endpoint: string, options: any = {}) {
  const url = `${API_URL}/${endpoint}`;

  const fetchOptions: RequestInit = { ...options };

  // If body is FormData, let the browser set the Content-Type.
  // Otherwise, default to application/json if not specified in options.
  if (!(options.body instanceof FormData)) {
    fetchOptions.headers = {
      'Content-Type': 'application/json',
      ...options.headers
    };
  }

  const response = await fetch(url, fetchOptions);

  if (!response.ok) {
    try {
        const errorBody = await response.json();
        // The backend returns validation errors in an 'errors' object.
        if (errorBody.errors) {
            const errorMessages = Object.values(errorBody.errors).flat().join(' ');
            throw new Error(errorMessages || errorBody.title || 'API request failed');
        }
        throw new Error(errorBody.title || await response.text() || 'API request failed');
    } catch (e: any) {
        // If parsing error JSON fails, fall back to text.
        if (e instanceof Error) {
            throw e;
        }
        const errorText = await response.text();
        throw new Error(errorText || 'API request failed');
    }
  }

  if (options.returnFormat === 'text') {
    return response.text();
  }

  if (options.returnFormat === 'blob') {
    if (!response.ok) {
      // For blob responses, we need to handle errors differently
      const errorText = await response.text();
      try {
        const errorJson = JSON.parse(errorText);
        throw new Error(errorJson.message || 'Download failed');
      } catch (e) {
        if (e instanceof Error) {
          throw e;
        }
        throw new Error(errorText || 'Download failed');
      }
    }
    return response.blob();
  }

  // Handle cases where response body might be empty
  const text = await response.text();
  try {
    return JSON.parse(text);
  } catch (e) {
    return text; // Return text if it's not valid JSON (e.g., "OK")
  }
}

// Helper function to normalize keys to camelCase
function normalizeObjectKeys(obj: any): any {
    if (Array.isArray(obj)) {
        return obj.map(v => normalizeObjectKeys(v));
    } else if (obj !== null && typeof obj === 'object') {
        return Object.keys(obj).reduce((acc, key) => {
            const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
            acc[camelKey] = normalizeObjectKeys(obj[key]);
            return acc;
        }, {} as any);
    }
    return obj;
}


// --- Authentication Store ---

function createAuthStore(key: string) {
  const { subscribe, set } = writable(getInitialUser(key));

  function getInitialUser(storageKey: string) {
    if (typeof window !== 'undefined' && window.sessionStorage) {
      const storedUser = window.sessionStorage.getItem(storageKey);
      if (storedUser) {
        try {
          return JSON.parse(storedUser);
        } catch (error: any) {
          console.error(`Error parsing stored user data from ${storageKey}:`, error.message);
          return null;
        }
      }
    }
    return null;
  }

  return {
    subscribe,
    setUser: (user: any) => {
      set(user);
      if (typeof window !== 'undefined' && window.sessionStorage) {
        try {
          window.sessionStorage.setItem(key, JSON.stringify(user));
        } catch (error: any) {
          console.error('Failed to save user to session storage:', error.message);
        }
      }
    },
    removeUser: () => {
      set(null);
      if (typeof window !== 'undefined' && window.sessionStorage) {
        try {
          window.sessionStorage.removeItem(key);
        } catch (error: any) {
          console.error('Failed to remove user from session storage:', error.message);
        }
      }
    }
  };
}

export const authService = {
  user: createAuthStore('user'),
  rider: createAuthStore('rider'),
  owner: createAuthStore('owner'),
  worker: createAuthStore('worker')
};

// --- API Services ---

// User-related API calls
export const userService = {
  // Login user
  login: async (login: string, password: string) => {
    const formData = new FormData();
    formData.append('login', login);
    formData.append('password', password);
    
    const response = await fetchAPI('User/login', {
      method: 'POST',
      body: formData
    });
    
    if (response && response.userId) {
      const user = { id: response.userId, login: login };
      authService.user.setUser(user);
      return { success: true, message: response.message, userId: response.userId, user: user };
    }
    
    throw new Error(response.message || 'Login failed');
  },
  
  // Register user
  register: async (username: string, password: string, email: string, phoneNumber: string) => {
    const formData = new FormData();
    formData.append('username', username);
    formData.append('password', password);
    formData.append('email', email);
    formData.append('phoneNumber', phoneNumber);
    
    const response = await fetchAPI('User/register', {
      method: 'POST',
      body: formData
    });
    
    if (response && response.userId) {
      const user = { id: response.userId, login: username };
      authService.user.setUser(user);
      return { success: true, message: response.message, userId: response.userId, user: user };
    }
    
    throw new Error(response.message || 'Registration failed');
  },
  
  // Get user profile
  getProfile: async (userId: number) => {
    const response = await fetchAPI(`User/profile/${userId}`);
    return normalizeObjectKeys(response);
  },
  
  // Update user profile
  updateProfile: async (id: number, firstName: string, lastName: string, email: string, phoneNumber: string, preferredPaymentMethod: string) => {
    const formData = new FormData();
    formData.append('id', id.toString());
    formData.append('firstName', firstName);
    formData.append('lastName', lastName);
    formData.append('email', email);
    formData.append('phoneNumber', phoneNumber);
    formData.append('preferredPaymentMethod', preferredPaymentMethod);
    
    return fetchAPI('User/profile/update', {
      method: 'POST',
      body: formData,
      returnFormat: 'text'
    });
  },
  
  // Get user ride history
  getRideHistory: async (userId: number) => {
    const response = await fetchAPI(`User/rides/${userId}`);
    return normalizeObjectKeys(response);
  },
  
  // Rate a ride
  rateRide: async (rideId: number, rating: number, comment: string) => {
    const formData = new FormData();
    formData.append('RideId', rideId.toString());
    formData.append('Rating', rating.toString());
    formData.append('Comment', comment || '');
    
    return fetchAPI('User/ride/rate', { method: 'POST', body: formData });
  },
  
  // File a complaint
  fileComplaint: async (rideId: number, description: string) => {
    const formData = new FormData();
    formData.append('RideId', rideId.toString());
    formData.append('Description', description);
    
    return fetchAPI('User/complaint/file', { method: 'POST', body: formData });
  },
  
  // Process payment
  processPayment: async (rideId: number, paymentMethod: string) => {
    const formData = new FormData();
    formData.append('RideId', rideId.toString());
    formData.append('PaymentMethod', paymentMethod);
    
    return fetchAPI('User/payment/process', { method: 'POST', body: formData });
  }
};

// Rider-related API calls
export const riderService = {
  // Register rider
  register: async (login: string, password: string, name: string, surname: string, phoneNumber: string, email: string) => {
    const formData = new FormData();
    formData.append('login', login);
    formData.append('password', password);
    formData.append('name', name);
    formData.append('surname', surname);
    formData.append('phoneNumber', phoneNumber);
    formData.append('email', email);
    
    const response = await fetchAPI('Rider/register', { method: 'POST', body: formData });
    
    if (response && response.riderId) {
      const rider = { id: response.riderId, login: login, name: name };
      authService.rider.setUser(rider);
      return { success: true, message: response.message, riderId: response.riderId, rider: rider };
    }
    
    throw new Error(response.message || 'Registration failed');
  },
  
  // Login rider
  login: async (login: string, password: string) => {
    const formData = new FormData();
    formData.append('login', login);
    formData.append('password', password);
    
    const response = await fetchAPI('Rider/login', { method: 'POST', body: formData });
    
    if (response && response.riderId) {
      const rider = { id: response.riderId, login: login };
      authService.rider.setUser(rider);
      return { success: true, message: response.message, riderId: response.riderId, rider: rider };
    }
    
    throw new Error(response.message || 'Login failed');
  },

  // Get rider profile
  getProfile: async (id: number) => {
    const response = await fetchAPI(`Rider/profile/${id}`);
    return normalizeObjectKeys(response);
  },

  getById: async (id: number) => {
    const response = await fetchAPI(`Rider/details?id=${id}`);
    return normalizeObjectKeys(response);
  },

  // Get rider statistics
  getStatistics: async (riderId: number) => {
    const response = await fetchAPI(`Rider/statistics/${riderId}`);
    return normalizeObjectKeys(response);
  },

  // Get ride history
  getRideHistory: async (riderId: number) => {
    const response = await fetchAPI(`Rider/rides/${riderId}`);
    return normalizeObjectKeys(response);
  },

  // Get pending rides
  getPendingRides: async (riderId: number) => {
    const response = await fetchAPI(`Rider/pending-rides/${riderId}`);
    return normalizeObjectKeys(response);
  },

  // Get available rides to accept
  getAvailableRides: async (riderId: number) => {
    const response = await fetchAPI(`Rider/available-rides/${riderId}`);
    return normalizeObjectKeys(response);
  },

  updateProfile: async (id: number, name: string, surname: string, phoneNumber: string, email: string) => {
    const formData = new FormData();
    formData.append('id', id.toString());
    formData.append('name', name);
    formData.append('surname', surname);
    formData.append('phoneNumber', phoneNumber);
    formData.append('email', email);
    
    return fetchAPI('Rider/modify', { method: 'POST', body: formData, returnFormat: 'text' });
  },
  
  updateAvailability: async (riderId: number, isAvailable: boolean, latitude: number, longitude: number) => {
    const formData = new FormData();
    formData.append('RiderId', riderId.toString());
    formData.append('IsAvailable', isAvailable.toString());
    formData.append('Latitude', latitude.toString());
    formData.append('Longitude', longitude.toString());
    
    return fetchAPI('Rider/availability', { method: 'POST', body: formData });
  },
  
  getActiveRides: async (riderId: number) => {
    const response = await fetchAPI(`Rider/rides/active/${riderId}`);
    return normalizeObjectKeys(response);
  },
  
  updateRideStatus: async (rideId: number, status: string) => {
    const formData = new FormData();
    formData.append('RideId', rideId.toString());
    formData.append('Status', status);
    
    return fetchAPI('Rider/ride/status', { method: 'POST', body: formData });
  },
  
  updateLocation: async (riderId: number, latitude: number, longitude: number) => {
    const formData = new FormData();
    formData.append('riderId', riderId.toString());
    formData.append('latitude', latitude.toString());
    formData.append('longitude', longitude.toString());
    
    return fetchAPI('Rider/location/update', { method: 'POST', body: formData });
  }
};

// Ride-related API calls
export const rideService = {
  createRide: async (userId: number, pickupLocation: string, dropoffLocation: string) => {
    const formData = new FormData();
    formData.append('UserId', userId.toString());
    formData.append('PickupLocation', pickupLocation);
    formData.append('DropoffLocation', dropoffLocation);
    
    const response = await fetchAPI('RideOrder/create', { method: 'POST', body: formData });
    return normalizeObjectKeys(response);
  },
  
  getRideDetails: async (rideId: number) => {
    const response = await fetchAPI(`RideOrder/details/${rideId}`);
    return normalizeObjectKeys(response);
  },

  // Get ride offers
  getRideOffers: async (rideId: number) => {
    const response = await fetchAPI(`RideOrder/offers/${rideId}`);
    return normalizeObjectKeys(response);
  },

  // Get ride status
  getRideStatus: async (rideId: number) => {
    const response = await fetchAPI(`RideOrder/status/${rideId}`);
    return normalizeObjectKeys(response);
  },

  acceptRideOffer: async (rideId: number, riderId: number) => {
    const formData = new FormData();
    formData.append('rideId', rideId.toString());
    formData.append('riderId', riderId.toString());
    
    return fetchAPI('RideOrder/accept-offer', { method: 'POST', body: formData, returnFormat: 'text' });
  },
  
  cancelRide: async (rideId: number) => {
    const formData = new FormData();
    formData.append('rideId', rideId.toString());
    
    return fetchAPI('RideOrder/cancel', { method: 'POST', body: formData, returnFormat: 'text' });
  },

  getRideEstimate: async (pickupLatitude: number, pickupLongitude: number, dropoffLatitude: number, dropoffLongitude: number) => {
    const params = new URLSearchParams({
        pickupLatitude: pickupLatitude.toString(),
        pickupLongitude: pickupLongitude.toString(),
        dropoffLatitude: dropoffLatitude.toString(),
        dropoffLongitude: dropoffLongitude.toString()
    });
    const response = await fetchAPI(`RideOrder/estimate?${params.toString()}`);
    return normalizeObjectKeys(response);
  }
};

// Offer-related API calls
export const offerService = {
  getAvailableOffers: async (rideId: number) => {
    const response = await fetchAPI(`OfferComparison/offers/${rideId}`);
    return normalizeObjectKeys(response);
  }
};

// Support-related API calls
export const supportService = {
  getTickets: async (userId: number) => {
    const response = await fetchAPI(`Support/tickets/${userId}`);
    return normalizeObjectKeys(response);
  },

  // Get user tickets (alias for getTickets for consistency)
  getUserTickets: async (userId: number) => {
    const response = await fetchAPI(`Support/tickets/${userId}`);
    const normalizedResponse = normalizeObjectKeys(response);
    return normalizedResponse.tickets || [];
  },

  getTicketDetails: async (ticketId: number) => {
    const response = await fetchAPI(`Support/ticket/${ticketId}`);
    return normalizeObjectKeys(response);
  },

  createTicket: async (userId: number, subject: string, description: string) => {
    return fetchAPI('Support/ticket', {
      method: 'POST',
      body: JSON.stringify({ 
        userId, 
        subject, 
        description, 
        priority: 'Medium',
        rideId: null 
      })
    });
  },

  replyToTicket: async (ticketId: number, senderId: number, message: string, isAdmin: boolean) => {
    return fetchAPI(`Support/ticket/${ticketId}/message`, {
      method: 'POST',
      body: JSON.stringify({ 
        senderId, 
        message, 
        isFromSupport: isAdmin 
      })
    });
  },

  getAllTickets: async () => {
    const response = await fetchAPI('Support/tickets/pending');
    return normalizeObjectKeys(response);
  }
};

// Reports API calls
export const reportsService = {
  getReports: async () => {
    const response = await fetchAPI('Reports/list');
    return normalizeObjectKeys(response);
  },

  // Get user reports
  getUserReports: async (userId: number) => {
    const response = await fetchAPI(`Reports/user/${userId}`);
    return normalizeObjectKeys(response);
  },

  // Generate financial report
  generateFinancialReport: async (userId: number, startDate: string, endDate: string, reportType: string, includeVat: boolean = false, exportFormat: string = 'PDF') => {
    const requestBody = {
      startDate: startDate,
      endDate: endDate,
      reportType: reportType,
      includeVat: includeVat,
      exportFormat: exportFormat
    };
    
    const response = await fetchAPI('Reports/financial', {
      method: 'POST',
      body: JSON.stringify(requestBody)
    });
    
    return normalizeObjectKeys(response);
  },

  // Download report file
  downloadReport: async (reportId: number) => {
    const response = await fetchAPI(`Reports/download/${reportId}`, {
      returnFormat: 'blob'
    });
    return response;
  },

  getReportUrl: (reportData: { userType: string; userId: number; startDate?: string; endDate?: string; reportType?: string }) => {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(reportData)) {
      if (value !== undefined) {
        params.append(key, String(value));
      }
    }
    return `${API_URL}/Reports/generate?${params.toString()}`;
  }
};

// Owner-related API calls
export const ownerService = {
  // Register owner
  register: async (login: string, password: string, name: string, surname: string, phoneNumber: string, email: string) => {
    const formData = new FormData();
    formData.append('login', login);
    formData.append('password', password);
    formData.append('name', name);
    formData.append('surname', surname);
    formData.append('phoneNumber', phoneNumber);
    formData.append('email', email);
    
    const response = await fetchAPI('Owner/register', { method: 'POST', body: formData });
    
    if (response && response.ownerId) {
      const owner = { id: response.ownerId, login: login, name: name };
      authService.owner.setUser(owner);
      return { success: true, message: response.message, ownerId: response.ownerId, owner: owner };
    }
    
    throw new Error(response.message || 'Registration failed');
  },
  
  // Login owner
  login: async (login: string, password: string) => {
    const formData = new FormData();
    formData.append('login', login);
    formData.append('password', password);
    
    const response = await fetchAPI('Owner/login', { method: 'POST', body: formData });
    
    if (response && response.ownerId) {
      const owner = { id: response.ownerId, login: login };
      authService.owner.setUser(owner);
      return { success: true, message: response.message, ownerId: response.ownerId, owner: owner };
    }
    
    throw new Error(response.message || 'Login failed');
  },

  // Get owner profile
  getProfile: async (id: number) => {
    const response = await fetchAPI(`Owner/profile/${id}`);
    return normalizeObjectKeys(response);
  },

  // Get drivers for company
  getDrivers: async (companyId: number) => {
    const response = await fetchAPI(`Owner/drivers/${companyId}`);
    return normalizeObjectKeys(response);
  },

  // Generate financial report
  generateFinancialReport: async (ownerId: number, companyId: number, startDate: string, endDate: string, reportType: string, includeVat: boolean, exportFormat: string) => {
    const requestBody = {
      ownerId: ownerId,
      companyId: companyId,
      startDate: startDate,
      endDate: endDate,
      reportType: reportType,
      includeVat: includeVat,
      exportFormat: exportFormat
    };
    
    const response = await fetchAPI('Owner/financial-report', {
      method: 'POST',
      body: JSON.stringify(requestBody)
    });
    
    return normalizeObjectKeys(response);
  },

  // Download report
  downloadReport: async (reportId: number) => {
    const response = await fetchAPI(`Owner/download-report/${reportId}`, {
      returnFormat: 'blob'
    });
    return response;
  }
};

// Worker-related API calls
export const workerService = {
  // Register worker
  register: async (login: string, password: string, name: string, surname: string, phoneNumber: string, email: string, department: string) => {
    const formData = new FormData();
    formData.append('login', login);
    formData.append('password', password);
    formData.append('name', name);
    formData.append('surname', surname);
    formData.append('phoneNumber', phoneNumber);
    formData.append('email', email);
    formData.append('department', department);
    
    const response = await fetchAPI('Worker/register', { method: 'POST', body: formData });
    
    if (response && response.workerId) {
      const worker = { id: response.workerId, login: login, name: name };
      authService.worker.setUser(worker);
      return { success: true, message: response.message, workerId: response.workerId, worker: worker };
    }
    
    throw new Error(response.message || 'Registration failed');
  },
  
  // Login worker
  login: async (login: string, password: string) => {
    const formData = new FormData();
    formData.append('login', login);
    formData.append('password', password);
    
    const response = await fetchAPI('Worker/login', { method: 'POST', body: formData });
    
    if (response && response.workerId) {
      const worker = { id: response.workerId, login: login };
      authService.worker.setUser(worker);
      return { success: true, message: response.message, workerId: response.workerId, worker: worker };
    }
    
    throw new Error(response.message || 'Login failed');
  },

  // Get worker profile
  getProfile: async (id: number) => {
    const response = await fetchAPI(`Worker/profile/${id}`);
    return normalizeObjectKeys(response);
  },

  // Get assigned tickets
  getAssignedTickets: async (workerId: number) => {
    const response = await fetchAPI(`Worker/tickets/assigned/${workerId}`);
    return normalizeObjectKeys(response);
  },

  // Get unassigned tickets
  getUnassignedTickets: async () => {
    const response = await fetchAPI('Worker/tickets/unassigned');
    return normalizeObjectKeys(response);
  },

  // Assign ticket to worker
  assignTicket: async (ticketId: number, workerId: number) => {
    return fetchAPI(`Worker/ticket/${ticketId}/assign/${workerId}`, { method: 'POST' });
  },

  // Reply to ticket
  replyToTicket: async (ticketId: number, workerId: number, message: string) => {
    const requestBody = {
      workerId: workerId,
      message: message
    };
    
    return fetchAPI(`Worker/ticket/${ticketId}/reply`, {
      method: 'POST',
      body: JSON.stringify(requestBody)
    });
  },

  // Update ticket status
  updateTicketStatus: async (ticketId: number, status: string, resolution?: string, refundAmount?: number) => {
    const requestBody = {
      status: status,
      resolution: resolution,
      refundAmount: refundAmount
    };
    
    return fetchAPI(`Worker/ticket/${ticketId}/status`, {
      method: 'PUT',
      body: JSON.stringify(requestBody)
    });
  },

  // Get worker statistics
  getStatistics: async () => {
    const response = await fetchAPI('Worker/statistics');
    return normalizeObjectKeys(response);
  }
};

// Generic error handler for Svelte components
export function handleError(error: any): string {
    if (error instanceof Error) {
        return error.message;
    }
    return 'An unknown error occurred.';
}
