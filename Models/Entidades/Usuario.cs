using api.coleta.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class Usuario : Entity
    {
        [MaxLength(100)]
        public string? NomeCompleto { get; set; }

        [MaxLength(11)]
        public string? CPF { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Telefone { get; set; }

        [MaxLength(100)]
        public string? Senha { get; set; }

        [MaxLength(500)]
        public string? Observacao { get; set; }

        public bool Ativo { get; set; }

        public Guid? adminId { get; set; }
        public virtual Usuario? Admin { get; set; }
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
            ValidadorDeEntidade.ValidarSeVazioOuNulo(NomeCompleto ?? "", "O campo NomeCompleto não pode estar vazio");
            ValidadorDeEntidade.ValidarSeVazioOuNulo(CPF ?? "", "O campo Cpf do usuário não pode estar vazio");
            ValidadorDeEntidade.ValidarCpf(CPF ?? "", "O campo CPF do usuário é inválido");
            ValidadorDeEntidade.ValidarSeVazioOuNulo(Email ?? "", "O campo Email do usuário não pode estar vazio");
            ValidadorDeEntidade.ValidarSeVazioOuNulo(Telefone ?? "", "O campo Telefone do usuário não pode estar vazio");
            ValidadorDeEntidade.ValidarSeVazioOuNulo(Senha ?? "", "O campo Senha do usuário não pode estar vazio");
            ValidadorDeEntidade.ValidarMinimoMaximo(Senha ?? "", 6, 12, "O campo Senha do usuário deve ter entre 6 e 12 caracteres");
            ValidadorDeEntidade.ValidarMinimoMaximo(NomeCompleto ?? "", 3, 100, "O campo NomeCompleto do usuário deve ter entre 3 e 100 caracteres");
            ValidadorDeEntidade.ValidarMinimoMaximo(Email ?? "", 3, 100, "O campo Email do usuário deve ter entre 3 e 100 caracteres");
            ValidadorDeEntidade.ValidarMinimoMaximo(Telefone ?? "", 3, 100, "O campo Telefone do usuário deve ter entre 3 e 100 caracteres");
            ValidadorDeEntidade.ValidarMinimoMaximo(CPF ?? "", 3, 100, "O campo CPF do usuário deve ter entre 3 e 100 caracteres");
            ValidadorDeEntidade.ValidarMinimoMaximo(CPF ?? "", 11, 11, "O campo CPF do usuário deve ter 11 caracteres");
        }

        // atualizar usuario
        public Usuario Atualizar(UsuarioResquestDTO usuario)
        {
            NomeCompleto = usuario.NomeCompleto;
            CPF = usuario.CPF;
            Email = usuario.Email;
            Telefone = usuario.Telefone;
            Senha = usuario.Senha;
            Validador();
            return this;
        }

        // atualizar funcionario incluindo observacao e ativo
        public Usuario AtualizarFuncionario(UsuarioResquestDTO usuario, string? observacao, bool ativo)
        {
            NomeCompleto = usuario.NomeCompleto;
            CPF = usuario.CPF;
            Email = usuario.Email;
            Telefone = usuario.Telefone;
            Senha = usuario.Senha;
            Observacao = observacao;
            Ativo = ativo;
            Validador();
            return this;
        }
    }
}