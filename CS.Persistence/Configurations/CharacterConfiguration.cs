using CS.Core.Entities;
using CS.Core.ValueObjects;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations;
public class CharacterConfiguration : EntityConfiguration<Character> {
  public override string TableName => "Characters";

  public override void OnConfigure(EntityTypeBuilder<Character> builder) {
  builder
    .OwnsOne(b => b.Name, name => {
      name
        .Property<long>("CharacterId");
        // .UseHiLo(HiLoSequenceName);

      name
        .Property(c => c.Value)
        .HasColumnName("Name")
        .HasMaxLength(Name.MaxLength)
        .IsRequired();
    })
    .Navigation(b => b.Name)
    .IsRequired();
  }
}
