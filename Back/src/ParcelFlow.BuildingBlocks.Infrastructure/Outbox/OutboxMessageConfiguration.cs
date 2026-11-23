using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Tabla del outbox. Cada DbContext de módulo la aplica en su OnModelCreating, así que cada
/// módulo tiene la suya dentro de su schema (la frontera del módulo también existe en la BD).
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Type).HasMaxLength(OutboxMessage.TypeMaxLength);
        builder.Property(m => m.LastError).HasMaxLength(OutboxMessage.ErrorMaxLength);

        // El procesador busca "WHERE ProcessedOnUtc IS NULL ORDER BY OccurredOnUtc". Índice parcial:
        // solo contiene los pendientes, que son pocos (los ya procesados quedan fuera del índice).
        builder.HasIndex(m => m.OccurredOnUtc).HasFilter("\"ProcessedOnUtc\" IS NULL");
    }
}
