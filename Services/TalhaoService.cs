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

        public TalhaoService(TalhaoRepository talhaoRepository, IUnitOfWork unitOfWork, IMapper mapper,
            FazendaRepository fazendaRepository, ClienteRepository clienteRepository) : base(unitOfWork, mapper)
        {
            _talhaoRepository = talhaoRepository;
            _fazendaRepository = fazendaRepository;
            _clienteRepository = clienteRepository;
        }

        // Buscar Talhão por ID
        public TalhaoResponseDTO? BuscarTalhaoPorId(Guid userId, Guid id)
        {
            var talhao = _talhaoRepository.BuscarTalhaoId(userId, id);
            if (talhao != null)
            {
                var response = _mapper.Map<TalhaoResponseDTO>(talhao);
                var fazenda = _fazendaRepository.ObterPorId(talhao.FazendaID);
                if (fazenda != null)
                {
                    response.Fazenda = _mapper.Map<FazendaResponseDTO>(fazenda);
                }

                var cliente = _clienteRepository.ObterPorId(talhao.ClienteID);
                if (cliente != null)
                {
                    response.Cliente = _mapper.Map<ClienteResponseDTO>(cliente);
                }

                var json = _talhaoRepository.BuscarTalhaoJson(id);
                if (json != null)
                {
                    response.Talhoes = _mapper.Map<List<Talhoes>>(json);
                }

                return response;
            }

            return null;
        }

        // Salvar novos talhões
        public TalhaoResponseDTO SalvarTalhoes(Guid userId, TalhaoRequestDTO talhaoRequestDTO)
        {
            var talhao = _mapper.Map<Talhao>(talhaoRequestDTO);
            talhao.UsuarioID = userId;
            _talhaoRepository.Adicionar(talhao);
            var talhaoCoordenadas = _mapper.Map<List<TalhaoJson>>(talhaoRequestDTO.Talhoes);
            foreach (var coordenada in talhaoCoordenadas)
            {
                coordenada.TalhaoID = talhao.Id;
                _talhaoRepository.AdicionarCoordenadas(coordenada);
            }

            UnitOfWork.Commit();
            var response = _mapper.Map<TalhaoResponseDTO>(talhao);
            response.Talhoes = _mapper.Map<List<Talhoes>>(talhaoRequestDTO.Talhoes);
            return response;
        }

        public TalhaoRequestDTO? AtualizarTalhao(Guid userId, TalhaoRequestDTO talhaoRequestDTO)
        {
            var talhao = _mapper.Map<Talhao>(talhaoRequestDTO);
            var buscarTalhao = _talhaoRepository.BuscarTalhaoId(userId, talhao.Id);
            if (buscarTalhao != null)
            {
                buscarTalhao.FazendaID = talhao.FazendaID;
                buscarTalhao.ClienteID = talhao.ClienteID;
                buscarTalhao.UsuarioID = userId;
                _talhaoRepository.Atualizar(buscarTalhao);
                var talhaoCoordenadas = _mapper.Map<List<TalhaoJson>>(talhaoRequestDTO.Talhoes);
                var deletarTalhao = _talhaoRepository.DeletarTalhaoPorId(buscarTalhao.Id);
                _talhaoRepository.Deletar(deletarTalhao);
                foreach (var coordenada in talhaoCoordenadas)
                {
                    coordenada.TalhaoID = talhao.Id;
                    _talhaoRepository.AdicionarCoordenadas(coordenada);
                }

                UnitOfWork.Commit();
                return talhaoRequestDTO;
            }

            return null;
        }

        public void DeletarTalhao(Guid id)
        {
            var talhao = _talhaoRepository.ObterPorId(id);
            if (talhao != null)
            {
                _talhaoRepository.Deletar(talhao);
            }
        }

        public PagedResult<TalhaoResponseDTO> ListarTalhao(Guid userId, QueryTalhao query)
        {
            var talhao = _talhaoRepository.ListarTalhao(userId, query);
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

                var json = _talhaoRepository.BuscarTalhaoJson((Guid)dto.Id);
                if (json != null)
                {
                    dto.Talhoes = _mapper.Map<List<Talhoes>>(json);
                }
            }

            return new PagedResult<TalhaoResponseDTO>
            {
                Items = talhaoDtos,
                TotalPages = talhao.TotalPages,
                CurrentPage = talhao.CurrentPage
            };
        }

        public bool DeletarTalhao(Guid userId, Guid id)
        {
            var talhao = _talhaoRepository.BuscarTalhaoId(userId, id);
            if (talhao != null)
            {
                _talhaoRepository.Deletar(talhao);
                UnitOfWork.Commit();
                return true;
            }

            return false;
        }

        public TalhaoResponseDTO? BuscarTalhaoPorTalhao(Guid userId, Guid id)
        {
            var talhao = _talhaoRepository.BuscarPorTalhao(id);
            if (talhao != null)
            {
                var b = _talhaoRepository.BuscarTalhaoId(userId, talhao.TalhaoID);
                if (b != null)
                {
                    var map = _mapper.Map<TalhaoResponseDTO>(b);
                    map.Talhoes = new List<Talhoes> { _mapper.Map<Talhoes>(talhao) };
                    var cliente = _clienteRepository.ObterPorId(b.ClienteID);
                    if (cliente != null)
                    {
                        map.Cliente = _mapper.Map<ClienteResponseDTO>(cliente);
                    }

                    var fazenda = _fazendaRepository.ObterPorId(b.FazendaID);
                    if (fazenda != null)
                    {
                        map.Fazenda = _mapper.Map<FazendaResponseDTO>(fazenda);
                    }

                    return map;
                }
            }

            return null;
        }

        public TalhaoResponseDTO? BuscarTalhaoPorFazendaID(Guid userID, Guid id)
        {
            Talhao? talhao = _talhaoRepository.BuscarTalhaoPorFazendaID(userID, id);
            if (talhao != null)
            {
                var map = _mapper.Map<TalhaoResponseDTO>(talhao);
                var fazenda = _fazendaRepository.ObterPorId(talhao.FazendaID);
                if (fazenda != null)
                {
                    map.Fazenda = _mapper.Map<FazendaResponseDTO>(fazenda);
                }

                var cliente = _clienteRepository.ObterPorId(talhao.ClienteID);
                if (cliente != null)
                {
                    map.Cliente = _mapper.Map<ClienteResponseDTO>(cliente);
                }

                var json = _talhaoRepository.BuscarTalhaoJson((Guid)talhao.Id);
                if (json != null)
                {
                    map.Talhoes = _mapper.Map<List<Talhoes>>(json);
                }

                return map;
            }

            return null;
        }

        public TalhaoJson? BuscarTalhaoJsonPorId(Guid id)
        {
            return _talhaoRepository.BuscarTalhaoJsonPorId(id);
        }

        public Talhao? BuscarTalhaoPorTalhaoJson(Guid talhaoJsonId)
        {
            var talhaoJson = _talhaoRepository.BuscarTalhaoJsonPorId(talhaoJsonId);
            if (talhaoJson != null)
            {
                var talhao = _talhaoRepository.BuscarTalhaoComRelacionamentos(talhaoJson.TalhaoID);
                return talhao;
            }
            return null;
        }
    }
}