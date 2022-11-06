using CS.Persistence.Configurations.Abstractions;
using CS.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CS.Core.ValueObjects;
using CS.Core.Enums;

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


    builder.Ignore(c => c.CsInstanceId);
    builder.Ignore(c => c.CurrentAction);
    builder.Ignore(c => c.Status);
    builder.Ignore(c => c.Target);
    builder.Ignore(m => m.EngagedTill);
    builder.Ignore(m => m.Effectors);
    builder.Ignore(m => m.Effects);


    builder.OwnsOne(player => player.Position, position => {
      position
        .Property(p => p.LocationX)
        .HasColumnName("LocationX")
        .HasDefaultValue(Position.originX);

      position
        .Property(p => p.LocationY)
        .HasColumnName("LocationY")
        .HasDefaultValue(Position.originY);

      position.Ignore(p => p.IsAtDestination);
    })
    .Navigation(c => c.Position)
    .IsRequired();

    builder.OwnsOne(player => player.Stats, stats => {

      stats.OwnsOne(cs => cs.Hp, hp => {
        hp
          .Property(h => h.Current)
          .HasDefaultValue(-1)
          .HasColumnName("HP");

        hp.Ignore(h => h.Base);
        hp.Ignore(h => h.Cap);
        hp.Ignore(h => h.RegenerationAmountPerTick);
      });

      stats.OwnsOne(cs => cs.Mp, mp => {
        mp
          .Property(m => m.Current)
          .HasDefaultValue(-1)
          .HasColumnName("MP");

        mp.Ignore(m => m.Base);
        mp.Ignore(m => m.Cap);
        mp.Ignore(m => m.RegenerationAmountPerTick);
      });

      stats.Ignore(m => m.StatsList);
      stats.Ignore(m => m.PhysicalAttack);
      stats.Ignore(m => m.PhysicalAttackSpeed);
      stats.Ignore(m => m.PhysicalDefense);
      stats.Ignore(m => m.AttackRange);
      stats.Ignore(m => m.Speed);
      stats.Ignore(m => m.CastingSpeed);

    });

    builder
      .HasMany(c => c.BarShortcuts)
      .WithOne()
      .HasForeignKey(bs => bs.CharacterId)
      .OnDelete(DeleteBehavior.Cascade)
      .IsRequired();

    builder
      .HasMany(c => c.SkillWrappers)
      .WithOne()
      .HasForeignKey(sw => sw.CharacterId)
      .OnDelete(DeleteBehavior.Cascade)
      .IsRequired();

  }

}
