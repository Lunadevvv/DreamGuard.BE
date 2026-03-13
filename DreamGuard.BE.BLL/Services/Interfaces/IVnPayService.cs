using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using Microsoft.AspNetCore.Http;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl( VnPaymentRequest model);
        VnPaymentResponse GetPaymentResult(IQueryCollection collections);
    }
}