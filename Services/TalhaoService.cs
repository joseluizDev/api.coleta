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
        public TalhaoResponseDTO? BuscarTalhaoPorId(Guid userId, Guid id)
        {
            var talhao = _talhaoRepository.BuscarTalhaoId(userId, id);
            if(talhao != null) {
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
            response.Talhoes = _mapper.Map<List<Talhoes>>(talhaoRequestDTO.Talhoes); // Fixing the type conversion issue
            return response; // Adding the missing semicolon
        }

        // public void AtualizarTalhao(TalhaoRequestDTO talhaoRequestDTO)
        // {
        //     var talhao = _mapper.Map<Talhao>(talhaoRequestDTO);
        //     _talhaoRepository.Atualizar(talhao);
        // }

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
    }
}
