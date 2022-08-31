using CS.Application.Models.Abstractions;

public interface ICaptchaService {

  string GetCode();
  ICsFile GetImage(string code);

}
