using CS.Application.Enums;

namespace CS.Api.Communications.Models;
public class CharacterTravelCall {
  public string CharacterNickname { get; set; } = string.Empty;
  public TravelDirection TravelDirection { get; set; }

}
