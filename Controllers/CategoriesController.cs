using Microsoft.AspNetCore.Mvc;
using StoreApi.DTOs;
using StoreApi.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StoreApi.Controllers;



[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;
    
    public CategoriesController(
        ICategoryService categoryService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
    {
        try
        {
            _logger.LogInformation("start GetAllCategories from function GetAll");
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetAll");
            return BadRequest("Failed GetAll");

        }
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        
        if (category == null)
        {
            _logger.LogWarning($"GetById try to get Category with ID {id} not found.");
            return NotFound(new { message = $"Category with ID {id} not found." });
        }
        
        return Ok(category);
    }
    
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CategoryCreateDto createDto)
    {
        try
        {
            var category = await _categoryService.CreateCategoryAsync(createDto);
            _logger.LogInformation("Create category, category Name: {CategoryName}", createDto.Name  );

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex,"Error occurred while creating , category Name: {CategoryName}", createDto.Name  );
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryResponseDto>> Update(int id, [FromBody] CategoryUpdateDto updateDto)
    {
        try
        {
            var category = await _categoryService.UpdateCategoryAsync(id, updateDto);
            
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            
            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        
        if (!result)
        {
            return NotFound(new { message = $"Category with ID {id} not found." });
        }
        
        return NoContent();
    }
}
