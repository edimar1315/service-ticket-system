using System.ComponentModel.DataAnnotations;

namespace ServiceTicket.API.Models;

public class CreateTicketRequest
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O nome do cliente deve ter entre 3 e 200 caracteres.")]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição do problema é obrigatória.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 1000 caracteres.")]
    public string ProblemDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "A prioridade é obrigatória.")]
    [Range(1, 4, ErrorMessage = "Prioridade inválida.")]
    public int Priority { get; set; }
}