import api from './api';

const pick = (data, camelKey, pascalKey) => data?.[camelKey] ?? data?.[pascalKey];

const mapAuthPayload = (data) => ({
  email: pick(data, 'email', 'Email'),
  fullName: pick(data, 'fullName', 'FullName'),
  role: pick(data, 'role', 'Role') ?? 'Customer',
  expiresAt: pick(data, 'expiresAt', 'ExpiresAt'),
});

const extractToken = (data) => pick(data, 'token', 'Token');

export const authService = {
  register: async (fullName, email, password, role = 'Customer') => {
    const response = await api.post('/api/auth/register', {
      fullName,
      email,
      password,
      role,
    });

    const token = extractToken(response.data);

    if (token) {
      const user = mapAuthPayload(response.data);
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify(user));
      return { ...response.data, user };
    }

    throw new Error('Resposta de registro sem token de autenticação.');
  },

  login: async (email, password) => {
    const response = await api.post('/api/auth/login', { email, password });

    const token = extractToken(response.data);

    if (token) {
      const user = mapAuthPayload(response.data);
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify(user));
      return { ...response.data, user };
    }

    throw new Error('Resposta de login sem token de autenticação.');
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
