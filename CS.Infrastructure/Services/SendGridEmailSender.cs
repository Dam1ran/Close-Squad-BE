using CS.Application.Models;
using CS.Application.Options;
using CS.Application.Options.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Infrastructure.Models;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CS.Infrastructure.Services;
public class SendGridEmailService : IEmailService {
  private readonly SendGridOptions _sendGridOptions;
  private readonly SendEmailOptions _sendEmailOptions;

  public SendGridEmailService(IOptions<SendEmailOptions> sendEmailOptions,
    IOptions<SendGridOptions> sendGridOptions) {
    _sendGridOptions = Check.NotNull(sendGridOptions?.Value, nameof(sendGridOptions))!;
    _sendEmailOptions = Check.NotNull(sendEmailOptions?.Value, nameof(sendEmailOptions))!;
  }
  public async Task<SendEmailResponse> SendAsync(EmailDetails emailDetails) {
    var client = new SendGridClient(_sendGridOptions.Key);

    var from = new EmailAddress(_sendEmailOptions.FromEmail, _sendEmailOptions.FromName);
    var subject = emailDetails.Subject;
    var to = new EmailAddress(emailDetails.ToEmail, emailDetails.ToName);
    var content = emailDetails.Content;

    var msg = MailHelper.CreateSingleEmail(
      from,
      to,
      subject,
      //content goes here if message type is text
      emailDetails.IsHTML ? null : content,
      //content goes here if message type is HTML
      emailDetails.IsHTML ? content : null
    );

    var response = await client.SendEmailAsync(msg);

    if (response.StatusCode == System.Net.HttpStatusCode.Accepted) {
      return new SendEmailResponse();
    }

    try {
      var bodyResult = await response.Body.ReadAsStringAsync();
      var sendGridResponse = JsonConvert.DeserializeObject<SendGridResponse>(bodyResult);
      var errorResponse = new SendEmailResponse {
        Errors = sendGridResponse?.Errors.Select(f => f.Message).ToList()!
      };

      if (errorResponse.Errors == null || errorResponse.Errors.Count == 0) {
        errorResponse.Errors = new List<string>(new[] { "Unknown error from email sending service." });
      }

      return errorResponse;

    } catch (Exception) {
      return new SendEmailResponse {
        Errors = new List<string>(new[] { "Unknown error occurred." })
      };
    }

  }

}
