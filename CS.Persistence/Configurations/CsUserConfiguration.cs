using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations;
public class CsUserConfiguration : EntityConfiguration<CsUser> {
  public override string TableName => "CsUsers";
  protected override string? Schema => "cs.auth";

  public override void OnConfigure(EntityTypeBuilder<CsUser> builder) {

    builder.OwnsOne(csu => csu.Nickname, nickname => {
      nickname.Property<long>("csUserId");
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
      .HasOne(csu => csu.Verification)
      .WithOne()
      .HasForeignKey<Verification>("csUserId")
      .IsRequired();

    builder
      .HasOne(csu => csu.Identification)
      .WithOne()
      .HasForeignKey<Identification>("csUserId")
      .IsRequired();

  }

}
