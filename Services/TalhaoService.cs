using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Utils;
using api.fazenda.models;
using api.fazenda.repositories;
using api.safra.Models.DTOs;
using api.talhao.Models.DTOs;
using api.talhao.Repositories;
using AutoMapper;
using System;

namespace api.talhao.Services
{
    public class TalhaoService : ServiceBase
    {
        private readonly TalhaoRepository _talhaoRepository;
        private readonly FazendaRepository _fazendaRepository;
        private readonly ClienteRepository _clienteRepository;

        public TalhaoService(TalhaoRepository talhaoRepository, IUnitOfWork unitOfWork, IMapper mapper, FazendaRepository fazendaRepository, ClienteRepository clienteRepository) 
            : base(unitOfWork, mapper)
        {
            _talhaoRepository = talhaoRepository;
            _fazendaRepository = fazendaRepository;
            _clienteRepository = clienteRepository;
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
        public TalhaoResponseDTO SalvarTalhoes(Guid userId, TalhaoRequestDTO talhaoRequestDTO)
        {

            var talhao = _mapper.Map<Talhao>(talhaoRequestDTO);
            talhao.UsuarioID = userId;
            _talhaoRepository.Adicionar(talhao);
            UnitOfWork.Commit();
            return _mapper.Map<TalhaoResponseDTO>(talhao);
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

        public PagedResult<TalhaoResponseDTO> ListarTalhao(Guid userId, int page)
        {
            var talhao = _talhaoRepository.ListarTalhao(userId, page);
            var talhaoDtos = _mapper.Map<List<TalhaoResponseDTO>>(talhao.Items);

            foreach (var dto in talhaoDtos)
            {
                var fazenda = _fazendaRepository.ObterPorId(dto.FazendaID);
                if (fazenda != null)
                {
                    dto.Fazenda = _mapper.Map<FazendaResponseDTO>(fazenda);

                }
                var cliente = _clienteRepository.ObterPorId(dto.ClienteID);
                if (cliente != null)
                {
                    dto.Cliente = _mapper.Map<ClienteResponseDTO>(cliente);
                }
            }

            return new PagedResult<TalhaoResponseDTO>
            {
                Items = talhaoDtos,
                TotalPages = talhao.TotalPages,
                CurrentPage = talhao.CurrentPage
            };

        }
    }
}
