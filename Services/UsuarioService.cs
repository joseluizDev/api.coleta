
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Data.Repository;
using AutoMapper;
using api.cliente.Interfaces;
public class UsuarioService : ServiceBase
{
    private readonly UsuarioRepository _usuarioRepository;
    private readonly IJwtToken _jwtToken;

    public UsuarioService(UsuarioRepository usuarioRepository, IUnitOfWork unitOfWork, IMapper mapper, IJwtToken jwtToken)
        : base(unitOfWork, mapper)
    {
        _usuarioRepository = usuarioRepository;
        _jwtToken = jwtToken;
    }

    public UsuarioResquestDTO? BuscarUsuarioPorId(Guid id)
    {
        var usuario = _usuarioRepository.ObterPorId(id);
        if (usuario == null)
        {
            return null;
        }
        return _mapper.Map<UsuarioResquestDTO>(usuario);
    }

    public String? Login(string email, string senha)
    {
        var usuario = _usuarioRepository.Login(email, senha);
        if (usuario == null)
        {
            return null;
        }

        return _jwtToken.GerarToken(usuario);
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

        var usuarioEntidade = _mapper.Map<Usuario>(usuario);
        _usuarioRepository.Adicionar(usuarioEntidade);
        UnitOfWork.Commit();
        return true;
    }

    public UsuarioResquestDTO? AtualizarUsuario(Guid id, UsuarioResquestDTO usuarioDto)
    {
        var usuario = _usuarioRepository.ObterPorId(id);
        if (usuario == null)
        {
            return null;
        }
        usuario.Atualizar(usuarioDto);
        _usuarioRepository.Atualizar(usuario);
        UnitOfWork.Commit();
        return _mapper.Map<UsuarioResquestDTO>(usuario);

    }
}
