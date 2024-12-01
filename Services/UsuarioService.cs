
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using whtsapp.Data.Repository;

public class UsuarioService : ServiceBase
{
    private readonly UsuarioRepository _usuarioRepository;
    public UsuarioService(UsuarioRepository usuarioRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
    }

    public UsuarioResponseDTO? BuscarUsuarioPorId(Guid id)
    {
        var usuario = _usuarioRepository.ObterPorId(id);

        if (usuario == null)
        {
            return null;
        }
        return new UsuarioResponseDTO
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            CPF = usuario.CPF,
            Telefone = usuario.Telefone
        };
    }

    public UsuarioResponseDTO? Login(string login, string senha)
    {
        var usuario = _usuarioRepository.Login(login, senha);
        if (usuario == null)
        {
            return null;
        }
        return new UsuarioResponseDTO
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            CPF = usuario.CPF,
            Telefone = usuario.Telefone
        };
    }

    public bool Cadastrar(UsuarioResquestDTO usuario)
    {
        var usuarioExistente = _usuarioRepository.ObterPorEmail(usuario.Email);
        if (usuarioExistente != null)
        {
            return false;
        }

        var cpfExistente = _usuarioRepository.ObterPorCpf(usuario.CPF);
        if (cpfExistente != null)
        {
            return false;
        }

        var usuarioEntidade = new Usuario(usuario);
        _usuarioRepository.Adicionar(usuarioEntidade);
        UnitOfWork.Commit();
        return true;
    }

    public bool Atualizar(UsuarioResquestDTO usuario)
    {
        var usuarioExistente = _usuarioRepository.ObterPorId(usuario.Id);
        if (usuarioExistente == null)
        {
            return false;
        }

        if (usuarioExistente.Id != usuario.Id)
        {
            var cpfExistente = _usuarioRepository.ObterPorCpf(usuario.CPF);

            if (cpfExistente == null)
            {
                return false;
            }
        }


        var usuarioEntidade = new Usuario(usuario);
        _usuarioRepository.Atualizar(usuarioEntidade);
        UnitOfWork.Commit();
        return true;
    }



}
public class DynamicMapper
{
    // Método para mapear uma lista de entidades para uma lista de DTOs de forma dinâmica
    public static List<TDto> MapToDtoList<TEntity, TDto>(List<TEntity> entities)
        where TEntity : class
        where TDto : class, new()
    {
        var dtoList = new List<TDto>();

        foreach (var entity in entities)
        {
            var dto = MapToDto<TEntity, TDto>(entity);
            dtoList.Add(dto);
        }

        return dtoList;
    }

    public static TDto MapToDto<TEntity, TDto>(TEntity entity)
        where TEntity : class
        where TDto : class, new()
    {
        if (entity == null) return null;

        var dto = new TDto();

        var entityProperties = entity.GetType().GetProperties();
        var dtoProperties = dto.GetType().GetProperties();

        foreach (var entityProperty in entityProperties)
        {
            var dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == entityProperty.Name);
            if (dtoProperty != null)
            {
                dtoProperty.SetValue(dto, entityProperty.GetValue(entity));
            }
        }

        return dto;
    }
}