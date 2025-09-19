using System;
using System.Collections.Generic;
using api.coleta.Models.Entidades;
using api.coleta.Utils.Maps;
using api.fazenda.Models.Entidades;
using Xunit;

namespace api.coleta.Tests
{
    public class RelatorioMapDtoTests
    {
        [Fact]
        public void MapRelatorio_WithCompleteData_ShouldPopulateAllFields()
        {
            // arrange
            var coletaId = Guid.NewGuid();
            var coleta = new Coleta
            {
                NomeColeta = "Talhao 1",
                Talhao = new TalhaoJson
                {
                    Nome = "Talhao 1",
                    Talhao = new Talhao
                    {
                        Fazenda = new Fazenda
                        {
                            Nome = "Fazenda Primavera"
                        }
                    }
                },
                Safra = new Safra
                {
                    Observacao = null,
                    DataInicio = new DateTime(2024, 1, 1),
                    Fazenda = new Fazenda
                    {
                        Nome = "Fazenda Safra"
                    }
                },
                UsuarioResp = new Usuario
                {
                    NomeCompleto = "helber prates"
                },
                Observacao = "Observacao da coleta",
                TipoColeta = TipoColeta.Hexagonal,
                TipoAnalise = new List<TipoAnalise>
                {
                    TipoAnalise.Macronutrientes,
                    TipoAnalise.Micronutrientes,
                    TipoAnalise.Textura
                },
                Profundidade = Profundidade.ZeroADez
            };

            var relatorio = new Relatorio
            {
                ColetaId = coletaId,
                LinkBackup = "https://example.com/relatorio.pdf",
                JsonRelatorio = "{\"value\":1}",
                UsuarioId = Guid.NewGuid(),
                Coleta = coleta
            };

            // act
            var dto = relatorio.MapRelatorio();

            // assert
            Assert.Equal(relatorio.Id, dto.Id);
            Assert.Equal(coletaId.ToString(), dto.ColetaId);
            Assert.Equal(relatorio.LinkBackup, dto.LinkBackup);
            Assert.Equal(relatorio.DataInclusao, dto.DataInclusao);
            Assert.Equal("Talhao 1", dto.NomeColeta);
            Assert.Equal("Talhao 1", dto.Talhao);
            Assert.Equal("Hexagonal", dto.TipoColeta);
            Assert.Equal("Fazenda Primavera", dto.Fazenda);
            Assert.Equal("01/01/2024", dto.Safra);
            Assert.Equal("helber prates", dto.Funcionario);
            Assert.Equal("Observacao da coleta", dto.Observacao);
            Assert.Equal("ZeroADez", dto.Profundidade);
            Assert.Equal(new[] { "Macronutrientes", "Micronutrientes", "Textura" }, dto.TiposAnalise);
        }

        [Fact]
        public void MapRelatorio_WithMissingData_ShouldFallbackToDefaults()
        {
            // arrange
            var relatorio = new Relatorio
            {
                ColetaId = Guid.NewGuid(),
                LinkBackup = string.Empty,
                JsonRelatorio = string.Empty,
                UsuarioId = Guid.NewGuid(),
                Coleta = null
            };

            // act
            var dto = relatorio.MapRelatorio();

            // assert
            Assert.Equal("N/A", dto.NomeColeta);
            Assert.Equal("N/A", dto.Talhao);
            Assert.Equal("N/A", dto.TipoColeta);
            Assert.Equal("N/A", dto.Fazenda);
            Assert.Equal("N/A", dto.Safra);
            Assert.Equal("N/A", dto.Funcionario);
            Assert.Equal("N/A", dto.Observacao);
            Assert.Equal("N/A", dto.Profundidade);
            Assert.Empty(dto.TiposAnalise);
        }
    }
}
