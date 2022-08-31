using CS.Core.Entities;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations;
public class AnnouncementConfiguration : EntityConfiguration<ServerAnnouncement> {
  public override string TableName => "Announcements";
  public override void OnConfigure(EntityTypeBuilder<ServerAnnouncement> builder) {
    builder
      .Property(csu => csu.Message)
      .HasMaxLength(1024)
      .IsRequired();
  }
}
