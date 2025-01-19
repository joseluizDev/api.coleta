
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using AutoMapper;
using whtsapp.Data.Repository;
public class UsuarioService : ServiceBase
{
    private readonly UsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;

    public UsuarioService(UsuarioRepository usuarioRepository, IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public UsuarioResponseDTO? BuscarUsuarioPorId(Guid id)
    {
        var usuario = _usuarioRepository.ObterPorId(id);

        if (usuario == null)
        {
            return null;
        }

        // Mapeando a entidade para o DTO
        return _mapper.Map<UsuarioResponseDTO>(usuario);
    }

    public UsuarioResponseDTO? Login(string login, string senha)
    {
        var usuario = _usuarioRepository.Login(login, senha);
        if (usuario == null)
        {
            return null;
        }

        // Mapeando a entidade para o DTO
        return _mapper.Map<UsuarioResponseDTO>(usuario);
    }

    public bool Cadastrar(UsuarioResquestDTO usuario)
    {
        var usuarioExistente = _usuarioRepository.ObterPorEmail(usuario.Email);
        if (usuarioExistente != null)
        {
            throw new Exception("Email já cadastrado");
        }

        var cpfExistente = _usuarioRepository.ObterPorCpf(usuario.CPF);
        if (cpfExistente != null)
        {
            throw new Exception("CPF já cadastrado");
        }

        // Mapeando o DTO para a entidade
        var usuarioEntidade = _mapper.Map<Usuario>(usuario);
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

        // Atualizando a entidade com os dados do DTO
        var usuarioEntidade = _mapper.Map(usuario, usuarioExistente);
        _usuarioRepository.Atualizar(usuarioEntidade);
        UnitOfWork.Commit();
        return true;
    }
}
