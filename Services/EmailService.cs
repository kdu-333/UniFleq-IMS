using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace UnifleqSolutions_IMS.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendLowStockAlertAsync(string productName, string category, int currentQty, int reorderLevel)
        {
            var settings = _config.GetSection("EmailSettings");
            var smtpHost = settings["SmtpHost"];
            var smtpPort = int.Parse(settings["SmtpPort"]!);
            var senderEmail = settings["SenderEmail"];
            var senderName = settings["SenderName"];
            var appPassword = settings["AppPassword"];
            var notifyEmail = settings["NotifyEmail"];

            string status = currentQty == 0 ? "OUT OF STOCK" : "LOW STOCK";
            string color = currentQty == 0 ? "#ef4444" : "#f59e0b";

            string htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <body style='margin:0;padding:0;background:#f5f5f5;font-family:Arial,sans-serif'>
                <div style='max-width:600px;margin:40px auto;background:#ffffff;border:1px solid #e0e0e0;border-radius:12px;overflow:hidden'>

                    <!-- Header -->
                    <div style='background:#d62a24;padding:24px 32px'>
                        <div style='font-size:22px;font-weight:800;color:#ffffff;letter-spacing:-0.5px'>
                            Uni<span style='color:#ffcccc'>Fleq</span>
                        </div>
                        <div style='font-size:11px;color:rgba(255,255,255,0.7);margin-top:2px;text-transform:uppercase;letter-spacing:1px'>
                            Inventory Management System
                        </div>
                    </div>

                    <!-- Body -->
                    <div style='padding:32px'>
                        <div style='display:inline-block;background:{color}22;color:{color};padding:6px 14px;border-radius:20px;font-size:12px;font-weight:600;letter-spacing:0.5px;margin-bottom:16px'>
                            ⚠️ {status}
                        </div>

                        <h2 style='font-size:20px;font-weight:700;color:#1a1a1a;margin:0 0 8px'>
                            Stock Alert: {productName}
                        </h2>
                        <p style='color:#555555;font-size:14px;margin:0 0 24px'>
                            A product in your inventory has reached a critical stock level and requires immediate attention.
                        </p>

                        <!-- Details Table -->
                        <div style='background:#f5f5f5;border-radius:8px;padding:20px;margin-bottom:24px'>
                            <table style='width:100%;border-collapse:collapse'>
                                <tr>
                                    <td style='padding:8px 0;color:#888888;font-size:13px;width:140px'>Product</td>
                                    <td style='padding:8px 0;font-weight:600;color:#1a1a1a;font-size:13px'>{productName}</td>
                                </tr>
                                <tr style='border-top:1px solid #e0e0e0'>
                                    <td style='padding:8px 0;color:#888888;font-size:13px'>Category</td>
                                    <td style='padding:8px 0;color:#555555;font-size:13px'>{category}</td>
                                </tr>
                                <tr style='border-top:1px solid #e0e0e0'>
                                    <td style='padding:8px 0;color:#888888;font-size:13px'>Current Stock</td>
                                    <td style='padding:8px 0;font-weight:700;color:{color};font-size:13px'>{currentQty} units</td>
                                </tr>
                                <tr style='border-top:1px solid #e0e0e0'>
                                    <td style='padding:8px 0;color:#888888;font-size:13px'>Reorder Level</td>
                                    <td style='padding:8px 0;color:#555555;font-size:13px'>{reorderLevel} units</td>
                                </tr>
                                <tr style='border-top:1px solid #e0e0e0'>
                                    <td style='padding:8px 0;color:#888888;font-size:13px'>Status</td>
                                    <td style='padding:8px 0;font-weight:600;color:{color};font-size:13px'>{status}</td>
                                </tr>
                            </table>
                        </div>

                        <p style='color:#888888;font-size:12px;margin:0;border-top:1px solid #e0e0e0;padding-top:16px'>
                            This is an automated alert from UniFleq IMS. Please log in at
                            <a href='http://unifleq.runasp.net' style='color:#d62a24'>unifleq.runasp.net</a>
                            to process a restock.
                        </p>
                    </div>
                </div>
            </body>
            </html>";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("UniFleq Admin", notifyEmail));
            message.Subject = $"⚠️ {status} Alert — {productName}";

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(senderEmail, appPassword);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}