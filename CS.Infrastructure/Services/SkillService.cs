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

  private List<Skill> _skills = new();
  private List<Effect> _effects = new();
  private List<Effector> _effectors = new();

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

        var skill = _skills.SingleOrDefault(s => s.SkillKeyId == sw.SkillKeyId);
        if (skill is not null) {
          sw.Skill = skill;
          sw.Skill.Effectors = _effectors.Where(e => skill.EffectorKeyIds.Any(s => s == e.EffectorKeyId));
          foreach (var effector in sw.Skill.Effectors) {
            effector.Effect = _effects.SingleOrDefault(e => e.EffectKeyId == effector.EffectKeyId);
          }
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
    var skillsPath = Path.Combine(Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location)!,
        "Files/Skill",
        $"Skills.json");

    using var skillReader = new StreamReader(skillsPath);
    _skills = JsonConvert.DeserializeObject<List<Skill>>(skillReader.ReadToEnd())
      ?? throw new NotFoundException("Skills.json could not be loaded.");

    skillReader.Close();
    skillReader.Dispose();

    _logger.LogInformation($"-----------------------------------------------------------------------");
    _logger.LogInformation($"Loaded {_skills.Count()} skills.");

    var effectorsPath = Path.Combine(Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location)!,
        "Files/Skill",
        $"Effectors.json");

    using var effectorsReader = new StreamReader(effectorsPath);
    _effectors = JsonConvert.DeserializeObject<List<Effector>>(effectorsReader.ReadToEnd())
      ?? throw new NotFoundException("Effectors.json could not be loaded.");

    effectorsReader.Close();
    effectorsReader.Dispose();

    _logger.LogInformation($"-----------------------------------------------------------------------");
    _logger.LogInformation($"Loaded {_effectors.Count()} effectors.");

    var effectsPath = Path.Combine(Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location)!,
        "Files/Skill",
        $"Effects.json");

    using var effectsReader = new StreamReader(effectsPath);
    _effects = JsonConvert.DeserializeObject<List<Effect>>(effectsReader.ReadToEnd())
      ?? throw new NotFoundException("Effects.json could not be loaded.");

    effectsReader.Close();
    effectsReader.Dispose();

    _logger.LogInformation($"-----------------------------------------------------------------------");
    _logger.LogInformation($"Loaded {_effects.Count()} effects.");
  }

}
