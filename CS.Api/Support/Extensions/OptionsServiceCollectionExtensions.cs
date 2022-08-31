using CS.Api.Support.Other;
using CS.Application.Options;
using CS.Application.Options.Abstractions;

namespace CS.Api.Support.Extensions;
public static class OptionsServiceCollectionExtensions {
  public static IServiceCollection AddApplicationOptions(this IServiceCollection services) =>
    services
      .AddOptions<CaptchaOptions>()
      .BindConfiguration(CaptchaOptions.Captcha)
      .ValidateDataAnnotations()
      .Services
      .AddOptions<JwtOptions>()
      .BindConfiguration(JwtOptions.Jwt)
      .ValidateDataAnnotations()
      .Services
      .AddOptions<SendGridOptions>()
      .BindConfiguration(SendGridOptions.SendGrid)
      .ValidateDataAnnotations()
      .Services
      .AddOptions<SendEmailOptions>()
      .BindConfiguration(SendEmailOptions.SendEmail)
      .ValidateDataAnnotations()
      .Services
      .AddOptions<ExternalInfoOptions>()
      .BindConfiguration(ExternalInfoOptions.ExternalInfo)
      .ValidateDataAnnotations()
      .Services
      .AddOptions<RefreshTokenOptions>()
      .BindConfiguration(RefreshTokenOptions.RefreshToken)
      .ValidateDataAnnotations()
      .Services.AddSingleton(typeof(IOptions<>), typeof(OptionsAdapter<>));

}
