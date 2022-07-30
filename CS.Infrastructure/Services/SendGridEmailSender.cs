using CS.Application.Utils;
using CS.Infrastructure.Models;
using CS.Infrastructure.Services.Abstractions;
using CS.Infrastructure.Support.Configurations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CS.Infrastructure.Services;
public class SendGridEmailService : IEmailService{
  private readonly IConfiguration _configuration;
  public SendGridEmailService(IConfiguration configuration) {
    _configuration = Check.NotNull(configuration, nameof(configuration));
  }

  public async Task<SendEmailResponse> SendAsync(EmailDetails emailDetails) {
    var apiKey = _configuration["CloseSquadSendGridKey"];
    var client = new SendGridClient(apiKey);
    SendEmailSettings sendEmailSettings = new ();
    _configuration.GetSection("SendEmailSettings").Bind(sendEmailSettings);
    var from = new EmailAddress(sendEmailSettings.FromEmail, sendEmailSettings.FromName);
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