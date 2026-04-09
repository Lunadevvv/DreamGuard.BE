using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DreamGuard.BE.DAL
{
    public static class DependencyInjection
    {
        public static void AddDALServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductCertificateRepository, ProductCertificateRepository>();
            builder.Services.AddScoped<IProductAssetRepository, ProductAssetRepository>();
            builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            builder.Services.AddScoped<IComboRepository, ComboRepository>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IOtpRepository, OtpRepository>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IBabyProfileRepository, BabyProfileRepository>();
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();
            builder.Services.AddScoped<IUserVoucherRepository, UserVoucherRepository>();
            builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IProductCustomizeTypeRepository, ProductCustomizeTypeRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IFavoriteProductRepository, FavoriteProductRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IStaffRepository, StaffRepository>();
            builder.Services.AddScoped<DreamGuardDbContextInitialiser>();
            builder.Services.AddScoped<IProductTypeRepository, ProductTypeRepository>();
            builder.Services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
            builder.Services.AddScoped<IServicePackageMappingRepository, ServicePackageMappingRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
            builder.Services.AddScoped<IStaffRepository, StaffRepository>();
            builder.Services.AddScoped<IServiceTaskRepository, ServiceTaskRepository>();
            builder.Services.AddScoped<IServiceEvidenceRepository, ServiceEvidenceRepository>();
            builder.Services.AddScoped<IServiceAssetRepository, ServiceAssetRepository>();
            builder.Services.AddScoped<IVariantCustomizeTypeRepository, VariantCustomizeTypeRepository>();
            builder.Services.AddScoped<IRatingRepository, RatingRepository>();
            builder.Services.AddScoped<IShippingTaskRepository, ShippingTaskRepository>();
            builder.Services.AddScoped<IShippingEvidenceRepository, ShippingEvidenceRepository>();
            builder.Services.AddScoped<IProductFeedbackRepository, ProductFeedbackRepository>();
            builder.Services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
        }
    }
}