using CS.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace CS.Infrastructure.Services.Abstractions;
public interface IUserService {
  Task<IdentityResult> CreateUser(string nickname, string email, string password);
  Task<SendEmailResponse> SendConfirmationEmail(string email);
}