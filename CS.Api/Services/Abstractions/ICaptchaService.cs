using CS.Application.Services.Abstractions;

public interface ICaptchaService {
  string GetCode();
  ICsFile GetImage(string code);
}
