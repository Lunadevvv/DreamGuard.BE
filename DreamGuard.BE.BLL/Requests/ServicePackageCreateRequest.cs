using AutoMapper;
using DreamGuard.BE.DAL.Constants;
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
    public class ServicePackageCreateRequest
    {
        [Required]
        public string PackageName { get; set; } 
        [Required]
        public ServicePackageStatus? Status { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a non-negative integer.")]
        public int Duration { get; set; }
        [Required]
        public string SuitableFor { get; set; }
        [Required]
        public string Benefits { get; set; }
        [Required]
        public string ServiceContent { get; set; }  
        [Required]
        public IFormFile FormFile { get; set; }  
    }
}
