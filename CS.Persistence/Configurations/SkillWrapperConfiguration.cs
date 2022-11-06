using CS.Core.Entities;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations;
public class SkillWrapperConfiguration : EntityConfiguration<SkillWrapper> {

  public override string TableName => "SkillWrappers";

  public override void OnConfigure(EntityTypeBuilder<SkillWrapper> builder) {

    builder.Ignore(s => s.CoolDown);
    builder.Ignore(s => s.IsToggleActivated);
    builder.Ignore(s => s.Skill);

  }

}
