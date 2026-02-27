using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<Result<UserProfileResponse>> GetByIdAsync(Guid userId);
        Task<Result> UpdateAsync(Guid userId, UserProfileUpdateRequest userProfileUpdateRequest);
        Task<Result> ChangePhoneNumberAsync(Guid userId, ChangePhoneNumberRequest changePhoneNumberRequest);
        Task<Result> ChangePhoneNumberRequestAsync(Guid userId);
    }
}
