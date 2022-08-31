using CS.Application.Models;

namespace CS.Application.Services.Abstractions;
public interface ITemplatedEmailService {

  Task<SendEmailResponse> SendConfirmationAsync(string email, string userName, string confirmationLink);

}
