import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ticketService } from '../services/ticketService';
import Navbar from '../components/Navbar';
import Card from '../components/Card';
import Button from '../components/Button';
import Select from '../components/Select';

const TicketDetails = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [ticket, setTicket] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [isUpdatingStatus, setIsUpdatingStatus] = useState(false);

  useEffect(() => {
    loadTicket();
  }, [id]);

  const loadTicket = async () => {
    setIsLoading(true);
    setError('');

    try {
      const data = await ticketService.getById(id);
      setTicket(data);
    } catch (err) {
      setError('Erro ao carregar ticket');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleStatusChange = async (newStatus) => {
    setIsUpdatingStatus(true);
    setError('');

    try {
      await ticketService.updateStatus(id, newStatus);
      // Recarregar ticket atualizado do backend
      await loadTicket();
    } catch (err) {
      setError('Erro ao atualizar status');
      console.error(err);
    } finally {
      setIsUpdatingStatus(false);
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
    };
    return colors[priority] || 'text-gray-600';
  };

  const statusOptions = [
    { value: 'open', label: 'Aberto' },
    { value: 'inProgress', label: 'Em Progresso' },
    { value: 'resolved', label: 'Finalizado' },
    { value: 'closed', label: 'Cancelado' },
  ];

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

  if (!ticket) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="px-4 py-6 sm:px-0">
            <p className="text-gray-500">Ticket não encontrado</p>
            <Button onClick={() => navigate('/tickets')} className="mt-4">
              Voltar para lista
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <div className="max-w-5xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="flex justify-between items-center mb-8">
            <h1 className="text-3xl font-bold text-gray-900">
              Ticket #{ticket.id}
            </h1>
            <div className="flex gap-2">
              <Button 
                variant="secondary" 
                onClick={() => navigate('/tickets')}
              >
                Voltar
              </Button>
              <Button onClick={() => navigate(`/tickets/${id}/edit`)}>
                Editar
              </Button>
            </div>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
              {error}
            </div>
          )}

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 space-y-6">
              <Card>
                <h2 className="text-2xl font-semibold text-gray-900 mb-4">
                  {ticket.clientName}
                </h2>
                <div className="prose max-w-none">
                  <p className="text-gray-700 whitespace-pre-wrap">
                    {ticket.problemDescription}
                  </p>
                </div>
              </Card>

              <Card>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Histórico de Atividades
                </h3>
                <div className="space-y-4">
                  <div className="flex items-start">
                    <div className="flex-shrink-0">
                      <div className="h-8 w-8 rounded-full bg-primary-100 flex items-center justify-center">
                        <svg className="h-4 w-4 text-primary-600" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-11a1 1 0 10-2 0v2H7a1 1 0 100 2h2v2a1 1 0 102 0v-2h2a1 1 0 100-2h-2V7z" clipRule="evenodd" />
                        </svg>
                      </div>
                    </div>
                    <div className="ml-3">
                      <p className="text-sm text-gray-900">Ticket criado</p>
                      <p className="text-sm text-gray-500">
                        {new Date(ticket.createdAt).toLocaleString('pt-BR')}
                      </p>
                    </div>
                  </div>
                </div>
              </Card>
            </div>

            <div className="space-y-6">
              <Card>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Detalhes
                </h3>
                <dl className="space-y-4">
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Status</dt>
                    <dd className="mt-1">
                      <span className={`px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusColor(ticket.status)}`}>
                        {getStatusLabel(ticket.status)}
                      </span>
                    </dd>
                  </div>
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Prioridade</dt>
                    <dd className={`mt-1 text-sm font-medium capitalize ${getPriorityColor(ticket.priority)}`}>
                      {ticket.priority}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Criado em</dt>
                    <dd className="mt-1 text-sm text-gray-900">
                      {new Date(ticket.createdAt).toLocaleString('pt-BR')}
                    </dd>
                  </div>
                  {ticket.updatedAt && (
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Atualizado em</dt>
                      <dd className="mt-1 text-sm text-gray-900">
                        {new Date(ticket.updatedAt).toLocaleString('pt-BR')}
                      </dd>
                    </div>
                  )}
                </dl>
              </Card>

              <Card>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Atualizar Status
                </h3>
                <Select
                  name="status"
                  value={ticket.status}
                  onChange={(e) => handleStatusChange(e.target.value)}
                  options={statusOptions}
                />
                {isUpdatingStatus && (
                  <p className="mt-2 text-sm text-gray-500">Atualizando...</p>
                )}
              </Card>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TicketDetails;
