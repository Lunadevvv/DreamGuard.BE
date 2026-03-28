using AutoMapper;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Responses
{
    public class RatingResponse
    {
        public Guid RatingId { get; set; }
        public Guid StaffId { get; set; }
        public Guid ServiceOrderId { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Rating, RatingResponse>();
            }
        }
    }
}
