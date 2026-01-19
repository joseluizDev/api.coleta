using System;
using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using api.fazenda.models;
using api.fazenda.repositories;
using api.safra.Models.DTOs;
using api.talhao.Models.DTOs;
using api.talhao.Repositories;
using System;

namespace api.talhao.Services
{
    public class TalhaoService : ServiceBase
    {
        private readonly TalhaoRepository _talhaoRepository;
        private readonly FazendaRepository _fazendaRepository;
        private readonly ClienteRepository _clienteRepository;

        public TalhaoService(TalhaoRepository talhaoRepository, IUnitOfWork unitOfWork,
            FazendaRepository fazendaRepository, ClienteRepository clienteRepository) : base(unitOfWork)
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
                var response = talhao.ToResponseDto();
                if (response == null)
                {
                    return null;
                }

                var fazenda = _fazendaRepository.ObterPorId(talhao.FazendaID);
                if (fazenda != null)
                {
                    response.Fazenda = fazenda.ToResponseDto()!;
                }

                var cliente = _clienteRepository.ObterPorId(talhao.ClienteID);
                if (cliente != null)
                {
                    response.Cliente = cliente.ToResponseDto()!;
                }

                var json = _talhaoRepository.BuscarTalhaoJson(id);
                if (json != null)
                {
                    response.Talhoes = json.ToTalhoesList();
                }

                return response;
            }

