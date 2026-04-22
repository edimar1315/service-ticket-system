import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ticketService } from '../services/ticketService';
import Navbar from '../components/Navbar';
import Card from '../components/Card';
import Input from '../components/Input';
import Select from '../components/Select';
import Button from '../components/Button';

const CreateTicket = () => {
  const [formData, setFormData] = useState({
    clientName: '',
    description: '',
    priority: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const priorityOptions = [
    { value: 'low', label: 'Baixa' },
    { value: 'medium', label: 'Média' },
    { value: 'high', label: 'Alta' },
  ];

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      await ticketService.create(formData);
      navigate('/tickets');
    } catch (err) {
      setError(err.response?.data?.message || 'Erro ao criar ticket');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <div className="max-w-3xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <h1 className="text-3xl font-bold text-gray-900 mb-8">Criar Novo Ticket</h1>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
              {error}
            </div>
          )}

          <Card>
            <form onSubmit={handleSubmit}>
              <Input
                label="Nome do Cliente"
                name="clientName"
                value={formData.clientName}
                onChange={handleChange}
                placeholder="Nome completo do cliente"
                required
              />

              <div className="mb-4">
                <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                  Descrição do Problema <span className="text-red-500">*</span>
                </label>
                <textarea
                  id="description"
                  name="description"
                  value={formData.description}
                  onChange={handleChange}
                  rows={5}
                  required
                  minLength={10}
                  maxLength={1000}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                  placeholder="Descreva o problema em detalhes (mínimo 10 caracteres)..."
                />
              </div>

              <Select
                label="Prioridade"
                name="priority"
                value={formData.priority}
                onChange={handleChange}
                options={priorityOptions}
                required
              />

              <div className="flex gap-4 mt-6">
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? 'Criando...' : 'Criar Ticket'}
                </Button>
                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => navigate('/tickets')}
                >
                  Cancelar
                </Button>
              </div>
            </form>
          </Card>
        </div>
      </div>
    </div>
  );
};

export default CreateTicket;
