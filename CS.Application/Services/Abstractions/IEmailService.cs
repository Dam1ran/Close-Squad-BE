using CS.Application.Models;

namespace CS.Application.Services.Abstractions;
public interface IEmailService {

  Task<SendEmailResponse> SendAsync(EmailDetails emailDetails);

}
