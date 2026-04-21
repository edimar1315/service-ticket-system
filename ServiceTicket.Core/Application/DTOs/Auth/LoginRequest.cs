using System.ComponentModel.DataAnnotations;

namespace ServiceTicket.Core.Application.DTOs.Auth;

public record LoginRequest(
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    string Email,
    
    [Required(ErrorMessage = "A senha é obrigatória")]
    string Password
);