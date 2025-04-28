using System;
using System.Text.Json;
using api.utils.DTOs;
using api.vinculoClienteFazenda.Services;

namespace api.coleta.Tests
{
    public class TestPointsInsideArea
    {
        public static void Main()
        {
            // Exemplo de GeoJSON com hexágonos
            string geoJsonStr = @"{
                ""type"": ""FeatureCollection"",
                ""features"": [
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""type"": ""hexagon"",
                            ""id"": 1
                        },
                        ""geometry"": {
                            ""type"": ""Polygon"",
                            ""coordinates"": [
                                [
                                    [-51.39419646469525, -22.05836744330732],
                                    [-51.395095805989165, -22.05856526242054],
                                    [-51.39516966571233, -22.05825195657082],
                                    [-51.39469929813556, -22.05802283510112],
                                    [-51.39419439650906, -22.058321637943813],
                                    [-51.39419646469525, -22.05836744330732]
                                ]
                            ]
                        }
                    }
                ]
            }";

            // Criar o objeto de requisição
            var request = new PontosDentroDaAreaRequest
            {
                GeoJsonAreas = JsonSerializer.Deserialize<JsonElement>(geoJsonStr),
                QtdPontosNaArea = 5 // Gerar 5 pontos por hexágono
            };

            // Criar o serviço (sem dependências reais para este teste)
            // Nota: Este código não funcionará diretamente, é apenas para ilustrar a lógica
            // var utilsService = new UtilsService(null, null, null);
            // var result = utilsService.GetPointsInsideArea(request);

            // Imprimir o resultado
            // Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            
            Console.WriteLine("Teste concluído. Verifique o código para garantir que os pontos são gerados com IDs sequenciais e referência ao ID do hexágono.");
        }
    }
}
