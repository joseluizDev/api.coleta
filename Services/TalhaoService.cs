using api.coleta.Models.Entidades;
using api.talhao.Models.DTOs;
using api.talhao.Repositories;
using AutoMapper;
using System;

namespace api.talhao.Services
{
   public class TalhaoService
   {
      private readonly TalhaoRepository _talhaoRepository;
      private readonly IMapper _mapper;

      public TalhaoService(TalhaoRepository talhaoRepository, IMapper mapper)
      {
         _talhaoRepository = talhaoRepository;
         _mapper = mapper;
      }

      // Buscar Talhão por ID
      public TalhaoResponseDTO? BuscarTalhaoPorId(Guid id)
      {
         var talhao = _talhaoRepository.ObterPorId(id);
         if (talhao == null)
            return null;

         return _mapper.Map<TalhaoResponseDTO>(talhao);
      }

      // Salvar novos talhões
      public void SalvarTalhoes(TalhaoRequestDTO talhaoRequestDTO)
      {
         var talhao = _mapper.Map<Talhao>(talhaoRequestDTO);
         _talhaoRepository.Adicionar(talhao);
      }

      // Atualizar talhão existente
      public void AtualizarTalhao(TalhaoRequestDTO talhaoRequestDTO)
      {
         var talhao = _mapper.Map<Talhao>(talhaoRequestDTO);
         _talhaoRepository.Atualizar(talhao);
      }

      // Deletar talhão
      public void DeletarTalhao(Guid id)
      {
         var talhao = _talhaoRepository.ObterPorId(id);
         if (talhao != null)
         {
            _talhaoRepository.Deletar(talhao);
         }
      }
   }
}
