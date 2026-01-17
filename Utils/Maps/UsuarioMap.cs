
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.funcionario.Models.DTOs;

namespace api.coleta.Utils.Maps
{
    public static class UsuarioMap
    {
        public static UsuarioResquestDTO? ToRequestDto(this Usuario? usuario)
        {
            if (usuario == null)
            {
                return null;
            }

            return new UsuarioResquestDTO
            {
                NomeCompleto = usuario.NomeCompleto,
                CPF = usuario.CPF,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                Senha = usuario.Senha,
                adminId = usuario.adminId ?? Guid.Empty
            };
        }

        public static UsuarioResponseDTO? ToResponseDto(this Usuario? usuario)
        {
            if (usuario == null)
            {
                return null;
            }

            return new UsuarioResponseDTO
            {
                Id = usuario.Id,
                NomeCompleto = usuario.NomeCompleto,
                CPF = usuario.CPF,
                Email = usuario.Email,
                Telefone = usuario.Telefone
            };
        }

        public static FuncionarioResponseDTO? ToFuncionarioResponseDto(this Usuario? usuario)
        {
            if (usuario == null)
            {
                return null;
            }

            return new FuncionarioResponseDTO
            {
                Id = usuario.Id,
                Nome = usuario.NomeCompleto,
                CPF = usuario.CPF,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                Observacao = usuario.Observacao,
                Ativo = usuario.Ativo
            };
        }

        public static Usuario? ToEntity(this UsuarioResquestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Usuario
            {
                NomeCompleto = dto.NomeCompleto,
                CPF = dto.CPF,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Senha = dto.Senha,
                adminId = dto.adminId == Guid.Empty ? null : dto.adminId
            };
        }

        public static UsuarioResponseDTO? ToResponseDto(this UsuarioResquestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new UsuarioResponseDTO
            {
                Id = Guid.Empty,
                NomeCompleto = dto.NomeCompleto,
                CPF = dto.CPF,
                Email = dto.Email,
                Telefone = dto.Telefone
            };
        }

        public static UsuarioResquestDTO? ToRequestDto(this UsuarioResponseDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new UsuarioResquestDTO
            {
                NomeCompleto = dto.NomeCompleto,
                CPF = dto.CPF,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Senha = string.Empty,
                adminId = Guid.Empty
            };
        }

        public static UsuarioResquestDTO? ToRequestDto(this FuncionarioRequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new UsuarioResquestDTO
            {
                NomeCompleto = dto.Nome,
                CPF = dto.CPF,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Senha = dto.Senha,
                adminId = Guid.Empty
            };
        }

        public static List<UsuarioResponseDTO> ToResponseDtoList(this IEnumerable<Usuario?>? usuarios)
        {
            if (usuarios == null)
            {
                return new List<UsuarioResponseDTO>();
            }

            return usuarios
                .Select(u => u.ToResponseDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }

        public static List<UsuarioResponseDTO?> ToNullableResponseDtoList(this IEnumerable<Usuario?>? usuarios)
        {
            if (usuarios == null)
            {
                return new List<UsuarioResponseDTO?>();
            }

            return usuarios
                .Select(u => u.ToResponseDto())
                .ToList();
        }
    }
}
