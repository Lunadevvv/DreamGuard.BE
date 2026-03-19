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
    public class ProductTypeCreateRequest
    {
        [Required]
        public string ProductTypeName { get; set; }

        [Required]
        public bool? IsActive { get; set; }
    }
}
