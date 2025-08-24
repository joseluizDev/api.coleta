using api.coleta.Models.Entidades;
using Microsoft.AspNetCore.Mvc;
using api.coleta.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using api.coleta.models;
using api.coleta.Services;
using api.coleta.Settings;
using Microsoft.Extensions.Options;
using Azure;
using api.coleta.models.dtos;
using api.coleta.Utils;
using Microsoft.EntityFrameworkCore;
namespace api.coleta.Repositories
{
    public class ColetaRepository : GenericRepository<MColeta>
    {
        public ColetaRepository(ApplicationDbContext context) : base(context)
        { }

        public void SalvarColetas(List<MColeta> coletas)
        {
            foreach (var coleta in coletas)
            {
                Adicionar(coleta);
            }
        }

        public async Task<PagedResult<Coleta>> BuscarColetasPorUsuario(Guid userID, QueryColeta query)
        {
            if (query.Page < 1)
                query.Page = 1;

            int pageSize = 10;
            int page = query.Page;

            // Construindo a query base
            var clientesQuery = Context.Coletas
                .Include(c => c.Safra)
                    .ThenInclude(s => s.Fazenda)
                .Include(c => c.Talhao)
                .Where(c => c.UsuarioID == userID);

            // Filtro por Nome
            if (!string.IsNullOrWhiteSpace(query.Nome))
                clientesQuery = clientesQuery.Where(c => c.NomeColeta.Contains(query.Nome));

            // Filtro por Safra
            if (query.SafraID.HasValue)
                clientesQuery = clientesQuery.Where(c => c.SafraID == query.SafraID.Value);

            // Filtro por Cliente
            // if (query.ClienteID.HasValue)
            //     clientesQuery = clientesQuery.Where(c => c. == query.ClienteID.Value);

            // Filtro por Fazenda
            if (query.FazendaID.HasValue)
                clientesQuery = clientesQuery.Where(c => c.Safra.FazendaID == query.FazendaID.Value);

            // Filtro por Talhão
            if (query.TalhaoID.HasValue)
                clientesQuery = clientesQuery.Where(c => c.Talhao.Id == query.TalhaoID.Value);

            // Contagem assíncrona
            int totalItems = await clientesQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Listagem com paginação
            List<Coleta> coletas = await clientesQuery
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Coleta>
            {
                Items = coletas,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }







    }
}
