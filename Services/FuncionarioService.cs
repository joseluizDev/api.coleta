using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.funcionario.Models.DTOs;
using api.funcionario.Repositories;
using AutoMapper;

namespace api.funcionario.Services
{
   public class FuncionarioService : ServiceBase
   {
      private readonly FuncionarioRepository _funcionarioRepository;

      public FuncionarioService(FuncionarioRepository funcionarioRepository, IUnitOfWork unitOfWork, IMapper mapper)
          : base(unitOfWork, mapper)
      {
         _funcionarioRepository = funcionarioRepository;
      }

      public FuncionarioResponseDTO? BuscarFuncionarioPorId(Guid id)
      {
         var funcionario = _funcionarioRepository.ObterPorId(id);
         if (funcionario == null)
         {
            return null;
         }
         return _mapper.Map<FuncionarioResponseDTO>(funcionario);
      }

      public void SalvarFuncionario(FuncionarioRequestDTO funcionarioDto)
      {
         var funcionarioEntidade = _mapper.Map<Funcionario>(funcionarioDto);
         _funcionarioRepository.Adicionar(funcionarioEntidade);
         UnitOfWork.Commit();
      }

      public void AtualizarFuncionario(FuncionarioRequestDTO funcionarioDto)
      {
         var funcionarioEntidade = _mapper.Map<Funcionario>(funcionarioDto);
         _funcionarioRepository.Atualizar(funcionarioEntidade);
         UnitOfWork.Commit();
      }

      public void DeletarFuncionario(Guid id)
      {
         var funcionario = _funcionarioRepository.ObterPorId(id);
         _funcionarioRepository.Deletar(funcionario);
         UnitOfWork.Commit();
      }
   }
}
