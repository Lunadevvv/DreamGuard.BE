using AutoMapper;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class RatingUpdateRequest
    {
        [Range(1, 5, ErrorMessage = "Score must be between 1 and 5.")]
        public int Score { get; set; }
        [Required(ErrorMessage = "Comment is required.")]
        public string Comment { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<RatingUpdateRequest, Rating>();
            }
        }
    }
}
