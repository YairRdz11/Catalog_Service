using ApiUtilities.Classes.Common;
using AutoMapper;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Models;
using CatalogService.Transversal.Interfaces.BL;
using Microsoft.AspNetCore.Mvc;
using Utilities.Classes.Common;

namespace CatalogService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ApiControllerBase
    {
        private readonly IProductService _service;
        private readonly IMapper _mapper;

        public ProductsController(IProductService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetAllProductsAsync()
        {
            IEnumerable<ProductDTO> result = await _service.GetListAsync();
            List<ProductModel> resultModels = _mapper.Map<List<ProductModel>>(result);

            return Ok(new ApiResponse
            {
                Result = resultModels,
                Status = 200
            });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateProductAsync([FromBody] CreateProductModel model)
        {
            ProductDTO dto = _mapper.Map<ProductDTO>(model);
            ProductDTO createdDto = await _service.CreateAsync(dto);
            ProductModel createdModel = _mapper.Map<ProductModel>(createdDto);
            return Ok(new ApiResponse
            {
                Result = createdModel,
                Status = 201
            });
        }
    }
}
