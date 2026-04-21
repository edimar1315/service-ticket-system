using System.ComponentModel.DataAnnotations;

namespace ServiceTicket.API.Models;

public class UpdateTicketStatusRequest 
{
    [Required(ErrorMessage = "O campo Status é obrigatório.")]
    [Range(1, 4, ErrorMessage = "Status inválido.")]
    public int Status { get; set; }
}