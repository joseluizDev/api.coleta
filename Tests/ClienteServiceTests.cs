using api.cliente.Models.DTOs;
using api.cliente.Repositories;
using api.cliente.Services;
using api.coleta.Models.Entidades;
using api.coleta.Data;
using System.Linq;
using Xunit;

namespace api.coleta.Tests;

public class ClienteServiceTests
{
    private ClienteService CreateService(ApplicationDbContext context)
    {
        var repository = new ClienteRepository(context);
        var unitOfWork = new UnitOfWorkImplements(context);
        return new ClienteService(repository, unitOfWork);
    }

    [Fact]
    public void SalvarCliente_ShouldPersistClienteWithUsuarioId()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var usuarioId = Guid.NewGuid();
        var request = new ClienteRequestDTO
        {
            Nome = "Cliente Teste",
            Documento = "12345678901234",
            Email = "teste@example.com",
            Telefone = "11999999999",
            Cep = "12345678",
            Endereco = "Rua Um, 100",
            Cidade = "Cidade",
            Estado = "SP"
        };

        var resultado = service.SalvarCliente(request, usuarioId);

        Assert.NotNull(resultado);
        Assert.Equal(request.Nome, resultado.Nome);

        var entity = context.Clientes.Single();
        Assert.Equal(usuarioId, entity.UsuarioID);
        Assert.Equal(request.Email, entity.Email);
    }

    [Fact]
    public void BuscarClientePorId_WhenClienteNaoPertenceUsuario_DeveRetornarNull()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var outroUsuarioId = Guid.NewGuid();
        var cliente = new Cliente
        {
            Nome = "Cliente Externo",
            Documento = "23456789012345",
            Email = "externo@example.com",
            Telefone = "11888888888",
            Cep = "87654321",
            Endereco = "Rua Dois, 200",
            Cidade = "Outra Cidade",
            Estado = "RJ",
            UsuarioID = outroUsuarioId
        };

        context.Clientes.Add(cliente);
        context.SaveChanges();

        var resultado = service.BuscarClientePorId(Guid.NewGuid(), cliente.Id);

        Assert.Null(resultado);
    }

    [Fact]
    public void AtualizarCliente_DeveAtualizarDadosBasicos()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var service = CreateService(context);

        var usuarioId = Guid.NewGuid();
        var cliente = new Cliente
        {
            Nome = "Cliente Original",
            Documento = "34567890123456",
            Email = "original@example.com",
            Telefone = "11777777777",
            Cep = "11223344",
            Endereco = "Rua Tres, 300",
            Cidade = "Cidade",
            Estado = "MG",
            UsuarioID = usuarioId
        };

        context.Clientes.Add(cliente);
        context.SaveChanges();

        var request = new ClienteRequestDTO
        {
            Id = cliente.Id,
            Nome = "Cliente Atualizado",
            Documento = cliente.Documento,
            Email = "atualizado@example.com",
            Telefone = "11666666666",
            Cep = "11223344",
            Endereco = "Rua Quatro, 123",
            Cidade = "Nova Cidade",
            Estado = "PR"
        };

        var resultado = service.AtualizarCliente(usuarioId, request);

        Assert.NotNull(resultado);
        Assert.Equal(request.Nome, resultado!.Nome);
        Assert.Equal(request.Email, resultado.Email);
        Assert.Equal(request.Endereco, resultado.Endereco);

        var entity = context.Clientes.Single();
        Assert.Equal(request.Nome, entity.Nome);
        Assert.Equal(request.Email, entity.Email);
        Assert.Equal(request.Endereco, entity.Endereco);
        Assert.Equal(request.Cidade, entity.Cidade);
        Assert.Equal(request.Estado, entity.Estado);
    }
}
