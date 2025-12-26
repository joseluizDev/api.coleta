using System.Collections.Generic;
using System.Linq;
using api.cliente.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class ClienteMap
    {
        public static ClienteResponseDTO? ToResponseDto(this Cliente? cliente)
        {
            if (cliente == null)
            {
                return null;
            }

            return new ClienteResponseDTO
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                Cep = cliente.Cep,
                Endereco = cliente.Endereco,
                Cidade = cliente.Cidade,
                Estado = cliente.Estado
            };
        }

        public static Cliente? ToEntity(this ClienteRequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Cliente
            {
                Nome = dto.Nome,
                Documento = dto.Documento,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Cep = dto.Cep,
                Endereco = dto.Endereco,
                Cidade = dto.Cidade,
                Estado = dto.Estado
            };
        }

        public static List<ClienteResponseDTO> ToResponseDtoList(this IEnumerable<Cliente?>? clientes)
        {
            if (clientes == null)
            {
                return new List<ClienteResponseDTO>();
            }

            return clientes
                .Select(c => c.ToResponseDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }
    }
}
