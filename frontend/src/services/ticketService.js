import api from './api';

export const ticketService = {
  getAll: async () => {
    const response = await api.get('/api/tickets');
    const tickets = response.data.data || [];

    // Backend envia strings via .ToString(): 'Open','InProgress','Finished','Cancelled'
    const statusMap = {
      'Open': 'open',
      'InProgress': 'inProgress',
      'Finished': 'resolved',
      'Cancelled': 'closed'
    };

    // Backend envia strings via .ToString(): 'Low','Medium','High','Critical'
    const priorityMap = {
      'Low': 'low',
      'Medium': 'medium',
      'High': 'high',
      'Critical': 'critical'
    };

    return tickets.map(ticket => ({
      ...ticket,
      status: statusMap[ticket.status] ?? 'open',
      priority: priorityMap[ticket.priority] ?? 'medium'
    }));
  },

  getById: async (id) => {
    const response = await api.get(`/api/tickets/${id}`);
    const ticket = response.data;

    // Backend envia strings via .ToString(): 'Open','InProgress','Finished','Cancelled'
    const statusMap = {
      'Open': 'open',
      'InProgress': 'inProgress',
      'Finished': 'resolved',
      'Cancelled': 'closed'
    };

    // Backend envia strings via .ToString(): 'Low','Medium','High','Critical'
    const priorityMap = {
      'Low': 'low',
      'Medium': 'medium',
      'High': 'high',
      'Critical': 'critical'
    };

    return {
      ...ticket,
      status: statusMap[ticket.status] ?? 'open',
      priority: priorityMap[ticket.priority] ?? 'medium'
    };
  },

  create: async (ticketData) => {
    // Mapear prioridades de string para número conforme enum do backend
    const priorityMap = {
      'low': 1,      // Low = 1
      'medium': 2,   // Medium = 2
      'high': 3      // High = 3
    };

    // Transformar dados do frontend para formato do backend
    const payload = {
      clientName: ticketData.clientName,
      problemDescription: ticketData.description,
      priority: priorityMap[ticketData.priority] || 2
    };

    const response = await api.post('/api/tickets', payload);
    return response.data;
  },

  update: async (id, ticketData) => {
    const response = await api.put(`/api/tickets/${id}`, ticketData);
    return response.data;
  },

  updateStatus: async (id, status) => {
    // Mapear status frontend (string) → backend (int)
    const statusMap = {
      'open': 1,
      'inProgress': 2,
      'resolved': 3,
      'closed': 4
    };

    const numericStatus = statusMap[status];

    if (!numericStatus) {
      throw new Error(`Status inválido: ${status}`);
    }

    const response = await api.patch(`/api/tickets/${id}/status`, { status: numericStatus });
    return response.data;
  },

  delete: async (id) => {
    const response = await api.delete(`/api/tickets/${id}`);
    return response.data;
  },

  getMetrics: async () => {
    const response = await api.get('/api/tickets/metrics');
    return response.data;
  },
};
