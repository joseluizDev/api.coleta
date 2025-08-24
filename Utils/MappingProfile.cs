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
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using api.coleta.models;

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

        CreateMap<Coleta, ColetaPorUsuarioDto>()
         .ForMember(dest => dest.TipoAnalise,
               opt => opt.MapFrom(src => src.TipoAnalise
                   .Select(t => t.ToString())
                   .ToList()));
         
        CreateMap<Fazenda, FazendaResponseDTO>();
        CreateMap<FazendaRequestDTO, Fazenda>();

        CreateMap<Safra, SafraResponseDTO>();
        CreateMap<SafraRequestDTO, Safra>();
        CreateMap<SafraResponseDTO, Safra>();


        CreateMap<ConfiguracaoPadrao, ConfiguracaoPadraoResponseDTO>();
        CreateMap<ConfiguracaoPadraoRequestDTO, ConfiguracaoPadrao>();

        CreateMap<ConfiguracaoPersonalizada, ConfiguracaoPersonalizadaResponseDTO>();
        CreateMap<ConfiguracaoPersonalizadaRequestDTO, ConfiguracaoPersonalizada>();

        CreateMap<Talhao, TalhaoResponseDTO>();
        CreateMap<TalhaoRequestDTO, Talhao>();

         CreateMap<TalhaoJson, TalhaoResponseDTO>();

        // Mapeamento de Talhoes para TalhaoJson
        CreateMap<Talhoes, TalhaoJson>()
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area.ToString()))
            .ForMember(dest => dest.Coordenadas, opt => opt.MapFrom(src => JsonSerializer.Serialize(src.Coordenadas, new JsonSerializerOptions())));

        CreateMap<TalhaoJson, Talhoes>()
            .ForMember(dest => dest.TalhaoID, opt => opt.MapFrom(src => src.TalhaoID))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => double.Parse(src.Area)))
            .ForMember(dest => dest.Coordenadas, opt => opt.MapFrom(src => JsonSerializer.Deserialize<List<Coordenada>>(src.Coordenadas, new JsonSerializerOptions())));

        CreateMap<Coordenada, Coordenada>();

        CreateMap<VisualizarMapOutputDto, object>()
            .ConvertUsing(dto => new
            {
                dto.Id,
                dto.Talhao,
                Geojson = JsonSerializer.Serialize(dto.Geojson, new JsonSerializerOptions()), // Serializa corretamente para JSON
                dto.TalhaoID,
                dto.SafraID,
                dto.UsuarioRespID,
                dto.Observacao,
                dto.TipoColeta,
                dto.TipoAnalise,
                dto.Profundidade,
                dto.NomeColeta
            });

        CreateMap<Coleta, VisualizarMapOutputDto>()
            .ForMember(dest => dest.UsuarioRespID, opt => opt.MapFrom(src => new UsuarioResponseDTO
            {
                Id = src.UsuarioRespID
            }))
            .ForMember(dest => dest.Talhao, opt => opt.MapFrom(src => src.Talhao)) // Supondo que seja direto
            .ForMember(dest => dest.TalhaoID, opt => opt.MapFrom(src => src.TalhaoID))
            .ForMember(dest => dest.SafraID, opt => opt.MapFrom(src => src.SafraID))
            .ForMember(dest => dest.Geojson, opt => opt.MapFrom(src => src.Geojson))
            .ForMember(dest => dest.GeoJsonID, opt => opt.MapFrom(src => src.GeojsonID))
            .ForMember(dest => dest.UsuarioRespID, opt => opt.MapFrom(src => src.UsuarioRespID)) // ou outro campo correto
            .ForMember(dest => dest.Observacao, opt => opt.MapFrom(src => src.Observacao))
            .ForMember(dest => dest.TipoColeta, opt => opt.MapFrom(src => src.TipoColeta))
            .ForMember(dest => dest.TipoAnalise, opt => opt.MapFrom(src => src.TipoAnalise.Select(t => t.ToString()).ToList()))
            .ForMember(dest => dest.Profundidade, opt => opt.MapFrom(src => src.Profundidade))
            .ForMember(dest => dest.NomeColeta, opt => opt.MapFrom(src => src.NomeColeta));

        CreateMap<UsuarioResquestDTO, UsuarioResponseDTO>();
        CreateMap<UsuarioResponseDTO, UsuarioResquestDTO>();


        CreateMap<VinculoClienteFazenda, VinculoResponseDTO>();
        CreateMap<VinculoRequestDTO, VinculoClienteFazenda>();

        CreateMap<Usuario, FuncionarioResponseDTO>()
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.NomeCompleto))
            .ForMember(dest => dest.Observacao, opt => opt.MapFrom(src => src.Observacao))
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo))
            .ForMember(dest => dest.CPF, opt => opt.MapFrom(src => src.CPF))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<FuncionarioRequestDTO, UsuarioResquestDTO>()
            .ForMember(dest => dest.NomeCompleto, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.CPF, opt => opt.MapFrom(src => src.CPF))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
            .ForMember(dest => dest.Senha, opt => opt.MapFrom(src => src.Senha))
            .ForMember(dest => dest.adminId, opt => opt.Ignore());
    }
}
