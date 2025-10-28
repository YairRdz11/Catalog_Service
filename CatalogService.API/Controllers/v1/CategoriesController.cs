using Asp.Versioning;
using AutoMapper;
using CatalogService.Transversal.Classes.Dtos;
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
    public class CategoriesController : ApiControllerBase
    {
        private readonly ICategoryService _service;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetCategoriesAsync()
        {
            IEnumerable<CategoryDTO> result = await _service.GetListAsync();
            List<CategoryModel> resultModels = _mapper.Map<List<CategoryModel>>(result);

            return Ok(new ApiResponse
            {
                Result = resultModels,
                Status = 200
            });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateCategoryAsync([FromBody] CreateCategoryModel model)
        {
            CategoryDTO dto = _mapper.Map<CategoryDTO>(model);
            CategoryDTO createdDto = await _service.CreateAsync(dto);
            CategoryModel createdModel = _mapper.Map<CategoryModel>(createdDto);
            return Ok(new ApiResponse
            {
                Result = createdModel,
                Status = 201
            });
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> GetCategoryByIdAsync([FromRoute] Guid id)
        {
            CategoryDTO dto = await _service.GetByIdAsync(id);
            CategoryModel model = _mapper.Map<CategoryModel>(dto);
            return Ok(new ApiResponse
            {
                Result = model,
                Status = 200
            });
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> DeleteCategoryAsync([FromRoute] Guid id)
        {
            CategoryDTO deletedDto = await _service.DeleteAsync(id);
            CategoryModel deletedModel = _mapper.Map<CategoryModel>(deletedDto);
            return Ok(new ApiResponse
            {
                Result = deletedModel,
                Status = 200
            });
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> UpdateCategoryAsync([FromRoute] Guid id, [FromBody] CreateCategoryModel model)
        {
            CategoryDTO dto = _mapper.Map<CategoryDTO>(model);
            dto.Id = id;
            CategoryDTO updatedDto = await _service.UpdateAsync(dto);
            CategoryModel updatedModel = _mapper.Map<CategoryModel>(updatedDto);
            return Ok(new ApiResponse
            {
                Result = updatedModel,
                Status = 200
            });
        }
    }
}
