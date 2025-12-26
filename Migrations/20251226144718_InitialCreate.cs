using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConfiguracaoPadraos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Limite = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    CorHex = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracaoPadraos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Contatos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NomeCompleto = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroTelefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailUsuarioEnviado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EmailAdminsEnviado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataEnvioEmail = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contatos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Geojson",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Pontos = table.Column<string>(type: "JSON", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Grid = table.Column<string>(type: "JSON", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Geojson", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MColetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Lat = table.Column<double>(type: "double", nullable: false),
                    Lng = table.Column<double>(type: "double", nullable: false),
                    Reference = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Place_Id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MColetas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Minerais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Minerais", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NutrientConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NutrientName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfigData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsGlobal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutrientConfigs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PontoColetados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PontoID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ColetaID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FuncionarioID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    HexagonID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    DataColeta = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PontoColetados", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NomeCompleto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CPF = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Senha = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FcmToken = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    adminId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Usuarios_adminId",
                        column: x => x.adminId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Documento = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Cep = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConfiguracaoPersonalizadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LimiteInferior = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    LimiteSuperior = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    CorHex = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracaoPersonalizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracaoPersonalizadas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Imagens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObjectName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BucketName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomeOriginal = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imagens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MensagensAgendadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mensagem = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataHoraEnvio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataHoraEnviada = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    FuncionarioId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    MensagemErro = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TentativasEnvio = table.Column<int>(type: "int", nullable: false),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensAgendadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensagensAgendadas_Usuarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MensagensAgendadas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fazendas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Lat = table.Column<double>(type: "double", maxLength: 12, nullable: false),
                    Lng = table.Column<double>(type: "double", maxLength: 12, nullable: false),
                    ClienteID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fazendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fazendas_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fazendas_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Safras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Observacao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FazendaID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Safras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Safras_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Safras_Fazendas_FazendaID",
                        column: x => x.FazendaID,
                        principalTable: "Fazendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Safras_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Talhoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FazendaID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Talhoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Talhoes_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Talhoes_Fazendas_FazendaID",
                        column: x => x.FazendaID,
                        principalTable: "Fazendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Talhoes_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VinculoClienteFazendas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FazendaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VinculoClienteFazendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VinculoClienteFazendas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VinculoClienteFazendas_Fazendas_FazendaId",
                        column: x => x.FazendaId,
                        principalTable: "Fazendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImagensNdvi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LinkImagem = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataImagem = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TipoImagem = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PercentualNuvens = table.Column<double>(type: "double", nullable: true),
                    NdviMax = table.Column<double>(type: "double", nullable: true),
                    NdviMin = table.Column<double>(type: "double", nullable: true),
                    AltimetriaMin = table.Column<double>(type: "double", nullable: true),
                    AltimetriaMax = table.Column<double>(type: "double", nullable: true),
                    AltimetriaVariacao = table.Column<double>(type: "double", nullable: true),
                    DataImagemColheita = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ColheitaMin = table.Column<double>(type: "double", nullable: true),
                    ColheitaMax = table.Column<double>(type: "double", nullable: true),
                    ColheitaMedia = table.Column<double>(type: "double", nullable: true),
                    Legenda = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TalhaoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FazendaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagensNdvi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagensNdvi_Fazendas_FazendaId",
                        column: x => x.FazendaId,
                        principalTable: "Fazendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImagensNdvi_Talhoes_TalhaoId",
                        column: x => x.TalhaoId,
                        principalTable: "Talhoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImagensNdvi_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TalhaoJson",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TalhaoID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Area = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Coordenadas = table.Column<string>(type: "JSON", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nome = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalhaoJson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalhaoJson_Talhoes_TalhaoID",
                        column: x => x.TalhaoID,
                        principalTable: "Talhoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Coletas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TalhaoID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GeojsonID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioRespID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Observacao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomeColeta = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoColeta = table.Column<int>(type: "int", nullable: false),
                    TipoAnalise = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Profundidade = table.Column<int>(type: "int", nullable: false),
                    UsuarioID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SafraID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    FazendaID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coletas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coletas_Fazendas_FazendaID",
                        column: x => x.FazendaID,
                        principalTable: "Fazendas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Coletas_Geojson_GeojsonID",
                        column: x => x.GeojsonID,
                        principalTable: "Geojson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coletas_Safras_SafraID",
                        column: x => x.SafraID,
                        principalTable: "Safras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Coletas_TalhaoJson_TalhaoID",
                        column: x => x.TalhaoID,
                        principalTable: "TalhaoJson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coletas_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coletas_Usuarios_UsuarioRespID",
                        column: x => x.UsuarioRespID,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Relatorios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LinkBackup = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JsonRelatorio = table.Column<string>(type: "JSON", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ColetaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relatorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relatorios_Coletas_ColetaId",
                        column: x => x.ColetaId,
                        principalTable: "Coletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Relatorios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Recomendacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RelatorioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ColetaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NomeColuna = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnidadeMedida = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recomendacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recomendacoes_Coletas_ColetaId",
                        column: x => x.ColetaId,
                        principalTable: "Coletas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Recomendacoes_Relatorios_RelatorioId",
                        column: x => x.RelatorioId,
                        principalTable: "Relatorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioID",
                table: "Clientes",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Coletas_FazendaID",
                table: "Coletas",
                column: "FazendaID");

            migrationBuilder.CreateIndex(
                name: "IX_Coletas_GeojsonID",
                table: "Coletas",
                column: "GeojsonID");

            migrationBuilder.CreateIndex(
                name: "IX_Coletas_SafraID",
                table: "Coletas",
                column: "SafraID");

            migrationBuilder.CreateIndex(
                name: "IX_Coletas_TalhaoID",
                table: "Coletas",
                column: "TalhaoID");

            migrationBuilder.CreateIndex(
                name: "IX_Coletas_UsuarioID",
                table: "Coletas",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Coletas_UsuarioRespID",
                table: "Coletas",
                column: "UsuarioRespID");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracaoPersonalizadas_UsuarioId",
                table: "ConfiguracaoPersonalizadas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Fazendas_ClienteID",
                table: "Fazendas",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Fazendas_UsuarioID",
                table: "Fazendas",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Imagens_UsuarioId",
                table: "Imagens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensNdvi_FazendaId",
                table: "ImagensNdvi",
                column: "FazendaId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensNdvi_TalhaoId",
                table: "ImagensNdvi",
                column: "TalhaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensNdvi_UsuarioId",
                table: "ImagensNdvi",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensAgendadas_FuncionarioId",
                table: "MensagensAgendadas",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensAgendadas_UsuarioId",
                table: "MensagensAgendadas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Recomendacoes_ColetaId",
                table: "Recomendacoes",
                column: "ColetaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recomendacoes_RelatorioId",
                table: "Recomendacoes",
                column: "RelatorioId");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_ColetaId",
                table: "Relatorios",
                column: "ColetaId");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_UsuarioId",
                table: "Relatorios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Safras_ClienteID",
                table: "Safras",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Safras_FazendaID",
                table: "Safras",
                column: "FazendaID");

            migrationBuilder.CreateIndex(
                name: "IX_Safras_UsuarioID",
                table: "Safras",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_TalhaoJson_TalhaoID",
                table: "TalhaoJson",
                column: "TalhaoID");

            migrationBuilder.CreateIndex(
                name: "IX_Talhoes_ClienteID",
                table: "Talhoes",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Talhoes_FazendaID",
                table: "Talhoes",
                column: "FazendaID");

            migrationBuilder.CreateIndex(
                name: "IX_Talhoes_UsuarioID",
                table: "Talhoes",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_adminId",
                table: "Usuarios",
                column: "adminId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculoClienteFazendas_ClienteId",
                table: "VinculoClienteFazendas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculoClienteFazendas_FazendaId",
                table: "VinculoClienteFazendas",
                column: "FazendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracaoPadraos");

            migrationBuilder.DropTable(
                name: "ConfiguracaoPersonalizadas");

            migrationBuilder.DropTable(
                name: "Contatos");

            migrationBuilder.DropTable(
                name: "Imagens");

            migrationBuilder.DropTable(
                name: "ImagensNdvi");

            migrationBuilder.DropTable(
                name: "MColetas");

            migrationBuilder.DropTable(
                name: "MensagensAgendadas");

            migrationBuilder.DropTable(
                name: "Minerais");

            migrationBuilder.DropTable(
                name: "NutrientConfigs");

            migrationBuilder.DropTable(
                name: "PontoColetados");

            migrationBuilder.DropTable(
                name: "Recomendacoes");

            migrationBuilder.DropTable(
                name: "VinculoClienteFazendas");

            migrationBuilder.DropTable(
                name: "Relatorios");

            migrationBuilder.DropTable(
                name: "Coletas");

            migrationBuilder.DropTable(
                name: "Geojson");

            migrationBuilder.DropTable(
                name: "Safras");

            migrationBuilder.DropTable(
                name: "TalhaoJson");

            migrationBuilder.DropTable(
                name: "Talhoes");

            migrationBuilder.DropTable(
                name: "Fazendas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
