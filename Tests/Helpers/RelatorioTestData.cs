using api.coleta.Models.Entidades;
using System;
using System.Collections.Generic;

namespace api.coleta.Tests.Helpers;

public static class RelatorioTestData
{
    public static (Guid UsuarioId, Coleta Coleta, Relatorio RelatorioComLink) SeedRelatorios(ApplicationDbContext context)
    {
        var usuario = new Usuario
        {
            NomeCompleto = "Usuario Teste",
            Email = "usuario@example.com",
            Telefone = "11999999999"
        };

        var cliente = new Cliente
        {
            Nome = "Cliente",
            Documento = "12345678901234",
            Email = "cliente@example.com",
            Telefone = "11988888888",
            Cep = "01001000",
            Endereco = "Rua X",
            Cidade = "Cidade",
            Estado = "SP",
            UsuarioID = usuario.Id
        };

        var fazenda = new api.fazenda.Models.Entidades.Fazenda
        {
            Nome = "Fazenda",
            Endereco = "Endereco",
            Lat = 0,
            Lng = 0,
            Cliente = cliente,
            ClienteID = cliente.Id,
            Usuario = usuario,
            UsuarioID = usuario.Id
        };

        var talhao = new Talhao
        {
            Fazenda = fazenda,
            FazendaID = fazenda.Id,
            Cliente = cliente,
            ClienteID = cliente.Id,
            Usuario = usuario,
            UsuarioID = usuario.Id
        };

        var talhaoJson = new TalhaoJson
        {
            Talhao = talhao,
            TalhaoID = talhao.Id,
            Area = "10",
            Coordenadas = "[]",
            Nome = "Talhao 1"
        };

        var safra = new Safra
        {
            Observacao = "Safra 2024",
            DataInicio = new DateTime(2024, 1, 1),
            Fazenda = fazenda,
            FazendaID = fazenda.Id,
            Cliente = cliente,
            ClienteID = cliente.Id,
            Usuario = usuario,
            UsuarioID = usuario.Id
        };

        var coleta = new Coleta
        {
            Talhao = talhaoJson,
            TalhaoID = talhaoJson.Id,
            GeojsonID = Guid.NewGuid(),
            Geojson = new Geojson { Pontos = "[]", Grid = "[]" },
            UsuarioResp = usuario,
            UsuarioRespID = usuario.Id,
            Usuario = usuario,
            UsuarioID = usuario.Id,
            Safra = safra,
            SafraID = safra.Id,
            NomeColeta = "Coleta Principal",
            Observacao = "Observacao",
            TipoColeta = TipoColeta.Hexagonal,
            TipoAnalise = new List<TipoAnalise> { TipoAnalise.Macronutrientes },
            Profundidade = Profundidade.ZeroADez
        };

        var relatorioValido = new Relatorio
        {
            Coleta = coleta,
            ColetaId = coleta.Id,
            LinkBackup = "https://example.com/relatorio.pdf",
            JsonRelatorio = "{}",
            UsuarioId = usuario.Id
        };

        var relatorioSemLink = new Relatorio
        {
            Coleta = coleta,
            ColetaId = coleta.Id,
            LinkBackup = string.Empty,
            JsonRelatorio = "{}",
            UsuarioId = usuario.Id
        };

        var relatorioOutroUsuario = new Relatorio
        {
            Coleta = coleta,
            ColetaId = coleta.Id,
            LinkBackup = "https://example.com/outro.pdf",
            JsonRelatorio = "{}",
            UsuarioId = Guid.NewGuid()
        };

        context.Relatorios.AddRange(relatorioValido, relatorioSemLink, relatorioOutroUsuario);
        context.SaveChanges();

        return (usuario.Id, coleta, relatorioValido);
    }
}
