using api.coleta.Models.Entidades;
using Microsoft.AspNetCore.Mvc;
using api.coleta.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using api.coleta.models;
using api.coleta.Services;
using api.coleta.Settings;
using Microsoft.Extensions.Options;

namespace api.coleta.Repositories
{
    public class ColetaRepository : GenericRepository<Coleta>
    {
        public ColetaRepository(ApplicationDbContext context) : base(context)
        { }

        public void SalvarColetas(List<Coleta> coletas)
        {
            foreach (var coleta in coletas)
            {
                Adicionar(coleta);
            }
        }
    }
}
