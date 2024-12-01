
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
        return AutoMapper.Map<Usuario, UsuarioResponseDTO>(usuario);
    }

    public UsuarioResponseDTO? Login(string login, string senha)
    {
        var usuario = _usuarioRepository.Login(login, senha);
        if (usuario == null)
        {
            return null;
        }
        return AutoMapper.Map<Usuario, UsuarioResponseDTO>(usuario);
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