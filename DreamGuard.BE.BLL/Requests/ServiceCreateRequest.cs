using AutoMapper;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Requests
{
    public class ServiceCreateRequest
    {
        [Required]
        public string ServiceName { get; set; }
        [Required]
        public string Description { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
        public decimal Price { get; set; }
        [Required]
        public bool? IsActive { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "EstimatedDuration must be a non-negative integer.")]
        public int EstimatedDuration { get; set; }
        [MinLength(1, ErrorMessage = "At least one file is required.")]
        public ICollection<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
