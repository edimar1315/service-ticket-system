import Card from './Card';

const AnalystTicketsCard = ({ analysts, isLoading }) => {
  const getStatusColor = (status) => {
    const colors = {
      'Open': 'bg-yellow-100 text-yellow-800',
      'InProgress': 'bg-blue-100 text-blue-800',
      'Finished': 'bg-green-100 text-green-800',
      'Cancelled': 'bg-gray-100 text-gray-800',
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  const getStatusLabel = (status) => {
    const labels = {
      'Open': 'Aberto',
      'InProgress': 'Em Progresso',
      'Finished': 'Finalizado',
      'Cancelled': 'Cancelado',
    };
    return labels[status] || status;
  };

  const getPriorityColor = (priority) => {
    const colors = {
      'Low': 'bg-blue-100 text-blue-800',
      'Medium': 'bg-yellow-100 text-yellow-800',
      'High': 'bg-orange-100 text-orange-800',
      'Critical': 'bg-red-100 text-red-800',
    };
    return colors[priority] || 'bg-gray-100 text-gray-800';
  };

  const getPriorityLabel = (priority) => {
    const labels = {
      'Low': 'Baixa',
      'Medium': 'Média',
      'High': 'Alta',
      'Critical': 'Crítica',
    };
    return labels[priority] || priority;
  };

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-12 bg-gray-200 rounded"></div>
        <div className="h-24 bg-gray-200 rounded"></div>
      </div>
    );
  }

  if (!analysts || analysts.length === 0) {
    return (
      <Card>
        <div className="text-center py-8 text-gray-500">
          <p>Nenhum analista com tickets atribuídos</p>
        </div>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      {analysts.map((analyst) => (
        <Card key={analyst.analystId}>
          <div className="space-y-4">
            {/* Cabeçalho do analista */}
            <div className="border-b pb-4">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">
                    {analyst.analystName}
                  </h3>
                  <p className="text-sm text-gray-500">{analyst.analystEmail}</p>
                </div>
                <div className="text-right">
                  <div className="text-2xl font-bold text-primary-600">
                    {analyst.totalTickets}
                  </div>
                  <p className="text-xs text-gray-500">tickets</p>
                </div>
              </div>

              {/* Estatísticas */}
              <div className="mt-3 grid grid-cols-3 gap-2">
                <div className="bg-yellow-50 rounded p-2 text-center">
                  <div className="text-lg font-semibold text-yellow-700">
                    {analyst.openCount}
                  </div>
                  <p className="text-xs text-yellow-600">Abertos</p>
                </div>
                <div className="bg-blue-50 rounded p-2 text-center">
                  <div className="text-lg font-semibold text-blue-700">
                    {analyst.inProgressCount}
                  </div>
                  <p className="text-xs text-blue-600">Em Progresso</p>
                </div>
                <div className="bg-green-50 rounded p-2 text-center">
                  <div className="text-lg font-semibold text-green-700">
                    {analyst.finishedCount}
                  </div>
                  <p className="text-xs text-green-600">Finalizados</p>
                </div>
              </div>
            </div>

            {/* Lista de tickets */}
            <div className="space-y-2">
              <p className="text-sm font-medium text-gray-700">Tickets Recentes</p>
              {analyst.tickets.length === 0 ? (
                <p className="text-xs text-gray-500 italic">Sem tickets</p>
              ) : (
                <div className="space-y-2 max-h-48 overflow-y-auto">
                  {analyst.tickets.slice(0, 5).map((ticket) => (
                    <div
                      key={ticket.id}
                      className="p-2 bg-gray-50 rounded border border-gray-200 hover:border-primary-300 transition-colors"
                    >
                      <div className="flex justify-between items-start gap-2">
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-900 truncate">
                            {ticket.clientName}
                          </p>
                          <p className="text-xs text-gray-600 line-clamp-1">
                            {ticket.status}
                          </p>
                        </div>
                        <div className="flex gap-1">
                          <span className={`px-2 py-1 rounded text-xs font-medium whitespace-nowrap ${getPriorityColor(ticket.priority)}`}>
                            {getPriorityLabel(ticket.priority)}
                          </span>
                          <span className={`px-2 py-1 rounded text-xs font-medium whitespace-nowrap ${getStatusColor(ticket.status)}`}>
                            {getStatusLabel(ticket.status)}
                          </span>
                        </div>
                      </div>
                    </div>
                  ))}
                  {analyst.tickets.length > 5 && (
                    <p className="text-xs text-center text-gray-500 pt-2">
                      ... e mais {analyst.tickets.length - 5}
                    </p>
                  )}
                </div>
              )}
            </div>
          </div>
        </Card>
      ))}
    </div>
  );
};

export default AnalystTicketsCard;
