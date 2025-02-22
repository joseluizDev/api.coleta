using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Utils;
using AutoMapper;

namespace api.cliente.Services
{
   public class ClienteService : ServiceBase
   {
      private readonly ClienteRepository _clienteRepository;

      public ClienteService(ClienteRepository clienteRepository, IUnitOfWork unitOfWork, IMapper mapper)
          : base(unitOfWork, mapper)
      {
         _clienteRepository = clienteRepository;
      }

      public List<ClienteResponseDTO> BuscarClientes(
          Guid id,
          int page = -1,
          int limit = -1,
          string searchTerm = ""
        )
      {

         var clientesBd = _clienteRepository.BuscarClientes(id, page);

         return _mapper.Map<List<ClienteResponseDTO>>(clientesBd);

      }

      public PagedResult<ClienteResponseDTO> TotalClientes(Guid id, int page)
      {

         var clientes = _clienteRepository.listarClientes(id, page);
         var clienteDtos = _mapper.Map<List<ClienteResponseDTO>>(clientes.Items);
         return new PagedResult<ClienteResponseDTO>
         {
            Items = clienteDtos,
            TotalPages = clientes.TotalPages,
            CurrentPage = clientes.CurrentPage
         };
      }

      public ClienteResponseDTO? BuscarClientePorId(Guid userId, Guid id)
      {
         var cliente = _clienteRepository.BuscarClienteId(userId, id);
         if (cliente == null)
         {
            return null;
         }
         return _mapper.Map<ClienteResponseDTO>(cliente);
      }

      public ClienteResponseDTO SalvarCliente(ClienteRequestDTO clienteDto, Guid idUser)
      {
         var clienteEntidade = _mapper.Map<Cliente>(clienteDto);
         clienteEntidade.UsuarioID = idUser;
         _clienteRepository.Adicionar(clienteEntidade);
         UnitOfWork.Commit();
         return _mapper.Map<ClienteResponseDTO>(clienteEntidade);
      }

      public ClienteResponseDTO? AtualizarCliente(Guid userId, ClienteRequestDTO clienteDto)
      {
         var clienteEntidade = _mapper.Map<Cliente>(clienteDto);
         var cliente = _clienteRepository.BuscarClienteId(userId, clienteEntidade.Id);
         if (cliente != null)
         {
            cliente.Nome = clienteEntidade.Nome;
            cliente.CPF = clienteEntidade.CPF;
            cliente.Email = clienteEntidade.Email;
            cliente.Telefone = clienteEntidade.Telefone;
            cliente.Cep = clienteEntidade.Cep;
            cliente.Endereco = clienteEntidade.Endereco;
            cliente.Cidade = clienteEntidade.Cidade;
            cliente.Estado = clienteEntidade.Estado;
            _clienteRepository.Atualizar(cliente);
            UnitOfWork.Commit();

            return _mapper.Map<ClienteResponseDTO>(cliente);
         }
         return null;
      }

      public ClienteResponseDTO? DeletarCliente(Guid userId, Guid id)
      {
         var cliente = _clienteRepository.BuscarClienteId(userId, id);
         if (cliente != null)
         {
            _clienteRepository.Deletar(cliente);
            UnitOfWork.Commit();
            return _mapper.Map<ClienteResponseDTO>(cliente);
         }
         return null;
      }

      public List<ClienteResponseDTO> ListarTodosClientes(Guid userId)
      {
         var clientes = _clienteRepository.ListarTodosClientes(userId);
         return _mapper.Map<List<ClienteResponseDTO>>(clientes);
      }
   }
}