            return null;
        }

        // Salvar novos talhões
        public TalhaoResponseDTO SalvarTalhoes(Guid userId, TalhaoRequestDTO talhaoRequestDTO)
        {
            var talhao = talhaoRequestDTO.ToEntity();
            if (talhao == null)
            {
                throw new InvalidOperationException("Não foi possível converter os dados do talhão.");
            }

            talhao.UsuarioID = userId;
            _talhaoRepository.Adicionar(talhao);
            var talhaoCoordenadas = talhaoRequestDTO.Talhoes.ToTalhaoJsonList();
            foreach (var coordenada in talhaoCoordenadas)
            {
                coordenada.TalhaoID = talhao.Id;
                _talhaoRepository.AdicionarCoordenadas(coordenada);
            }

            UnitOfWork.Commit();
            var response = talhao.ToResponseDto();
            if (response == null)
            {
                throw new InvalidOperationException("Não foi possível montar a resposta do talhão.");
            }

            response.Talhoes = talhaoRequestDTO.Talhoes.CloneTalhoes();
            return response;
        }

        public TalhaoRequestDTO? AtualizarTalhao(Guid userId, TalhaoRequestDTO talhaoRequestDTO)
        {
            var talhao = talhaoRequestDTO.ToEntity();
            if (talhao == null)
            {
                throw new InvalidOperationException("Não foi possível converter os dados do talhão.");
            }
            var buscarTalhao = _talhaoRepository.BuscarTalhaoId(userId, talhao.Id);
            if (buscarTalhao != null)
            {
                buscarTalhao.FazendaID = talhao.FazendaID;
                buscarTalhao.ClienteID = talhao.ClienteID;
                buscarTalhao.UsuarioID = userId;
                _talhaoRepository.Atualizar(buscarTalhao);
                var talhaoCoordenadas = talhaoRequestDTO.Talhoes.ToTalhaoJsonList();
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

        public List<TalhaoResponseDTO> ListarTalhao(Guid userId, QueryTalhao query)
        {
            var talhoes = _talhaoRepository.ListarTalhao(userId, query);
<<<<<<< HEAD
            var talhaoDtos = talhoes.ToResponseDtoList();
=======
            var talhaoDtos = _mapper.Map<List<TalhaoResponseDTO>>(talhoes);
>>>>>>> hotfix/0.0.1
            foreach (var dto in talhaoDtos)
            {
                var fazenda = _fazendaRepository.ObterPorId(dto.FazendaID);
                if (fazenda != null)
                {
                    dto.Fazenda = fazenda.ToResponseDto()!;
                }

                var cliente = _clienteRepository.ObterPorId(dto.ClienteID);
                if (cliente != null)
                {
                    dto.Cliente = cliente.ToResponseDto()!;
                }

                var json = _talhaoRepository.BuscarTalhaoJson((Guid)dto.Id);
                if (json != null)
                {
                    dto.Talhoes = json.ToTalhoesList();
                }
            }

            return talhaoDtos;
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
                    var map = b.ToResponseDto();
                    if (map == null)
                    {
                        return null;
                    }

                    var talhaoMapped = talhao.ToTalhoes();
                    map.Talhoes = talhaoMapped != null
                        ? new List<Talhoes> { talhaoMapped }
                        : new List<Talhoes>();
                    var cliente = _clienteRepository.ObterPorId(b.ClienteID);
                    if (cliente != null)
                    {
                        map.Cliente = cliente.ToResponseDto()!;
                    }

                    var fazenda = _fazendaRepository.ObterPorId(b.FazendaID);
                    if (fazenda != null)
                    {
                        map.Fazenda = fazenda.ToResponseDto()!;
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
                var map = talhao.ToResponseDto();
                if (map == null)
                {
                    return null;
                }
                var fazenda = _fazendaRepository.ObterPorId(talhao.FazendaID);
                if (fazenda != null)
                {
                    map.Fazenda = fazenda.ToResponseDto()!;
                }

                var cliente = _clienteRepository.ObterPorId(talhao.ClienteID);
                if (cliente != null)
                {
                    map.Cliente = cliente.ToResponseDto()!;
                }

                var json = _talhaoRepository.BuscarTalhaoJson((Guid)talhao.Id);
                if (json != null)
                {
                    map.Talhoes = json.ToTalhoesList();
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

        // Deletar TalhaoJson por ID
        public bool DeletarTalhaoJson(Guid userId, Guid talhaoJsonId)
        {
            var talhaoJson = _talhaoRepository.BuscarTalhaoJsonPorId(talhaoJsonId);
            
            if (talhaoJson != null)
            {
                // Verificar se o usuário tem permissão (verificando se o talhão pertence ao usuário)
                var talhao = _talhaoRepository.BuscarTalhaoId(userId, talhaoJson.TalhaoID);
                
                if (talhao != null)
                {
                    _talhaoRepository.DeletarTalhaoJson(talhaoJson);
                    UnitOfWork.Commit();
                    return true;
                }
            }
            
            return false;
        }

        // Atualizar nome e observação do TalhaoJson
        public bool AtualizarNomeTalhaoJson(Guid userId, Guid talhaoJsonId, AtualizarNomeTalhaoJsonDTO dto)
        {
            var talhaoJson = _talhaoRepository.BuscarTalhaoJsonPorId(talhaoJsonId);
            
            if (talhaoJson != null)
            {
                // Verificar se o usuário tem permissão (verificando se o talhão pertence ao usuário)
                var talhao = _talhaoRepository.BuscarTalhaoId(userId, talhaoJson.TalhaoID);
                
                if (talhao != null)
                {
                    // Atualizar os campos
                    talhaoJson.Nome = dto.Nome;
                    if (dto.Observacao != null)
                    {
                        talhaoJson.Observacao = dto.Observacao;
                    }
                    
                    _talhaoRepository.AtualizarTalhaoJson(talhaoJson);
                    UnitOfWork.Commit();
                    return true;
                }
            }
            
            return false;
        }

        // Listar talhões agrupados por fazenda
        public List<TalhaoAgrupadoPorFazendaResponseDTO> ListarTalhoesAgrupadosPorFazenda(Guid userId, Guid? fazendaId = null)
        {
            var talhoes = _talhaoRepository.ListarTodosComFazenda(userId, fazendaId);
            
            // Agrupar por fazenda
            var talhoesAgrupados = talhoes
                .GroupBy(t => new { t.FazendaID, t.Fazenda.Nome })
                .Select(g => new TalhaoAgrupadoPorFazendaResponseDTO
                {
                    FazendaID = g.Key.FazendaID,
                    NomeFazenda = g.Key.Nome,
                    Talhoes = g.SelectMany(t => 
                    {
                        var talhaoJsonList = _talhaoRepository.BuscarTalhaoJson(t.Id);
                        if (talhaoJsonList != null && talhaoJsonList.Any())
                        {
                            return talhaoJsonList.Select(tj => new TalhaoResumoDTO
                            {
                                Id = tj.Id,
                                Nome = tj.Nome,
                                Area = double.TryParse(tj.Area, out var area) ? area : 0,
                                Observacao = tj.Observacao
                            }).ToList();
                        }
                        return new List<TalhaoResumoDTO>();
                    }).ToList()
                })
                .ToList();

            return talhoesAgrupados;
        }
    }
}
