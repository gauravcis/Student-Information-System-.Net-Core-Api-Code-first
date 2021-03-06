﻿using Microsoft.Extensions.Configuration;
using SIMS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SIMS.Controllers
{
    public class EmailSender : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {

            _configuration = configuration;
        }

        public async Task SendEmail(string email, string subject, string message)
        {
            using (var client = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = _configuration["Email:Email"],
                    Password = _configuration["Email:Password"]
                };

                client.Credentials = credential;
                client.Host = _configuration["Email:Host"];
                client.Port = int.Parse(_configuration["Email:Port"]);
                client.EnableSsl = true;

                using (var emailMessage = new MailMessage())
                {
                    emailMessage.To.Add(new MailAddress(email));
                    emailMessage.From = new MailAddress(_configuration["Email:Email"]);
                    emailMessage.Subject = subject;
                    emailMessage.Body = message;
                    client.Send(emailMessage);
                }
            }
            await Task.CompletedTask;
        }

        public int SendOtp(string Email, string Subject, string Message) {
            Random generator = new Random();
            int otp = generator.Next(10000, 99999);
            var body = Message + " :" + otp;

            try {
                SendEmail(Email, Subject, body);
                return otp;
            }
            catch (Exception e) {
                return 0;
            }


        }
    }
}
