using System.Reflection;
using CS.Application.Persistence.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Exceptions;
using CS.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CS.Infrastructure.Services;
public class SkillService : ISkillService {

  private readonly ILogger<SkillService> _logger;
  private readonly IServiceProvider _serviceProvider;

  private Dictionary<long, Skill> _skills = new();

  public SkillService(
    ILogger<SkillService> logger,
    IServiceProvider serviceProvider) {
    _logger = Check.NotNull(logger, nameof(logger));
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
  }

  public void AssignSkills(IEnumerable<Character> characters) {

    foreach(var character in characters) {

      foreach(var sw in character.SkillWrappers) {
        if (sw.SkillKeyId < 1) {
          continue;
        }

        if (_skills.TryGetValue(sw.SkillKeyId, out var skill)) {
          sw.Skill = skill;
        } else {
          sw.SkillKeyId = -1;
        }
      }

    }

  }

  public async Task CleanUpSkills(IEnumerable<Character> characters) {

    foreach(var character in characters) {

      var unusedSkillWrappers = character.SkillWrappers.Where(sw => sw.SkillKeyId < 1);
      if (unusedSkillWrappers is null || !unusedSkillWrappers.Any()) {
        continue;
      }


      using var scope = _serviceProvider.CreateScope();
      var _context = scope.ServiceProvider.GetRequiredService<IContext>();
      _context.SkillWrappers.RemoveRange(unusedSkillWrappers);
      character.SkillWrappers.RemoveAll(sw => sw.SkillKeyId < 1);
      await _context.SaveChangesAsync();

    }

  }

  public void Init() {
    var path = Path.Combine(Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location)!,
        "Files/Skill",
        $"Skills.json");

    using var skillReader = new StreamReader(path);
    var readSkills = JsonConvert.DeserializeObject<List<Skill>>(skillReader.ReadToEnd())
      ?? throw new NotFoundException("Skills.json could not be loaded");

    skillReader.Close();
    skillReader.Dispose();

    for (var i = 0; i < readSkills.Count; i++) {
      _skills.Add(readSkills[i].SkillKeyId, readSkills[i]);
    }

    _logger.LogInformation($"-----------------------------------------------------------------------");
    _logger.LogInformation($"Loaded {_skills.Count()} skills.");
  }

}
