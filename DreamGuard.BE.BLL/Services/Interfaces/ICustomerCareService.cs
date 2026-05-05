using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface ICustomerCareService
    {
        Task SendProductCareEmailsAsync();
    }
}
