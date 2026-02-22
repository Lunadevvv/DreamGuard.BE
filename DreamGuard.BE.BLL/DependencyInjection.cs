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
        }
    }
}
