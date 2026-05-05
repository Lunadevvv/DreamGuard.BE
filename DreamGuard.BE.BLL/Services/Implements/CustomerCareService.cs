using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class CustomerCareService : ICustomerCareService
    {
        private readonly DreamGuardContext _context;
        private readonly IBrevoEmailService _emailService;
        private readonly ILogger<CustomerCareService> _logger;

        public CustomerCareService(
            DreamGuardContext context,
            IBrevoEmailService emailService,
            ILogger<CustomerCareService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendProductCareEmailsAsync()
        {
            try
            {
                var targetDate = DateTime.UtcNow.AddDays(-60).Date;

                // Query Orders
                var pendingOrders = await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Where(o => o.Status == OrderStatus.Completed 
                             && !o.IsCareEmailSent 
                             && o.UpdatedAt.Date <= targetDate)
                    .ToListAsync();

                // Query ServiceOrders
                var pendingServiceOrders = await _context.ServiceOrders
                    .Include(so => so.Customer)
                        .ThenInclude(c => c.User)
                    .Where(so => so.Status == OrderServiceStatus.Completed 
                              && !so.IsCareEmailSent 
                              && so.UpdatedAt != null 
                              && so.UpdatedAt.Value.Date <= targetDate)
                    .ToListAsync();

                var customerEmailsToNotify = new HashSet<string>();
                var htmlContent = GetCareEmailTemplate();

                // Process Orders
                foreach (var order in pendingOrders)
                {
                    var email = order.Customer?.User?.Email;
                    if (!string.IsNullOrEmpty(email) && customerEmailsToNotify.Add(email))
                    {
                        var result = await _emailService.SendEmailAsync(email, "Product Care Service Proposal - DreamGuard", htmlContent);
                        if (result.Succeeded)
                        {
                            order.IsCareEmailSent = true;
                            _context.Orders.Update(order);
                        }
                    }
                    else if (customerEmailsToNotify.Contains(email))
                    {
                        // Prevent sending duplicate emails but still mark as sent so it won't be picked up again
                        order.IsCareEmailSent = true;
                        _context.Orders.Update(order);
                    }
                }

                // Process ServiceOrders
                foreach (var so in pendingServiceOrders)
                {
                    var email = so.Customer?.User?.Email;
                    if (!string.IsNullOrEmpty(email) && customerEmailsToNotify.Add(email))
                    {
                        var result = await _emailService.SendEmailAsync(email, "Product Care Service Proposal - DreamGuard", htmlContent);
                        if (result.Succeeded)
                        {
                            so.IsCareEmailSent = true;
                            _context.ServiceOrders.Update(so);
                        }
                    }
                    else if (customerEmailsToNotify.Contains(email))
                    {
                        so.IsCareEmailSent = true;
                        _context.ServiceOrders.Update(so);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully sent product care emails to {customerEmailsToNotify.Count} customers.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending product care emails.");
            }
        }

        private string GetCareEmailTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>DreamGuard Product Care</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;'>
    <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);'>
        <h2 style='color: #2c3e50; text-align: center;'>Protect Your Health - Product Care with DreamGuard</h2>
        <p style='color: #34495e; font-size: 16px; line-height: 1.6;'>
            Dear Valued Customer,
        </p>
        <p style='color: #34495e; font-size: 16px; line-height: 1.6;'>
            It has been 60 days since your last purchase or cleaning service with <strong>DreamGuard</strong>. We hope you are having wonderful experiences with our products.
        </p>
        <p style='color: #34495e; font-size: 16px; line-height: 1.6;'>
            According to health experts, regular cleaning of bedding and mattresses every 2-3 months is extremely important to remove dirt, bacteria, and allergens. This protects your respiratory system and brings a better night's sleep to your family.
        </p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='https://dream-guard.vercel.app/services' style='background-color: #3498db; color: #ffffff; padding: 12px 25px; text-decoration: none; font-size: 18px; border-radius: 5px; font-weight: bold; display: inline-block;'>
                Explore Cleaning Services
            </a>
        </div>
        <p style='color: #34495e; font-size: 16px; line-height: 1.6;'>
            Let DreamGuard help you care for your living space comprehensively. Our professional team is always ready to serve you.
        </p>
        <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
        <p style='color: #7f8c8d; font-size: 14px; text-align: center;'>
            Best regards,<br />
            <strong>The DreamGuard Team</strong>
        </p>
    </div>
</body>
</html>";
        }
    }
}
