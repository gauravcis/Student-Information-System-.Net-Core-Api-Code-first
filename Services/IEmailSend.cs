using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.Services
{
    public interface IEmailService
    {
        Task SendEmail(string email, string subject, string message);

        int SendOtp(string Email, string Subject, string Message);
    }
}
