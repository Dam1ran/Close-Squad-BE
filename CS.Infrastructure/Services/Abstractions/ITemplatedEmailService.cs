using CS.Infrastructure.Models;

namespace CS.Infrastructure.Services.Abstractions;
public interface ITemplatedEmailService {
  Task<SendEmailResponse> SendConfirmationAsync(string email, string userName, string confirmationLink);
}