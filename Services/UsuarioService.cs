using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Data.Repository;
using AutoMapper;
using api.cliente.Interfaces;
using api.coleta.Utils;
using api.fazenda.Models.Entidades;
using api.fazenda.models;
using api.funcionario.Models.DTOs;
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

    public bool Cadastrar(Guid userId, UsuarioResquestDTO usuario)
    {
        var usuarioAdmin = _usuarioRepository.ObterPorId(userId);
        if(usuarioAdmin != null)
        {
            usuario.adminId = usuarioAdmin.Id;
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
        return false;
    }

    public PagedResult<UsuarioResponseDTO> Funcionarios(QueryFuncionario query, Guid userId)
    {
        var usuarios = _usuarioRepository.ListarFuncionarios(query, userId);
        var usuariosDto = _mapper.Map<List<UsuarioResponseDTO>>(usuarios.Items);
        return new PagedResult<UsuarioResponseDTO>
        {
            Items = usuariosDto,
            TotalPages = usuarios.TotalPages,
            CurrentPage = usuarios.CurrentPage
        };
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

    public bool DeletarFuncionario(Guid id, Guid userId)
    {
        var usuario = _usuarioRepository.ObterFuncionario(id, userId);
        if (usuario == null)
        {
            return false;
        }
        _usuarioRepository.Deletar(usuario);
        UnitOfWork.Commit();
        return true;
    }

    public List<UsuarioResponseDTO?> ListarUsuarioFuncionario(Guid userId)
    {
        var usuario = _usuarioRepository.ListarUsuariosPorFuncionario(userId);
        if (usuario == null)
        {
            return null;
        }
        return _mapper.Map<List<UsuarioResponseDTO?>>(usuario);
    }

    public String? LoginMobile(UsuarioLoginDTO usuario)
    {
        var u = _usuarioRepository.LoginMobile(usuario.Email, usuario.Senha);
        if (u == null)
        {
            return null;
        }

        return _jwtToken.GerarToken(u);
    }

    public FuncionarioResponseDTO BuscarFuncionarioPorId(Guid id, Guid userId)
    {
        var usuario = _usuarioRepository.ObterFuncionario(id, userId);
        if (usuario == null)
        {
            throw new Exception("Funcionário não encontrado.");
        }
        return _mapper.Map<FuncionarioResponseDTO>(usuario);
    }

    public FuncionarioResponseDTO? AtualizarFuncionario(Guid userId, FuncionarioRequestDTO funcionarioDto)
    {
        var usuario = _usuarioRepository.ObterFuncionario(funcionarioDto.Id, userId);
        if (usuario == null)
        {
            return null;
        }
        var usuarioDto = _mapper.Map<UsuarioResquestDTO>(funcionarioDto);
        usuario.Atualizar(usuarioDto);
        _usuarioRepository.Atualizar(usuario);
        UnitOfWork.Commit();
        return _mapper.Map<FuncionarioResponseDTO>(usuario);
    }
}
