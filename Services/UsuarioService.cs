
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Data.Repository;
using AutoMapper;
public class UsuarioService : ServiceBase
{
    private readonly UsuarioRepository _usuarioRepository;
    public UsuarioService(UsuarioRepository usuarioRepository, IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper)
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

        return _mapper.Map<UsuarioResponseDTO>(usuario);
    }

    public UsuarioResponseDTO? Login(string login, string senha)
    {
        var usuario = _usuarioRepository.Login(login, senha);
        if (usuario == null)
        {
            return null;
        }

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

        var usuarioEntidade = _mapper.Map<Usuario>(usuario);
        _usuarioRepository.Adicionar(usuarioEntidade);
        UnitOfWork.Commit();
        return true;
    }

}
