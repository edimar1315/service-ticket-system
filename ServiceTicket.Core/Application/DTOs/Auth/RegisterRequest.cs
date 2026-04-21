using System.ComponentModel.DataAnnotations;

namespace ServiceTicket.Core.Application.DTOs.Auth;

public record RegisterRequest(
    [Required(ErrorMessage = "O nome completo é obrigatório")]
    [StringLength(200, MinimumLength = 3)]
    string FullName,
    
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    string Email,
    
    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
    string Password
);