import api from './api';

const mapAuthPayload = (data) => ({
  email: data.email,
  fullName: data.fullName,
  role: data.role ?? 'Customer',
  expiresAt: data.expiresAt,
});

export const authService = {
  register: async (fullName, email, password, role = 'Customer') => {
    const response = await api.post('/api/auth/register', {
      fullName,
      email,
      password,
      role,
    });

    if (response.data.token) {
      const user = mapAuthPayload(response.data);
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(user));
      return { ...response.data, user };
    }

    return response.data;
  },

  login: async (email, password) => {
    const response = await api.post('/api/auth/login', { email, password });

    if (response.data.token) {
      const user = mapAuthPayload(response.data);
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(user));
      return { ...response.data, user };
    }

    return response.data;
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getCurrentUser: () => {
    const userStr = localStorage.getItem('user');
    if (!userStr || userStr === 'undefined') {
      return null;
    }

    try {
      return JSON.parse(userStr);
    } catch {
      localStorage.removeItem('user');
      return null;
    }
  },

  isAuthenticated: () => !!localStorage.getItem('token'),
};
