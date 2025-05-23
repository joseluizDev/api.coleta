﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace api.coleta.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250601000000_UpdateConfiguracaoPersonalizadaLimites")]
    partial class UpdateConfiguracaoPersonalizadaLimites
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("api.coleta.Models.Entidades.ConfiguracaoPersonalizada", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("CorHex")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<DateTime>("DataInclusao")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("LimiteInferior")
                        .HasColumnType("decimal(12,4)");

                    b.Property<decimal>("LimiteSuperior")
                        .HasColumnType("decimal(12,4)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<Guid>("UsuarioId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioId");

                    b.ToTable("ConfiguracaoPersonalizadas");
                });

            modelBuilder.Entity("api.coleta.Models.Entidades.ConfiguracaoPersonalizada", b =>
                {
                    b.HasOne("api.coleta.Models.Entidades.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Usuario");
                });
        }
    }
}
