using AutoMapper;
using ServiceTicket.Core.Application.DTOs;
using ServiceTicket.Core.Domain.Entities;

namespace ServiceTicket.API.Mappings;

public class TicketMappingProfile : Profile
{
    public TicketMappingProfile()
    {
        // Domain → DTO (para respostas da API)
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId))
            .ForMember(dest => dest.AssignedToUserId, opt => opt.MapFrom(src => src.AssignedToUserId));
    }
}