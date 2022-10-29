using CS.Core.Entities;

namespace CS.Core.Services.Interfaces;
public interface ISkillService {
  void Init();
  void AssignSkills(IEnumerable<Character> characters);
  Task CleanUpSkills(IEnumerable<Character> characters);

}