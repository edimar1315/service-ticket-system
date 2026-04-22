using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ServiceTicket.Core.Application.DTOs.Auth;
using ServiceTicket.Core.Domain.Entities;
using ServiceTicket.Core.Domain.Enums;
using ServiceTicket.Core.Interfaces.Services;

namespace ServiceTicket.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registrando novo usuário: {Email}", request.Email);

        var role = request.Role is UserRole.Customer or UserRole.Support
            ? request.Role
            : UserRole.Customer;

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Tentativa de registro com email já existente: {Email}", request.Email);
            throw new InvalidOperationException("Email já está em uso.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Falha ao criar usuário {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException($"Falha ao criar usuário: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError("Falha ao atribuir role '{Role}' ao usuário {Email}: {Errors}", role, request.Email, roleErrors);
            throw new InvalidOperationException($"Usuário criado, mas falha ao atribuir perfil '{role}': {roleErrors}");
        }

        _logger.LogInformation("Usuário {Email} criado com role {Role}", request.Email, role);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tentativa de login: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado: {Email}", request.Email);
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Senha inválida para usuário: {Email}", request.Email);
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        _logger.LogInformation("Login bem-sucedido: {Email}", request.Email);

        return await GenerateAuthResponseAsync(user);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada"));

        var expiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpirationHours"] ?? "8"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Adicionar roles (se houver)
        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? UserRole.Customer;
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new AuthResponse(
            Token: tokenString,
            Email: user.Email!,
            FullName: user.FullName,
            Role: primaryRole,
            ExpiresAt: expiresAt
        );
    }
}