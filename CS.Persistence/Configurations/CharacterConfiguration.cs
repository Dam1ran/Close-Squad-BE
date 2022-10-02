using CS.Persistence.Configurations.Abstractions;
using CS.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CS.Core.ValueObjects;
namespace CS.Persistence.Configurations;
public class CharacterConfiguration : EntityConfiguration<Character> {
  public override string TableName => "Characters";

  public override void OnConfigure(EntityTypeBuilder<Character> builder) {
    builder.OwnsOne(player => player.Nickname, nickname => {
      nickname.Property<long>("characterId");
      nickname
        .Property(p => p.Value)
        .HasColumnName("Nickname")
        .HasMaxLength(Nickname.MaxLength)
        .IsRequired();
      nickname
        .Property(p => p.ValueLowerCase)
        .HasColumnName("Nickname_LowerCase")
        .HasMaxLength(Nickname.MaxLength)
        .IsRequired();
      nickname
        .HasIndex(p => p.ValueLowerCase)
        .IsUnique()
        .HasFilter("[Nickname] IS NOT NULL");
    })
    .Navigation(c => c.Nickname)
    .IsRequired();

    builder
      .HasOne(p => p.Quadrant)
      .WithMany()
      .HasForeignKey("quadrantId")
      .OnDelete(DeleteBehavior.NoAction)
      .IsRequired();


    builder.Ignore(c => c.IsAwake);

    builder.Property(p => p.HP).HasField("_hp");
    builder.Property(p => p.MP).HasField("_mp");

  }

}
