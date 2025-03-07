using AutoMapper;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.cliente.Models.DTOs;
using api.funcionario.Models.DTOs;
using api.fazenda.Models.Entidades;
using api.safra.Models.DTOs;
using api.talhao.Models.DTOs;
using api.vinculoClienteFazenda.Models.DTOs;
using api.fazenda.models;
using System.Text.Json;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeamentos para Usuario
        CreateMap<Usuario, UsuarioResquestDTO>();
        CreateMap<Usuario, UsuarioResponseDTO>();
        CreateMap<UsuarioResquestDTO, Usuario>();


        CreateMap<Cliente, ClienteResponseDTO>();
        CreateMap<ClienteRequestDTO, Cliente>();

        CreateMap<Funcionario, FuncionarioResponseDTO>();
        CreateMap<FuncionarioRequestDTO, Funcionario>();

        CreateMap<Fazenda, FazendaResponseDTO>();
        CreateMap<FazendaRequestDTO, Fazenda>();

        CreateMap<Safra, SafraResponseDTO>();
        CreateMap<SafraRequestDTO, Safra>();

        CreateMap<Talhao, TalhaoResponseDTO>();
        CreateMap<TalhaoRequestDTO, Talhao>();

        // Mapeamento de Talhoes para TalhaoJson
        CreateMap<Talhoes, TalhaoJson>()
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area.ToString()))
            .ForMember(dest => dest.Coordenadas, opt => opt.MapFrom(src => JsonSerializer.Serialize(src.Coordenadas, new JsonSerializerOptions())));

        CreateMap<TalhaoJson, Talhoes>()
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => double.Parse(src.Area)))
            .ForMember(dest => dest.Coordenadas, opt => opt.MapFrom(src => JsonSerializer.Deserialize<List<Coordenada>>(src.Coordenadas, new JsonSerializerOptions())));

        CreateMap<VinculoClienteFazenda, VinculoResponseDTO>();
        CreateMap<VinculoRequestDTO, VinculoClienteFazenda>();
    }
}
