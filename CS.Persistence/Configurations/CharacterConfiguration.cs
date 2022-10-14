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

    builder.OwnsOne(player => player.CharacterStats, characterStats => {

      characterStats.OwnsOne(cs => cs.Hp, hp => {
        hp
          .Property(h => h.Current)
          .HasDefaultValue(-1)
          .HasColumnName("HP");

        hp.Ignore(h => h.Base);
        hp.Ignore(h => h.RegenerationAmountPerTick);
      });

      characterStats.OwnsOne(cs => cs.Mp, mp => {
        mp
          .Property(m => m.Current)
          .HasDefaultValue(-1)
          .HasColumnName("MP");

        mp.Ignore(m => m.Base);
        mp.Ignore(m => m.RegenerationAmountPerTick);
      });

      characterStats.OwnsOne(cs => cs.Speed, speed => {
        speed
          .Property(m => m.Current)
          .HasDefaultValue(-1)
          .HasColumnName("Speed");

        speed.Ignore(s => s.Base);
        speed.Ignore(s => s.RegenerationAmountPerTick);
      });

    });

  }

}
