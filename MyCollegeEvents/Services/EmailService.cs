using System.Net;
using System.Net.Mail;
using MyCollegeEvents.Models;

namespace MyCollegeEvents.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendConfirmationEmailAsync(Participant participant)
        {
            try
            {
                var subject = "تأكيد التسجيل في الفعالية";
                var body = $@"
                    <div dir='rtl' style='font-family: Arial, sans-serif;'>
                        <h2>مرحباً {participant.Name}</h2>
                        <p>تم تسجيلك بنجاح في فعالية: <strong>{participant.Event?.Title}</strong></p>
                        <p>تاريخ الفعالية: {participant.Event?.Date:dd/MM/yyyy}</p>
                        <p>سيتم مراجعة طلبك من قبل المشرفة وستصلك رسالة تأكيد قريباً.</p>
                        <br>
                        <p>شكراً لك</p>
                        <p>فريق إدارة فعاليات الكلية</p>
                    </div>";

                return await SendEmailAsync(participant.Email, subject, body);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendApprovalEmailAsync(Participant participant)
        {
            try
            {
                var subject = "تم قبول مشاركتك في الفعالية";
                var body = $@"
                    <div dir='rtl' style='font-family: Arial, sans-serif;'>
                        <h2>مبروك {participant.Name}!</h2>
                        <p>تم قبول مشاركتك في فعالية: <strong>{participant.Event?.Title}</strong></p>
                        <p>تاريخ الفعالية: {participant.Event?.Date:dd/MM/yyyy}</p>
                        <p>نتطلع لرؤيتك في الفعالية.</p>
                        <br>
                        <p>شكراً لك</p>
                        <p>فريق إدارة فعاليات الكلية</p>
                    </div>";

                return await SendEmailAsync(participant.Email, subject, body);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendBulkEmailsAsync(List<Participant> participants, string subject, string message)
        {
            try
            {
                var successCount = 0;
                var totalCount = participants.Count;

                foreach (var participant in participants)
                {
                    try
                    {
                        var personalizedMessage = message.Replace("{Name}", participant.Name)
                                                        .Replace("{EventTitle}", participant.Event?.Title ?? "الفعالية");

                        var success = await SendEmailAsync(participant.Email, subject, personalizedMessage);
                        if (success)
                        {
                            successCount++;
                        }

                        // تأخير قصير بين الرسائل لتجنب الحظر
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"فشل في إرسال البريد إلى {participant.Email}: {ex.Message}");
                    }
                }

                Console.WriteLine($"تم إرسال {successCount} من أصل {totalCount} رسالة بنجاح");
                return successCount > 0; // نجح إذا تم إرسال رسالة واحدة على الأقل
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في الإرسال الجماعي: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // التحقق من وضع الاختبار
                var testMode = bool.Parse(_configuration["EmailSettings:TestMode"] ?? "false");
                if (testMode)
                {
                    // في وضع الاختبار، نسجل الرسالة فقط
                    Console.WriteLine("=== وضع اختبار البريد الإلكتروني ===");
                    Console.WriteLine($"المرسل إليه: {toEmail}");
                    Console.WriteLine($"الموضوع: {subject}");
                    Console.WriteLine($"الرسالة: {body.Substring(0, Math.Min(100, body.Length))}...");
                    Console.WriteLine("⚠️ تحذير: النظام في وضع الاختبار - لن يتم إرسال رسائل حقيقية!");
                    Console.WriteLine("لتفعيل الإرسال الحقيقي، غير TestMode إلى false في appsettings.json");
                    Console.WriteLine("=====================================");
                    return true;
                }

                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                // التحقق من وجود الإعدادات المطلوبة
                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword) ||
                    senderEmail == "collegeevents2025@gmail.com" || senderPassword == "your-gmail-app-password")
                {
                    throw new InvalidOperationException("إعدادات البريد الإلكتروني غير مكتملة. يرجى تحديث appsettings.json");
                }

                using var client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = 30000; // 30 ثانية

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(toEmail);

                // إضافة نسخة للمشرفة
                var adminEmail = _configuration["EmailSettings:AdminEmail"];
                if (!string.IsNullOrEmpty(adminEmail) && adminEmail != toEmail)
                {
                    mailMessage.CC.Add(adminEmail);
                }

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ للمراجعة
                Console.WriteLine($"Email sending failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
