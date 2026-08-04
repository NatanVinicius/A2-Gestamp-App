using A2GestampApp.Domain.Features.ProductionShift.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace A2GestampApp.Infrastructure.Features.Database.Configurations;

internal sealed class ProductionShiftConfiguration
    : IEntityTypeConfiguration<ProductionShift>
{
  public void Configure(
      EntityTypeBuilder<ProductionShift> builder)
  {
    builder.ToTable("production_shift");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .ValueGeneratedOnAdd();

    builder.Property(x => x.ShiftNumber)
        .IsRequired();

    builder.Property(x => x.StartDate)
        .IsRequired();

    builder.Property(x => x.EndDate)
        .IsRequired();

    builder.Property(x => x.Produced)
        .IsRequired();

    builder.Property(x => x.Approved)
        .IsRequired();

    builder.Property(x => x.Reproved)
        .IsRequired();

    builder.Property(x => x.CreatedAt)
        .IsRequired();

    builder.Property(x => x.IsClosed)
        .IsRequired();

    builder.Ignore(x => x.RejectionRate);
  }
}
