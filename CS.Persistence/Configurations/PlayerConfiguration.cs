using CS.Persistence.Configurations.Abstractions;
using CS.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CS.Core.ValueObjects;

namespace CS.Persistence.Configurations;
public class PlayerConfiguration : EntityConfiguration<Player> {
  public override string TableName => "Players";
  public override void OnConfigure(EntityTypeBuilder<Player> builder) {

    builder.OwnsOne(player => player.Nickname, nickname => {
      nickname.Property<long>("playerId");
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
      .Property(p => p.ClanName)
      .HasMaxLength(20)
      .HasDefaultValue("")
      .IsRequired();

    builder
      .Property(p => p.ClanIcon)
      .HasMaxLength(20)
      .HasDefaultValue("")
      .IsRequired();

    builder.Ignore(c => c.QuadrantIndex);
    builder.Ignore(c => c.LogoutAt);
    builder.Ignore(c => c.QuadrantsUrl);

    builder
      .HasMany(p => p.Characters)
      .WithOne()
      .HasForeignKey(c => c.PlayerId)
      .OnDelete(DeleteBehavior.NoAction)
      .IsRequired();

  }

}
