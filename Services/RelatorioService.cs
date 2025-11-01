using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils.Maps;
using api.minionStorage.Services;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace api.coleta.Services
{
    public class RelatorioService : ServiceBase
    {
        private readonly RelatorioRepository _relatorioRepository;
        private readonly IMinioStorage _minioStorage;

        public RelatorioService(RelatorioRepository relatorioRepository, IMinioStorage minioStorage, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _relatorioRepository = relatorioRepository;
            _minioStorage = minioStorage;
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

            // Buscar relatórios com filtros
            var (relatorios, totalItems) = await _relatorioRepository.ListarRelatoriosMobileAsync(userId, query);

            if (relatorios == null || relatorios.Count == 0)
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

            // Mapear para DTO
            var items = new List<RelatorioMobileItemDTO>();

            foreach (var relatorio in relatorios)
            {
                if (relatorio.Coleta == null) continue;

                var coleta = relatorio.Coleta;
                var talhao = coleta.Talhao;
                var fazenda = talhao?.Talhao?.Fazenda ?? coleta.Safra?.Fazenda;

                // Extrair pontos do GeoJSON
                var pontos = ExtrairPontosDoGeoJson(coleta.Geojson);

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

        private List<PontoColetaMobileDTO> ExtrairPontosDoGeoJson(Geojson? geojson)
        {
            var pontos = new List<PontoColetaMobileDTO>();

            if (geojson == null || string.IsNullOrEmpty(geojson.Pontos))
                return pontos;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var geoJsonData = JsonSerializer.Deserialize<JsonElement>(geojson.Pontos, options);

                if (geoJsonData.TryGetProperty("points", out JsonElement pointsElement))
                {
                    foreach (var point in pointsElement.EnumerateArray())
                    {
                        if (point.TryGetProperty("geometry", out JsonElement geometry) &&
                            geometry.TryGetProperty("coordinates", out JsonElement coordinates) &&
                            point.TryGetProperty("properties", out JsonElement properties))
                        {
                            var ponto = new PontoColetaMobileDTO
                            {
                                Id = properties.TryGetProperty("id", out JsonElement id)
                                    ? id.GetInt32().ToString()
                                    : Guid.NewGuid().ToString(),
                                Longitude = coordinates[0].GetDouble(),
                                Latitude = coordinates[1].GetDouble(),
                                Coletado = properties.TryGetProperty("coletado", out JsonElement coletado)
                                    ? coletado.GetBoolean()
                                    : false,
                                DadosAmostra = null // Dados de análise não disponíveis no momento
                            };

                            pontos.Add(ponto);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro (pode adicionar logger aqui)
                Console.WriteLine($"Erro ao extrair pontos do GeoJSON: {ex.Message}");
            }

            return pontos;
        }

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
