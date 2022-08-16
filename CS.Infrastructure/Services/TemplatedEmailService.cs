using System.Reflection;
using System.Text;
using CS.Application.Options;
using CS.Application.Options.Abstractions;
using CS.Application.Utils;
using CS.Infrastructure.Models;
using CS.Infrastructure.Services.Abstractions;

namespace CS.Infrastructure.Services;
public class TemplatedEmailService : ITemplatedEmailService {
  private readonly IEmailService _emailService;
  private readonly ExternalInfoOptions _externalInfoOptions;

  public TemplatedEmailService(IOptions<ExternalInfoOptions> externalInfoOptions, IEmailService emailService) {
    _externalInfoOptions = Check.NotNull(externalInfoOptions?.Value, nameof(externalInfoOptions))!;
    _emailService = Check.NotNull(emailService, nameof(emailService));
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
      .Replace("--WebSiteLink--", _externalInfoOptions.WebSiteLink);

    return await _emailService.SendAsync(new EmailDetails() {
      ToName = userName,
      ToEmail = email,
      Subject = "Your Email Confirmation Link",
      IsHTML = true,
      Content = templateText
    });
  }
}
