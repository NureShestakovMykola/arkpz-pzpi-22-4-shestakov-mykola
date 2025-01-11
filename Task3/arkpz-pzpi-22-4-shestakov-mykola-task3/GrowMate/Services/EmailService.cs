using Core.Enums;
using Core.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GrowMate.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        private readonly Dictionary<EmailType, string> _body = new()
        {
            { EmailType.NewAdminTempPassword,
                @"<html>
                    <h2>Dear {0}!</h2></hr>
                    <p>You were registered as an {1} at GrowMate!</p>
                    <p>Please, use the password below to acces your account. 
                    DO NOT SHARE IT with anyone and change it as soon as possible.</p>
                    <p>Your temporary password: {2}</p>
                    </hr>
                    <i>Sincerelly, GrowMate team</i>
                    </html>"
            },
            { EmailType.PasswordReset,
                @"<html>
                    <h2>Dear {0}!</h2></hr>
                    <p>You requested a password reset for your GrowMate account!</p>
                    <p>Below is the temporrary code you would need to confirm your password reset. 
                    Keep in mind, that if your session runs out, you would need to request the password reset code again.</p>
                    <p>Your password reset code: {1}</p>
                    </hr>
                    <i>Sincerelly, GrowMate team</i>
                    </html>"
            },
        };

        private readonly Dictionary<EmailType, string> _subject = new()
        {
            { EmailType.NewAdminTempPassword, "URGENT - GrowMate {0}"},
            { EmailType.PasswordReset, "Password reset code"},
        };

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string fullName, int code, string userEmail)
        {
            var emailBody = string.Format(_body[EmailType.PasswordReset], fullName, code);
            var emailSubject = _subject[EmailType.PasswordReset];

            var email = FormEmailAsync(userEmail, emailSubject, emailBody);
            if (email == null)
            {
                return false;
            }

            return await SendEmailAsync(email);
        }

        public async Task<bool> SendNewAdminEmailAsync(string fullName, string tempPassword, UserRole role, string userEmail)
        {
            var userRole = role == UserRole.Admin ? "Admin" : "Database Admin";
            var emailBody = string.Format(_body[EmailType.NewAdminTempPassword], fullName, userRole, tempPassword);
            var emailSubject = string.Format(_subject[EmailType.PasswordReset], userRole);

            var email = FormEmailAsync(userEmail, emailSubject, emailBody);
            if (email == null)
            {
                return false;
            }

            return await SendEmailAsync(email);
        }

        private MimeMessage FormEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("Forming email");

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogError("Failed to form email - no destination was given");

                return null;
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                _logger.LogError("Failed to form email - no subject was given");

                return null;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogError("Failed to form email - no body was given");

                return null;
            }

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };

            _logger.LogInformation("Email formed successfully");

            return message;
        }

        private async Task<bool> SendEmailAsync(MimeMessage message)
        {
            _logger.LogInformation("Sending email: {subject}", message.Subject);

            var smtp = new SmtpClient();

            try
            {
                if (_settings.UseStartTls)
                {
                    await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                }
                else if (_settings.UseSSL)
                {
                    await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect);
                }


                await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
                await smtp.SendAsync(message);

                _logger.LogInformation("Email sent successfully");

                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send email! Error: {error}", ex.Message);

                return false;
            }
            finally
            {
                smtp.Dispose();
            }
        }
    }
}
