using Asp.Versioning;
using AutoMapper;
using CatalogService.Transversal.Classes.Dtos;
using CatalogService.Transversal.Classes.Filters;
using CatalogService.Transversal.Classes.Models;
using CatalogService.Transversal.Interfaces.BL;
using Common.ApiUtilities.Classes.Common;
using Common.Utilities.Classes.Common;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
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
        public async Task<ActionResult<ApiResponse>> GetAllProductsAsync([FromQuery] ProductFilterParams productFilter)
        {
            IEnumerable<ProductDTO> result = await _service.GetListAsync(productFilter);
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

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> GetProductByIdAsync([FromRoute] Guid id)
        {
            ProductDTO dto = await _service.GetByIdAsync(id);
            ProductModel model = _mapper.Map<ProductModel>(dto);
            return Ok(new ApiResponse
            {
                Result = model,
                Status = 200
            });
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> DeleteProductByIdAsync([FromRoute] Guid id)
        {
            ProductDTO dto = await _service.DeleteAsync(id);
            ProductModel model = _mapper.Map<ProductModel>(dto);
            return Ok(new ApiResponse
            {
                Result = model,
                Status = 200
            });
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> UpdateProductAsync([FromRoute] Guid id, [FromBody] CreateProductModel model)
        {
            ProductDTO dto = _mapper.Map<ProductDTO>(model);
            dto.Id = id;
            ProductDTO updatedDto = await _service.UpdateAsync(dto);
            ProductModel updatedModel = _mapper.Map<ProductModel>(updatedDto);
            return Ok(new ApiResponse
            {
                Result = updatedModel,
                Status = 200
            });
        }
    }
}
