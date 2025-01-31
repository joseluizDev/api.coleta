using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Services;
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

      public  List<ClienteResponseDTO> BuscarClientesPaginados(Guid id, int page=-1, int limit=-1)
        {

           var clientesBd = _clienteRepository.BuscaPaginadaCliente(id, page, limit);

            return _mapper.Map<List<ClienteResponseDTO>>(clientesBd);

        }

      public ClienteResponseDTO? BuscarClientePorId(Guid id)
      {
         var cliente = _clienteRepository.ObterPorId(id);
         if (cliente == null)
         {
            return null;
         }
         return _mapper.Map<ClienteResponseDTO>(cliente);
      }

      public void SalvarCliente(ClienteRequestDTO clienteDto, Guid idUser)
      {
          var clienteEntidade = _mapper.Map<Cliente>(clienteDto);
          clienteEntidade.UsuarioID = idUser;
         _clienteRepository.Adicionar(clienteEntidade);
         UnitOfWork.Commit();
      }

      public void AtualizarCliente(ClienteRequestDTO clienteDto)
      {
         var clienteEntidade = _mapper.Map<Cliente>(clienteDto);
         _clienteRepository.Atualizar(clienteEntidade);
         UnitOfWork.Commit();
      }

      public void DeletarCliente(Guid id)
      {
         var cliente = _clienteRepository.ObterPorId(id);
         _clienteRepository.Deletar(cliente);
         UnitOfWork.Commit();
      }
   }
}
