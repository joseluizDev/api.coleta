using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils.Maps;
using api.minionStorage.Services;
using System.Text.Json;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace api.coleta.Services
{
    public class RelatorioService : ServiceBase
    {
        private readonly RelatorioRepository _relatorioRepository;
        private readonly IMinioStorage _minioStorage;
        private readonly PontoColetadoRepository _pontoColetadoRepository;

        public RelatorioService(RelatorioRepository relatorioRepository, IMinioStorage minioStorage, PontoColetadoRepository pontoColetadoRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _relatorioRepository = relatorioRepository;
            _minioStorage = minioStorage;
            _pontoColetadoRepository = pontoColetadoRepository;
        }

        public async Task<string?> SalvarRelatorio(RelatorioDTO arquivo, Guid userId)
        {
            string bucketName = "coleta";
            var file = arquivo.Arquivo;

            string fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            string contentType = file.ContentType;
            string objectName = $"{Guid.NewGuid()}.{fileExtension}";

            using var stream = file.OpenReadStream();
            string url = await _minioStorage.UploadFileAsync(bucketName, objectName, stream, contentType);

            if (url != null)
            {
                Relatorio map = RelatorioMapDto.MapRelatorio(arquivo);
                map.LinkBackup = url;
                map.UsuarioId = userId;

                _relatorioRepository.Adicionar(map);
                UnitOfWork.Commit();
                return "Opa";

            }

            return url;
        }

        public async Task<RelatorioOuputDTO?> GetRelario(Guid id, Guid userId)
        {
            var relatorio = await _relatorioRepository.ObterPorId(id, userId);
            if (relatorio != null)
            {
                return RelatorioMapDto.MapRelatorio(relatorio);
            }
            return null;

        }

        public async Task<List<RelatorioOuputDTO>> ListarRelatoriosPorUploadAsync(Guid userId)
        {

            var relatorios = await _relatorioRepository.ListarRelatoriosPorUploadAsync(userId);
            if (relatorios == null || relatorios.Count == 0)
            {
                return [];
            }

            return relatorios.MapRelatorio();
        }

        public async Task<bool> AtualizarJsonRelatorioAsync(Guid coletaId, Guid relatorioId, Guid userId, string jsonRelatorio)
        {
            var relatorio = await _relatorioRepository.ObterPorIdColetaRelatorio(coletaId, relatorioId, userId);
            if (relatorio == null)
            {
                return false;
            }

            relatorio.JsonRelatorio = jsonRelatorio;

            _relatorioRepository.Atualizar(relatorio);
            UnitOfWork.Commit();

            return true;
        }

        public async Task<RelatorioMobileResponseDTO> ListarRelatoriosMobileAsync(Guid userId, QueryRelatorioMobile query)
        {
            // Validar parâmetros de data
            if (query.DataInicio.HasValue && query.DataFim.HasValue && query.DataInicio > query.DataFim)
            {
                return new RelatorioMobileResponseDTO
                {
                    Success = false,
                    Data = new List<RelatorioMobileItemDTO>()
                };
            }

            // 1) Tenta buscar Relatórios existentes (fluxo original)
            var (relatorios, totalItems) = await _relatorioRepository.ListarRelatoriosMobileAsync(userId, query);

            // 2) Se não houver relatórios, fazer fallback para COLETAS do usuário (para não retornar vazio)
            if (relatorios == null || relatorios.Count == 0)
            {
                var (coletas, totalColetas) = await _relatorioRepository.ListarColetasParaRelatorioMobileAsync(userId, query);

                // Se também não houver coletas, retorna vazio
                if (coletas == null || coletas.Count == 0)
                {
                    return new RelatorioMobileResponseDTO
                    {
                        Success = true,
                        Data = new List<RelatorioMobileItemDTO>(),
                        Pagination = new PaginationDTO
                        {
                            CurrentPage = query.Page,
                            TotalPages = 0,
                            TotalItems = 0,
                            ItemsPerPage = query.Limit
                        }
                    };
                }

                var itemsFallback = new List<RelatorioMobileItemDTO>();
                foreach (var coleta in coletas)
                {
                    var talhaoC = coleta.Talhao;
                    var fazendaC = talhaoC?.Talhao?.Fazenda ?? coleta.Safra?.Fazenda;

                    // Marcar pontos coletados usando a tabela PontoColetado
                    var coletadosC = await _pontoColetadoRepository.BuscarPontosPorColetaAsync(coleta.Id);
                    var pontosC = ExtrairPontosDoGeoJsonMarcandoColetados(coleta.Geojson, coletadosC);

                    itemsFallback.Add(new RelatorioMobileItemDTO
                    {
                        Id = coleta.Id.ToString(),
                        Fazenda = fazendaC?.Nome ?? "N/A",
                        Talhao = talhaoC?.Nome ?? "N/A",
                        Data = coleta.DataInclusao.ToString("yyyy-MM-dd"),
                        PontosColetados = ContarPontosColetados(pontosC),
                        TotalPontos = pontosC.Count,
                        Profundidade = FormatarProfundidade(coleta.Profundidade),
                        Grid = DeterminarGrid(coleta.TipoColeta),
                        Localizacao = fazendaC?.Endereco ?? "N/A",
                        TalhaoJson = talhaoC?.Coordenadas,
                        Pontos = pontosC
                    });
                }

                int totalPagesFallback = (int)Math.Ceiling(totalColetas / (double)query.Limit);

                return new RelatorioMobileResponseDTO
                {
                    Success = true,
                    Data = itemsFallback,
                    Pagination = new PaginationDTO
                    {
                        CurrentPage = query.Page,
                        TotalPages = totalPagesFallback,
                        TotalItems = totalColetas,
                        ItemsPerPage = query.Limit
                    }
                };
            }

            // 3) Caso haja relatórios, agrupa por ColetaId para evitar duplicidade e mapeia
            var items = new List<RelatorioMobileItemDTO>();

            var relatoriosDistinct = relatorios
                .GroupBy(r => r.ColetaId)
                .Select(g => g.OrderByDescending(r => r.DataInclusao).First())
                .ToList();

            foreach (var relatorio in relatoriosDistinct)
            {
                if (relatorio.Coleta == null) continue;

                var coleta = relatorio.Coleta;
                var talhao = coleta.Talhao;
                var fazenda = talhao?.Talhao?.Fazenda ?? coleta.Safra?.Fazenda;

                // Extrair pontos do GeoJSON
                // Marcar pontos coletados usando a tabela PontoColetado
                var coletados = await _pontoColetadoRepository.BuscarPontosPorColetaAsync(coleta.Id);
                var pontos = ExtrairPontosDoGeoJsonMarcandoColetados(coleta.Geojson, coletados);

                var item = new RelatorioMobileItemDTO
                {
                    Id = relatorio.Id.ToString(),
                    Fazenda = fazenda?.Nome ?? "N/A",
                    Talhao = talhao?.Nome ?? "N/A",
                    Data = relatorio.DataInclusao.ToString("yyyy-MM-dd"),
                    PontosColetados = ContarPontosColetados(pontos),
                    TotalPontos = pontos.Count,
                    Profundidade = FormatarProfundidade(coleta.Profundidade),
                    Grid = DeterminarGrid(coleta.TipoColeta),
                    Localizacao = fazenda?.Endereco ?? "N/A",
                    TalhaoJson = talhao?.Coordenadas,
                    Pontos = pontos
                };

                items.Add(item);
            }

            // Calcular paginação
            int totalPages = (int)Math.Ceiling(totalItems / (double)query.Limit);

            return new RelatorioMobileResponseDTO
            {
                Success = true,
                Data = items,
                Pagination = new PaginationDTO
                {
                    CurrentPage = query.Page,
                    TotalPages = totalPages,
                    TotalItems = totalItems,
                    ItemsPerPage = query.Limit
                }
            };
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private List<PontoColetaMobileDTO> ExtrairPontosDoGeoJsonMarcandoColetados(Geojson? geojson, List<PontoColetado> pontosColetados)
        {
            var pontos = new List<PontoColetaMobileDTO>();
            if (geojson == null || string.IsNullOrEmpty(geojson.Pontos)) return pontos;

            try
            {
                var geoJsonData = JsonSerializer.Deserialize<JsonElement>(geojson.Pontos, _jsonOptions);
                if (geoJsonData.TryGetProperty("points", out JsonElement pointsElement))
                {
                    // Preparar conjunto para match por coordenadas com tolerância
                    var coletadosSet = new HashSet<(long lat, long lon)>();
                    foreach (var pc in pontosColetados)
                    {
                        coletadosSet.Add((Round6(pc.Latitude), Round6(pc.Longitude)));
                    }

                    foreach (var point in pointsElement.EnumerateArray())
                    {
                        if (point.TryGetProperty("geometry", out JsonElement geometry) &&
                            geometry.TryGetProperty("coordinates", out JsonElement coordinates) &&
                            point.TryGetProperty("properties", out JsonElement properties))
                        {
                            var lon = coordinates[0].GetDouble();
                            var lat = coordinates[1].GetDouble();
                            var marcadoPorJson = properties.TryGetProperty("coletado", out JsonElement coletadoJson) && coletadoJson.GetBoolean();
                            var marcadoPorTabela = coletadosSet.Contains((Round6(lat), Round6(lon)));

                            pontos.Add(new PontoColetaMobileDTO
                            {
                                Id = properties.TryGetProperty("id", out JsonElement id)
                                    ? id.GetInt32().ToString()
                                    : Guid.NewGuid().ToString(),
                                Longitude = lon,
                                Latitude = lat,
                                Coletado = marcadoPorJson || marcadoPorTabela,
                                DadosAmostra = null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao extrair pontos do GeoJSON: {ex.Message}");
            }

            return pontos;
        }

        private static long Round6(double value) => (long)Math.Round(value * 1_000_000d);

        private int ContarPontosColetados(List<PontoColetaMobileDTO> pontos)
        {
            return pontos.Count(p => p.Coletado);
        }

        private string FormatarProfundidade(Profundidade profundidade)
        {
            return profundidade switch
            {
                Profundidade.ZeroADez => "0-10 cm",
                Profundidade.ZeroAVinte => "0-20 cm",
                Profundidade.ZeroATrinta => "0-30 cm",
                Profundidade.ZeroAQuarenta => "0-40 cm",
                Profundidade.ZeroACinquenta => "0-50 cm",
                Profundidade.ZeroASetenta => "0-70 cm",
                Profundidade.DezAVinte => "10-20 cm",
                Profundidade.VinteATrinta => "20-30 cm",
                Profundidade.TrintaAQuarenta => "30-40 cm",
                Profundidade.QuarentaACinquenta => "40-50 cm",
                Profundidade.CinquentaASetenta => "50-70 cm",
                _ => "N/A"
            };
        }

        private string DeterminarGrid(TipoColeta tipoColeta)
        {
            return tipoColeta switch
            {
                TipoColeta.Hexagonal => "Hexagonal",
                TipoColeta.Retangular => "Retangular",
                TipoColeta.PontosAmostrais => "Pontos Amostrais",
                _ => "N/A"
            };
        }
    }
}
