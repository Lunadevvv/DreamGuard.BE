using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/variants")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IMapper _mapper;
        public ProductVariantController(IProductVariantRepository productVariantRepository, IMapper mapper)
        {
            _productVariantRepository = productVariantRepository;
            _mapper = mapper;
        }


    }
}