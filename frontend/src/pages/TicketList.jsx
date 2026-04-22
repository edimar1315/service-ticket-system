import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ticketService } from '../services/ticketService';
import Navbar from '../components/Navbar';
import Card from '../components/Card';
import Button from '../components/Button';

const TicketList = () => {
  const [tickets, setTickets] = useState([]);
  const [filteredTickets, setFilteredTickets] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const navigate = useNavigate();

  useEffect(() => {
    loadTickets();
  }, []);

  useEffect(() => {
    filterTickets();
  }, [statusFilter, tickets]);

  const loadTickets = async () => {
    setIsLoading(true);
    setError('');

    try {
      const data = await ticketService.getAll();
      setTickets(Array.isArray(data) ? data : []);
    } catch (err) {
      setError('Erro ao carregar tickets');
      console.error(err);
      setTickets([]);
    } finally {
      setIsLoading(false);
    }
  };

  const filterTickets = () => {
    if (!Array.isArray(tickets)) {
      setFilteredTickets([]);
      return;
    }

    if (statusFilter === 'all') {
      setFilteredTickets(tickets);
    } else {
      setFilteredTickets(tickets.filter(t => t.status === statusFilter));
    }
  };

  const getStatusColor = (status) => {
    const colors = {
      open: 'bg-yellow-100 text-yellow-800',
      pending: 'bg-yellow-100 text-yellow-800',
      inProgress: 'bg-blue-100 text-blue-800',
      resolved: 'bg-green-100 text-green-800',
      finished: 'bg-green-100 text-green-800',
      closed: 'bg-gray-100 text-gray-800',
      cancelled: 'bg-gray-100 text-gray-800',
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  const getStatusLabel = (status) => {
    const labels = {
      open: 'Aberto',
      pending: 'Aberto',
      inProgress: 'Em Progresso',
      resolved: 'Finalizado',
      finished: 'Finalizado',
      closed: 'Cancelado',
      cancelled: 'Cancelado',
    };
    return labels[status] || status;
  };

  const getPriorityColor = (priority) => {
    const colors = {
      low: 'text-green-600',
      medium: 'text-yellow-600',
      high: 'text-red-600',
      critical: 'text-red-800',
    };
    return colors[priority] || 'text-gray-600';
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="flex items-center justify-center h-96">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="flex justify-between items-center mb-8">
            <h1 className="text-3xl font-bold text-gray-900">Tickets</h1>
            <Button onClick={() => navigate('/tickets/new')}>
              + Novo Ticket
            </Button>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
              {error}
            </div>
          )}

          <Card className="mb-6">
            <div className="flex gap-2">
              <button
                onClick={() => setStatusFilter('all')}
                className={`px-4 py-2 rounded-md ${
                  statusFilter === 'all'
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
              >
                Todos ({Array.isArray(tickets) ? tickets.length : 0})
              </button>
              <button
                onClick={() => setStatusFilter('open')}
                className={`px-4 py-2 rounded-md ${
                  statusFilter === 'open'
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
              >
                Abertos ({Array.isArray(tickets) ? tickets.filter(t => t.status === 'open').length : 0})
              </button>
              <button
                onClick={() => setStatusFilter('inProgress')}
                className={`px-4 py-2 rounded-md ${
                  statusFilter === 'inProgress'
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
              >
                Em Progresso ({Array.isArray(tickets) ? tickets.filter(t => t.status === 'inProgress').length : 0})
              </button>
              <button
                onClick={() => setStatusFilter('resolved')}
                className={`px-4 py-2 rounded-md ${
                  statusFilter === 'resolved'
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
              >
                Finalizados ({Array.isArray(tickets) ? tickets.filter(t => t.status === 'resolved').length : 0})
              </button>
            </div>
          </Card>

          <Card>
            {filteredTickets.length === 0 ? (
              <p className="text-gray-500 text-center py-8">
                {statusFilter === 'all' 
                  ? 'Nenhum ticket encontrado' 
                  : `Nenhum ticket ${getStatusLabel(statusFilter).toLowerCase()}`}
              </p>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        ID
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Cliente
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Status
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Prioridade
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Criado em
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Ações
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {filteredTickets.map((ticket) => (
                      <tr key={ticket.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          #{ticket.id.substring(0, 8)}
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-900">
                          {ticket.clientName}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusColor(ticket.status)}`}>
                            {getStatusLabel(ticket.status)}
                          </span>
                        </td>
                        <td className={`px-6 py-4 whitespace-nowrap text-sm font-medium capitalize ${getPriorityColor(ticket.priority)}`}>
                          {ticket.priority}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {new Date(ticket.createdAt).toLocaleDateString('pt-BR')}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                          <button
                            onClick={() => navigate(`/tickets/${ticket.id}`)}
                            className="text-primary-600 hover:text-primary-900 mr-4"
                          >
                            Ver
                          </button>
                          <button
                            onClick={() => navigate(`/tickets/${ticket.id}/edit`)}
                            className="text-blue-600 hover:text-blue-900">
                            Editar
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        </div>
      </div>
    </div>
  );
};

export default TicketList;
