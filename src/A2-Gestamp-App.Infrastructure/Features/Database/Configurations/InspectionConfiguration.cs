using A2GestampApp.Domain.Features.Inspection.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace A2GestampApp.Infrastructure.Features.Database.Configurations;

internal sealed class InspectionConfiguration
    : IEntityTypeConfiguration<Inspection>
{
  public void Configure(
      EntityTypeBuilder<Inspection> builder)
  {
    builder.ToTable("inspection");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .ValueGeneratedOnAdd();

    builder.Property(x => x.Date)
        .IsRequired();

    builder.Ignore(x => x.Camera1);
    builder.Ignore(x => x.Camera2);
    builder.Ignore(x => x.Camera3);

    builder.Ignore(x => x.IsCompleted);
    builder.Ignore(x => x.Approved);
    builder.Ignore(x => x.CycleTime);

    builder.Property(x => x.FirstImagePath)
    .IsRequired();

    builder.Property(x => x.SecondImagePath)
        .IsRequired();

    builder.Property(x => x.ThirdImagePath)
        .IsRequired();

    builder.Property(x => x.OriginalJudgement)
       .HasConversion<string>();

    builder.Property(x => x.FinalJudgement)
       .HasConversion<string>();

    builder.Property(x => x.OperatorName)
        .HasMaxLength(100);

    builder.Property(x => x.EmployeeNumber)
        .HasMaxLength(50);

    builder.Property(x => x.OperatorRole);

    builder.Property(x => x.ProductionShiftId);
  }
}
