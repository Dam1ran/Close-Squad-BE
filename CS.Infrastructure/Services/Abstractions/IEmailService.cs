using CS.Infrastructure.Models;

namespace CS.Infrastructure.Services.Abstractions;
public interface IEmailService {
  Task<SendEmailResponse> SendAsync(EmailDetails emailDetails);
}
