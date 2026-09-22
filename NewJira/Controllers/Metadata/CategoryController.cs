using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewJira.Controllers.Metadata
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        
        [Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCategory()
        {
            var categories = await _categoryRepository.GetAllProjectCategoriesAsync();
            return Ok(new { statusCode = 200, message = "Lấy danh sách danh mục thành công", content = categories });
        }


        [Authorize(Policy = "user.manage")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.CategoryName))
            {
                return BadRequest(new { statusCode = 400, message = "Tên danh mục không được để trống!" });
            }

            var newCategory = new Category
            {
                CategoryName = model.CategoryName,
            };

            await _categoryRepository.AddProjectCategoryAsync(newCategory);
            return Ok(new { statusCode = 200, message = "Tạo danh mục thành công", content = newCategory });
        }

        [Authorize(Policy = "user.manage")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.CategoryName))
            {
                return BadRequest(new { statusCode = 400, message = "Tên danh mục cập nhật không được để trống!" });
            }

            var existingCategory = await _categoryRepository.GetProjectCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy danh mục để cập nhật!" });
            }

            existingCategory.CategoryName = model.CategoryName;

            await _categoryRepository.UpdateProjectCategoryAsync(existingCategory); // Đã sửa đúng repository method
            return Ok(new { statusCode = 200, message = "Cập nhật danh mục thành công", content = existingCategory });
        }

        [Authorize(Policy = "user.manage")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var existingCategory = await _categoryRepository.GetProjectCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy danh mục để xóa!" });
            }

            await _categoryRepository.DeleteProjectCategoryAsync(id); // Đã sửa đúng repository method
            return Ok(new { statusCode = 200, message = "Xóa danh mục thành công" });
        }

        // --- DTOs NỘI BỘ ---
        public class CreateCategoryDto
        {
            [Required(ErrorMessage = "Tên danh mục không được để trống")]
            public string CategoryName { get; set; } = string.Empty;
        }

        public class UpdateCategoryDto
        {
            [Required(ErrorMessage = "Tên danh mục không được để trống")]
            public string CategoryName { get; set; } = string.Empty;
        }
    }
}