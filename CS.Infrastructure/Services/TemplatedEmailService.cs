using System.Reflection;
using System.Text;
using CS.Application.Utils;
using CS.Infrastructure.Models;
using CS.Infrastructure.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CS.Infrastructure.Services;
public class TemplatedEmailService : ITemplatedEmailService {
  private readonly IEmailService _emailService;
  private readonly IConfiguration _configuration;


  public TemplatedEmailService(IEmailService emailService, IConfiguration configuration) {
    _emailService = Check.NotNull(emailService, nameof(emailService));
    _configuration = Check.NotNull(configuration, nameof(configuration));
  }

  public async Task<SendEmailResponse> SendConfirmationAsync(string email, string userName, string confirmationLink) {

    var templateText = default(string);
    using (var reader =
      new StreamReader(
        Assembly.GetExecutingAssembly()?.GetManifestResourceStream("CS.Infrastructure.Support.Email.ConfirmEmailTemplate.html")!, Encoding.UTF8)) {
      templateText = await reader.ReadToEndAsync();
    }
    templateText = templateText
      .Replace("--UserName--", userName)
      .Replace("--ConfirmationButtonLink--", confirmationLink)
      .Replace("--WebSiteLink--", _configuration["WebSiteLink"]);

    return await _emailService.SendAsync(new EmailDetails() {
      ToName = userName,
      ToEmail = email,
      Subject = "Your Email Confirmation Link",
      IsHTML = true,
      Content = templateText
    });
  }
}