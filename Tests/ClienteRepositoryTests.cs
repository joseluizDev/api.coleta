using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.coleta.Models.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace api.coleta.Tests;

public class ClienteRepositoryTests
{
    [Fact]
    public void ListarClientes_ComFiltroNome_DeveRetornarSomenteCorrespondentes()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var repository = new ClienteRepository(context);

        var usuarioId = Guid.NewGuid();
        var outrosUsuarioId = Guid.NewGuid();

        context.Clientes.AddRange(new List<Cliente>
        {
            new Cliente
            {
                Nome = "Maria Silva",
                Documento = "12345678901234",
                Email = "maria@example.com",
                Telefone = "11999990000",
                Cep = "01001000",
                Endereco = "Rua A",
                Cidade = "Sao Paulo",
                Estado = "SP",
                UsuarioID = usuarioId
            },
            new Cliente
            {
                Nome = "Mariana Souza",
                Documento = "22345678901234",
                Email = "mariana@example.com",
                Telefone = "11888880000",
                Cep = "01001000",
                Endereco = "Rua B",
                Cidade = "Sao Paulo",
                Estado = "SP",
                UsuarioID = usuarioId
            },
            new Cliente
            {
                Nome = "Joao Lima",
                Documento = "32345678901234",
                Email = "joao@example.com",
                Telefone = "11777770000",
                Cep = "01001000",
                Endereco = "Rua C",
                Cidade = "Sao Paulo",
                Estado = "SP",
                UsuarioID = outrosUsuarioId
            }
        });

        context.SaveChanges();

        var query = new QueryClienteDTO
        {
            Page = 1,
            Nome = "Maria"
        };

        var resultado = repository.ListarClientes(usuarioId, query);

        Assert.Equal(2, resultado.Items.Count);
        Assert.All(resultado.Items, item => Assert.Contains("Maria", item.Nome));
        Assert.Equal(1, resultado.CurrentPage);
    }

    [Fact]
    public void BuscarClientes_DeveRetornarSomenteClientesDoUsuarioNaPagina()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var repository = new ClienteRepository(context);

        var usuarioId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();

        for (int i = 0; i < 15; i++)
        {
            context.Clientes.Add(new Cliente
            {
                Nome = $"Cliente {i}",
                Documento = $"{i:D14}",
                Email = $"cliente{i}@example.com",
                Telefone = $"110000{i:D4}",
                Cep = "01001000",
                Endereco = "Rua Teste",
                Cidade = "Cidade",
                Estado = "SP",
                UsuarioID = usuarioId
            });
        }

        context.Clientes.Add(new Cliente
        {
            Nome = "Outro Usuario",
            Documento = "99999999999999",
            Email = "outro@example.com",
            Telefone = "11999999999",
            Cep = "01001000",
            Endereco = "Rua Outra",
            Cidade = "Cidade",
            Estado = "SP",
            UsuarioID = outroUsuarioId
        });

        context.SaveChanges();

        var resultado = repository.BuscarClientes(usuarioId, page: 2);

        Assert.True(resultado.Count <= 10);
        Assert.NotEmpty(resultado);
        Assert.All(resultado, item => Assert.Equal(usuarioId, item.UsuarioID));
    }
}
