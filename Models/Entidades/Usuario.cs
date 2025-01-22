using api.coleta.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades;

public class Usuario : Entity
{
    [MaxLength(100)]
    public string NomeCompleto { get; set; }

    [MaxLength(11)]
    public string CPF { get; set; }

    [MaxLength(100)]
    public string Email { get; set; }

    [MaxLength(100)]
    public string Telefone { get; set; }

    [MaxLength(100)]
    public string Senha { get; set; }

    [ForeignKey(nameof(UsuarioSupervisor))]
    public Guid? UsuarioSupervisorID { get; set; }
    public Usuario? UsuarioSupervisor { get; set; }

    public Usuario()
    {
    }
    public Usuario(UsuarioResquestDTO usuario)
    {
        NomeCompleto = usuario.NomeCompleto;
        CPF = usuario.CPF;
        Email = usuario.Email;
        Telefone = usuario.Telefone;
        Senha = usuario.Senha;
        Validador();
    }

    private void Validador()
    {
        ValidadorDeEntidade.ValidarSeVazioOuNulo(NomeCompleto, "O campo NomeCompleto não pode estar vazio");
        ValidadorDeEntidade.ValidarSeVazioOuNulo(CPF, "O campo Cpf do usuário não pode estar vazio");
        ValidadorDeEntidade.ValidarCpf(CPF, "O campo CPF do usuário é inválido");
    }
}
