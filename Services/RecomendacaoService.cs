using api.cliente.Interfaces;
using api.coleta.Data;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;

namespace api.coleta.Services
{
    public class RecomendacaoService : ServiceBase
    {
        private readonly RecomendacaoRepository _recomendacaoRepository;
        private readonly RelatorioRepository _relatorioRepository;
        private readonly INotificador _notificador;

        public RecomendacaoService(
            RecomendacaoRepository recomendacaoRepository,
            RelatorioRepository relatorioRepository,
            INotificador notificador,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _recomendacaoRepository = recomendacaoRepository;
            _relatorioRepository = relatorioRepository;
            _notificador = notificador;
        }

        public async Task<RecomendacaoOutputDTO?> CriarRecomendacaoAsync(RecomendacaoDTO dto, Guid userId)
        {
            // Verificar se o relatório pertence ao usuário
            var relatorio = await _recomendacaoRepository.ObterRelatorioPorIdSeguro(dto.RelatorioId, userId);
            if (relatorio == null)
            {
                _notificador.Notificar(new Notificacao("Relatório não encontrado ou não pertence ao usuário."));
                return null;
            }

            var recomendacao = new Recomendacao
            {
                RelatorioId = dto.RelatorioId,
                Descricao = dto.Descricao
            };

            _recomendacaoRepository.Adicionar(recomendacao);
            UnitOfWork.Commit();

            return new RecomendacaoOutputDTO
            {
                Id = recomendacao.Id,
                RelatorioId = recomendacao.RelatorioId,
                Descricao = recomendacao.Descricao,
                DataInclusao = recomendacao.DataInclusao
            };
        }

        public async Task<List<RecomendacaoOutputDTO>> BuscarPorRelatorioAsync(Guid relatorioId, Guid userId)
        {
            // Verificar se o relatório pertence ao usuário
            var relatorio = await _recomendacaoRepository.ObterRelatorioPorIdSeguro(relatorioId, userId);
            if (relatorio == null)
            {
                return new List<RecomendacaoOutputDTO>();
            }

            var recomendacoes = await _recomendacaoRepository.ListarPorRelatorio(relatorioId);
            
            return recomendacoes.Select(r => new RecomendacaoOutputDTO
            {
                Id = r.Id,
                RelatorioId = r.RelatorioId,
                Descricao = r.Descricao,
                DataInclusao = r.DataInclusao
            }).ToList();
        }

        public async Task<RecomendacaoOutputDTO?> AtualizarRecomendacaoAsync(Guid id, RecomendacaoDTO dto, Guid userId)
        {
            var recomendacao = await _recomendacaoRepository.ObterPorId(id);
            if (recomendacao == null)
            {
                return null;
            }

            // Verificar se o relatório pertence ao usuário
            var relatorio = await _recomendacaoRepository.ObterRelatorioPorIdSeguro(recomendacao.RelatorioId, userId);
            if (relatorio == null)
            {
                _notificador.Notificar(new Notificacao("Você não tem permissão para atualizar esta recomendação."));
                return null;
            }

            recomendacao.Descricao = dto.Descricao;

            _recomendacaoRepository.Atualizar(recomendacao);
            UnitOfWork.Commit();

            return new RecomendacaoOutputDTO
            {
                Id = recomendacao.Id,
                RelatorioId = recomendacao.RelatorioId,
                Descricao = recomendacao.Descricao,
                DataInclusao = recomendacao.DataInclusao
            };
        }

        public async Task<bool> DeletarRecomendacaoAsync(Guid id, Guid userId)
        {
            var recomendacao = await _recomendacaoRepository.ObterPorId(id);
            if (recomendacao == null)
            {
                return false;
            }

            // Verificar se o relatório pertence ao usuário
            var relatorio = await _recomendacaoRepository.ObterRelatorioPorIdSeguro(recomendacao.RelatorioId, userId);
            if (relatorio == null)
            {
                _notificador.Notificar(new Notificacao("Você não tem permissão para deletar esta recomendação."));
                return false;
            }

            _recomendacaoRepository.Deletar(recomendacao);
            UnitOfWork.Commit();

            return true;
        }
    }
}
