using api.coleta.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;

namespace api.coleta.Services
{
    public class ContatoService : ServiceBase
    {
        private readonly ContatoRepository _contatoRepository;
        private readonly IZeptomailService _zeptomailService;

        public ContatoService(
            ContatoRepository contatoRepository,
            IZeptomailService zeptomailService,
            IUnitOfWork unitOfWork
        ) : base(unitOfWork)
        {
            _contatoRepository = contatoRepository;
            _zeptomailService = zeptomailService;
        }

        public async Task<ContatoResponseDTO> SalvarContatoAsync(ContatoRequestDTO dto)
        {
            // Criar entidade
            var contato = new Contato
            {
                NomeCompleto = dto.NomeCompleto,
                Cidade = dto.Cidade,
                Email = dto.Email,
                NumeroTelefone = dto.NumeroTelefone
            };

            // Persistir no banco
            _contatoRepository.Adicionar(contato);
            UnitOfWork.Commit();

            // Enviar emails de forma assíncrona
            var emailUsuarioTask = _zeptomailService.EnviarEmailConfirmacaoAsync(
                contato.NomeCompleto,
                contato.Email
            );

            var emailAdminsTask = _zeptomailService.EnviarEmailNotificacaoAdminsAsync(dto);

            // Aguardar ambos os envios
            var resultados = await Task.WhenAll(emailUsuarioTask, emailAdminsTask);

            // Atualizar status de envio
            contato.EmailUsuarioEnviado = resultados[0];
            contato.EmailAdminsEnviado = resultados[1];
            contato.DataEnvioEmail = DateTime.Now;

            _contatoRepository.Atualizar(contato);
            UnitOfWork.Commit();

            // Retornar resposta
            return new ContatoResponseDTO
            {
                Id = contato.Id,
                NomeCompleto = contato.NomeCompleto,
                Cidade = contato.Cidade,
                Email = contato.Email,
                NumeroTelefone = contato.NumeroTelefone,
                EmailEnviado = contato.EmailUsuarioEnviado && contato.EmailAdminsEnviado,
                DataInclusao = contato.DataInclusao
            };
        }

        public List<ContatoResponseDTO> ListarContatos(int page, int pageSize)
        {
            var contatos = _contatoRepository.ListarContatos(page, pageSize);

            return contatos.Select(c => new ContatoResponseDTO
            {
                Id = c.Id,
                NomeCompleto = c.NomeCompleto,
                Cidade = c.Cidade,
                Email = c.Email,
                NumeroTelefone = c.NumeroTelefone,
                EmailEnviado = c.EmailUsuarioEnviado && c.EmailAdminsEnviado,
                DataInclusao = c.DataInclusao
            }).ToList();
        }

        public int ContarContatos()
        {
            return _contatoRepository.ContarContatos();
        }
    }
}
