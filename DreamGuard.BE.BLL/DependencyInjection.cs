using DreamGuard.BE.BLL.Services.Implements;
using DreamGuard.BE.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL
{
    public static class DependencyInjection
    {
        public static void AddBLLServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
            builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
            builder.Services.AddScoped<IComboService, ComboService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();

            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IBrevoEmailService, BrevoEmailService>();
            builder.Services.AddScoped<IBabyProfileService, BabyProfileService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IVoucherService, VoucherService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
        }
    }
}
