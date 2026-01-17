using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using api.coleta.Models.Entidades;
using api.talhao.Models.DTOs;

namespace api.coleta.Utils.Maps
{
    public static class TalhaoMap
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static TalhaoResponseDTO? ToResponseDto(this Talhao? talhao)
        {
            if (talhao == null)
            {
                return null;
            }

            return new TalhaoResponseDTO
            {
                Id = talhao.Id,
                FazendaID = talhao.FazendaID,
                ClienteID = talhao.ClienteID,
                Fazenda = talhao.Fazenda.ToResponseDto(),
                Cliente = talhao.Cliente.ToResponseDto(),
                Talhoes = new List<Talhoes>()
            };
        }

        public static Talhao? ToEntity(this TalhaoRequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Talhao
            {
                FazendaID = dto.FazendaID,
                ClienteID = dto.ClienteID
            };
        }

        public static List<TalhaoResponseDTO> ToResponseDtoList(this IEnumerable<Talhao?>? talhoes)
        {
            if (talhoes == null)
            {
                return new List<TalhaoResponseDTO>();
            }

            return talhoes
                .Select(t => t.ToResponseDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }

        public static Talhoes? ToTalhoes(this TalhaoJson? talhaoJson)
        {
            if (talhaoJson == null)
            {
                return null;
            }

            return new Talhoes
            {
                Id = talhaoJson.Id,
                Area = ParseDouble(talhaoJson.Area),
                Nome = talhaoJson.Nome,
                observacao = talhaoJson.Observacao,
                TalhaoID = talhaoJson.TalhaoID,
                Coordenadas = DeserializeCoordenadas(talhaoJson.Coordenadas)
            };
        }

        public static List<Talhoes> ToTalhoesList(this IEnumerable<TalhaoJson?>? talhoesJson)
        {
            if (talhoesJson == null)
            {
                return new List<Talhoes>();
            }

            return talhoesJson
                .Select(t => t.ToTalhoes())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }

        public static TalhaoResponseDTO? ToTalhaoResponseDto(this TalhaoJson? talhaoJson)
        {
            if (talhaoJson == null)
            {
                return null;
            }

            var response = new TalhaoResponseDTO
            {
                Id = talhaoJson.Talhao?.Id ?? Guid.Empty,
                FazendaID = talhaoJson.Talhao?.FazendaID ?? Guid.Empty,
                ClienteID = talhaoJson.Talhao?.ClienteID ?? Guid.Empty,
                Talhoes = new List<Talhoes>
                {
                    new Talhoes
                    {
                        Id = talhaoJson.Id,
                        Area = ParseDouble(talhaoJson.Area),
                        Nome = talhaoJson.Nome,
                        observacao = talhaoJson.Observacao,
                        TalhaoID = talhaoJson.TalhaoID,
                        Coordenadas = new List<Coordenada>()
                    }
                }
            };

            if (talhaoJson.Talhao?.Fazenda != null)
            {
                response.Fazenda = talhaoJson.Talhao.Fazenda.ToResponseDto();
            }

            if (talhaoJson.Talhao?.Cliente != null)
            {
                response.Cliente = talhaoJson.Talhao.Cliente.ToResponseDto();
            }

            return response;
        }

        public static TalhaoJson? ToTalhaoJson(this Talhoes? talhoes)
        {
            if (talhoes == null)
            {
                return null;
            }

            return new TalhaoJson
            {
                TalhaoID = talhoes.TalhaoID,
                Area = talhoes.Area.ToString(Culture),
                Nome = talhoes.Nome,
                Observacao = talhoes.observacao,
                Coordenadas = SerializeCoordenadas(talhoes.Coordenadas)
            };
        }

        public static List<TalhaoJson> ToTalhaoJsonList(this IEnumerable<Talhoes?>? talhoes)
        {
            if (talhoes == null)
            {
                return new List<TalhaoJson>();
            }

            return talhoes
                .Select(t => t.ToTalhaoJson())
                .Where(t => t is not null)
                .Select(t => t!)
                .ToList();
        }

        public static List<Talhoes> CloneTalhoes(this IEnumerable<Talhoes?>? talhoes)
        {
            if (talhoes == null)
            {
                return new List<Talhoes>();
            }

            return talhoes
                .Where(t => t is not null)
                .Select(t => t!.CloneTalhao())
                .ToList();
        }

        private static Talhoes CloneTalhao(this Talhoes talhoes)
        {
            return new Talhoes
            {
                Id = talhoes.Id,
                Area = talhoes.Area,
                Nome = talhoes.Nome,
                observacao = talhoes.observacao,
                TalhaoID = talhoes.TalhaoID,
                Coordenadas = CloneCoordenadas(talhoes.Coordenadas)
            };
        }

        private static List<Coordenada> DeserializeCoordenadas(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Coordenada>();
            }

            try
            {
                var coordenadas = JsonSerializer.Deserialize<List<Coordenada>>(json, JsonOptions);
                return coordenadas ?? new List<Coordenada>();
            }
            catch
            {
                return new List<Coordenada>();
            }
        }

        private static List<Coordenada> CloneCoordenadas(List<Coordenada>? coordenadas)
        {
            if (coordenadas == null)
            {
                return new List<Coordenada>();
            }

            return coordenadas
                .Select(c => new Coordenada { Lat = c.Lat, Lng = c.Lng })
                .ToList();
        }

        private static string SerializeCoordenadas(List<Coordenada>? coordenadas)
        {
            if (coordenadas == null || coordenadas.Count == 0)
            {
                return "[]";
            }

            return JsonSerializer.Serialize(coordenadas, JsonOptions);
        }

        private static double ParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0d;
            }

            return double.TryParse(value, NumberStyles.Any, Culture, out var result) ? result : 0d;
        }
    }
}
