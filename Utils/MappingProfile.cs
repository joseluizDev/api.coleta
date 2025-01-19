using AutoMapper;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapear Usuario para UsuarioResponseDTO
        CreateMap<Usuario, UsuarioResponseDTO>();

        // Mapear UsuarioResquestDTO para Usuario
        CreateMap<UsuarioResquestDTO, Usuario>();
    }
}
