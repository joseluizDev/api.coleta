using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Utils;
using api.coleta.Utils.Maps;

namespace api.cliente.Services
{
   public class ClienteService : ServiceBase
   {
      private readonly ClienteRepository _clienteRepository;

      public ClienteService(ClienteRepository clienteRepository, IUnitOfWork unitOfWork)
          : base(unitOfWork)
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

         return clientesBd.ToResponseDtoList();

      }

      public PagedResult<ClienteResponseDTO> TotalClientes(Guid id, QueryClienteDTO query)
      {

         var clientes = _clienteRepository.ListarClientes(id, query);
         var clienteDtos = clientes.Items.ToResponseDtoList();
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
         return cliente.ToResponseDto();
      }

      public ClienteResponseDTO SalvarCliente(ClienteRequestDTO clienteDto, Guid idUser)
      {
         var clienteEntidade = clienteDto.ToEntity();
         if (clienteEntidade == null)
         {
            throw new InvalidOperationException("Não foi possível converter os dados do cliente.");
         }

         clienteEntidade.UsuarioID = idUser;
         _clienteRepository.Adicionar(clienteEntidade);
         UnitOfWork.Commit();
         return clienteEntidade.ToResponseDto()!;
      }

      public ClienteResponseDTO? AtualizarCliente(Guid userId, ClienteRequestDTO clienteDto)
      {
         if (clienteDto.Id is null)
         {
            return null;
         }

         var cliente = _clienteRepository.BuscarClienteId(userId, clienteDto.Id.Value);
         if (cliente == null)
         {
            return null;
         }

         var clienteEntidade = clienteDto.ToEntity();
         if (clienteEntidade == null)
         {
            throw new InvalidOperationException("Não foi possível converter os dados do cliente.");
         }

         cliente.Nome = clienteEntidade.Nome;
         cliente.Email = clienteEntidade.Email;
         cliente.Telefone = clienteEntidade.Telefone;
         cliente.Cep = clienteEntidade.Cep;
         cliente.Endereco = clienteEntidade.Endereco;
         cliente.Cidade = clienteEntidade.Cidade;
         cliente.Estado = clienteEntidade.Estado;
         _clienteRepository.Atualizar(cliente);
         UnitOfWork.Commit();

         return cliente.ToResponseDto();
      }

      public ClienteResponseDTO? DeletarCliente(Guid userId, Guid id)
      {
         var cliente = _clienteRepository.BuscarClienteId(userId, id);
         if (cliente != null)
         {
            _clienteRepository.Deletar(cliente);
            UnitOfWork.Commit();
            return cliente.ToResponseDto();
         }
         return null;
      }

      public List<ClienteResponseDTO> ListarTodosClientes(Guid userId)
      {
         var clientes = _clienteRepository.ListarTodosClientes(userId);
         return clientes.ToResponseDtoList();
      }
   }
}
