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


    builder.Ignore(c => c.CharacterStatus);


    builder.OwnsOne(player => player.Position, position => {
      position
        .Property(p => p.LocationX)
        .HasColumnName("LocationX")
        .HasDefaultValue(Position.originX);

      position
        .Property(p => p.LocationY)
        .HasColumnName("LocationY")
        .HasDefaultValue(Position.originY);

    })
    .Navigation(c => c.Position)
    .IsRequired();

    builder.OwnsOne(player => player.HpStat, hpStat => {
      hpStat
        .Property(p => p.Current)
        .HasField("_currentCalculated")
        .HasDefaultValue(-1)
        .HasColumnName("HP");

      hpStat.Ignore(mps => mps.Base);
    })
    .Navigation(c => c.HpStat)
    .IsRequired();

    builder.OwnsOne(player => player.MpStat, mpStat => {
      mpStat
        .Property(p => p.Current)
        .HasField("_currentCalculated")
        .HasDefaultValue(-1)
        .HasColumnName("MP");

      mpStat.Ignore(mps => mps.Base);
    })
    .Navigation(c => c.MpStat)
    .IsRequired();

    builder.OwnsOne(player => player.SpeedStat, speedStat => {
      speedStat
        .Property(p => p.Current)
        .HasField("_currentCalculated")
        .HasDefaultValue(-1)
        .HasColumnName("Speed");

      speedStat.Ignore(mps => mps.Base);
    })
    .Navigation(c => c.SpeedStat)
    .IsRequired();


  }

}
