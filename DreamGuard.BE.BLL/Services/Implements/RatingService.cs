using AutoMapper;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class RatingService : IRatingService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRatingRepository _ratingRepository;
        private readonly IServiceOrderRepository _serviceOrderRepository;
        public RatingService(IRatingRepository ratingRepository, IServiceOrderRepository serviceOrderRepository, IMapper mapper, IUnitOfWork unitOfWork, IStaffRepository staffRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _ratingRepository = ratingRepository;
            _serviceOrderRepository = serviceOrderRepository;
            _staffRepository = staffRepository;
        }
        
        public async Task<Result> CreateRatingAsync(Guid serviceOrderId, Guid customerId, RatingCreateRequest request)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithRating(serviceOrderId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }
            if (serviceOrder.Rating != null)
            {
                return Result.Failure("Rating already exists for this service order", 400);
            }
            // check if the service order belongs to the customer
            if(serviceOrder.CustomerId != customerId)
            {
                return Result.Failure("You can only rate your own service orders", 403);
            }
            // check if service order is completed
            if (serviceOrder.Status != OrderServiceStatus.Completed)
            {
                return Result.Failure("Cannot rate a service order that is not completed", 400);
            }

            Rating rating = new Rating
            {
                ServiceOrderId = serviceOrderId,
                Score = request.Score,
                Comment = request.Comment,
                StaffId = serviceOrder.ServiceTask!.StaffId,
            };
            var staff = serviceOrder.ServiceTask!.Staff;
            staff.TotalRating += 1;
            staff.AverageRating = ((staff.AverageRating * (staff.TotalRating - 1)) + request.Score) / staff.TotalRating;
            _staffRepository.UpdateEntity(staff);
             _ratingRepository.AddEntity(rating);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result == 0)
            {
                return Result.Failure("Nothing created", 400);
            }
            return Result.Success($"{rating.RatingId}");
        }

        public async Task<Result<PaginatedList<RatingResponse>>> GetRatingsByAdminSearchdAsync(Guid? serviceOrderId, Guid? staffId, int? score, DateTime? createdAt, DateTime? updatedAt, int pageNumber, int pageSize)
        {
            var ratings = await _ratingRepository.GetRatingsByAdminSearchdAsync(serviceOrderId, staffId, score, createdAt, updatedAt, pageNumber, pageSize);
            var ratingResponses = _mapper.Map<List<RatingResponse>>(ratings.Items);
            var paginatedResult = new PaginatedList<RatingResponse>(ratingResponses, ratings.TotalCount, ratings.PageNumber, ratings.PageSize);
            return Result<PaginatedList<RatingResponse>>.Success(paginatedResult);
        }



        public async Task<Result> UpdateRatingAsync(Guid ratingId, Guid customerId, RatingUpdateRequest request)
        {
            var ratingUpdate = await _ratingRepository.GetRatingByIdAsync(ratingId);
            if (ratingUpdate == null)
            {
                return Result.Failure("rating not found", 404);
            }
            // check if the service order belongs to the customer
            if (ratingUpdate.ServiceOrder.CustomerId != customerId)
            {
                return Result.Failure("You can only rate your own service orders", 403);
            }
            _mapper.Map(request, ratingUpdate);
            var staff = ratingUpdate.Staff;
            // update average rating
            staff.AverageRating = ((staff.AverageRating * staff.TotalRating) - ratingUpdate.Score + request.Score) / staff.TotalRating;
            ratingUpdate.UpdatedAt = DateTime.UtcNow;
            _staffRepository.UpdateEntity(staff);
            _ratingRepository.UpdateEntity(ratingUpdate);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result == 0)
            {
                return Result.Failure("Nothing updated", 400);
            }
            return Result.Success($"{result}");
        }

        public async Task<Result<RatingResponse>> GetRatingByIdAsync(Guid ratingId)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null)
            {
                return Result<RatingResponse>.Failure("Rating not found", 404);
            }
            var ratingResponse = _mapper.Map<RatingResponse>(rating);
            return Result<RatingResponse>.Success(ratingResponse);
        }

        public async Task<Result<PaginatedList<RatingResponse>>> GetRatingsByStaffId(Guid staffId, int pageNumber, int pageSize)
        {
            var ratings = await _ratingRepository.GetRatingsByStaffIdAsync(staffId, pageNumber, pageSize);
            var ratingResponses = _mapper.Map<List<RatingResponse>>(ratings.Items);
            var paginatedResult = new PaginatedList<RatingResponse>(ratingResponses, ratings.TotalCount, ratings.PageNumber, ratings.PageSize);
            return Result<PaginatedList<RatingResponse>>.Success(paginatedResult);
        }
    }
}