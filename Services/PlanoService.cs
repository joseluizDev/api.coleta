using api.coleta.Models.DTOs.Licenciamento;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;

namespace api.coleta.Services
{
    public class PlanoService : ServiceBase
    {
        private readonly PlanoRepository _planoRepository;
        private readonly INotificador _notificador;

        public PlanoService(
            PlanoRepository planoRepository,
            IUnitOfWork unitOfWork,
            INotificador notificador) : base(unitOfWork)
        {
            _planoRepository = planoRepository;
            _notificador = notificador;
        }

        public async Task<List<PlanoDTO>> ListarPlanosAtivosAsync()
        {
            var planos = await _planoRepository.ListarPlanosAtivosAsync();
            return planos.Select(MapToDTO).ToList();
        }

        public async Task<PlanoDTO?> ObterPorIdAsync(Guid id)
        {
            var plano = await _planoRepository.ObterPorIdAsync(id);
            return plano != null ? MapToDTO(plano) : null;
        }

        public async Task<PlanoDTO?> CriarPlanoAsync(PlanoCreateDTO dto)
        {
            if (await _planoRepository.ExistePlanoComNomeAsync(dto.Nome))
            {
                _notificador.Notificar(new Notificacao("Já existe um plano com este nome."));
                return null;
            }

            var plano = new Plano(
                dto.Nome,
                dto.Descricao,
                dto.ValorAnual,
                dto.LimiteHectares,
                dto.RequereContato
            );

            _planoRepository.Adicionar(plano);
            UnitOfWork.Commit();

            return MapToDTO(plano);
        }

        public async Task<PlanoDTO?> AtualizarPlanoAsync(Guid id, PlanoCreateDTO dto)
        {
            var plano = await _planoRepository.ObterPorIdAsync(id);

            if (plano == null)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado."));
                return null;
            }

            if (await _planoRepository.ExistePlanoComNomeAsync(dto.Nome, id))
            {
                _notificador.Notificar(new Notificacao("Já existe outro plano com este nome."));
                return null;
            }

            plano.Nome = dto.Nome;
            plano.Descricao = dto.Descricao;
            plano.ValorAnual = dto.ValorAnual;
            plano.LimiteHectares = dto.LimiteHectares;
            plano.RequereContato = dto.RequereContato;
            plano.EfiPayPlanIdInt = dto.EfiPayPlanIdInt;

            _planoRepository.Atualizar(plano);
            UnitOfWork.Commit();

            return MapToDTO(plano);
        }

        public async Task<bool> DesativarPlanoAsync(Guid id)
        {
            var plano = await _planoRepository.ObterPorIdAsync(id);

            if (plano == null)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado."));
                return false;
            }

            plano.Ativo = false;
            _planoRepository.Atualizar(plano);
            UnitOfWork.Commit();

            return true;
        }

        private static PlanoDTO MapToDTO(Plano plano)
        {
            return new PlanoDTO
            {
                Id = plano.Id,
                Nome = plano.Nome,
                Descricao = plano.Descricao,
                ValorAnual = plano.ValorAnual,
                LimiteHectares = plano.LimiteHectares,
                Ativo = plano.Ativo,
                RequereContato = plano.RequereContato,
                EfiPayPlanIdInt = plano.EfiPayPlanIdInt
            };
        }
    }
}
