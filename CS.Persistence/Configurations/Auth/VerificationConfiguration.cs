using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations.Auth;
public class VerificationConfiguration : EntityConfiguration<Verification> {
  public override string TableName => "Verifications";
  protected override string? Schema => "cs.auth";

  public override void OnConfigure(EntityTypeBuilder<Verification> builder) {

    builder.OwnsOne(c => c.Email, email => {
      email.Property<long>("VerificationId");
      email
        .Property(p => p.Value)
        .HasColumnName("Email")
        .HasMaxLength(Email.MaxLength)
        .IsRequired();
      email
        .Property(p => p.ValueLowerCase)
        .HasColumnName("Email_LowerCase")
        .HasMaxLength(Email.MaxLength)
        .IsRequired();
      email
        .HasIndex(p => p.ValueLowerCase)
        .IsUnique()
        .HasFilter("[Email] IS NOT NULL");
    })
    .Navigation(c => c.Email)
    .IsRequired();

    builder
      .Property(csu => csu.BanReason)
      .HasMaxLength(255)
      .HasDefaultValue("")
      .IsRequired();


  }

}
