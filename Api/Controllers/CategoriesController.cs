using Api.Auth;
using Api.Models;
using Api.Utils;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace Api.Controllers;

[Authorize(Roles = PolicyName.ADMIN)]
[Route("api/v1/categories")]
public class CategoriesController : BaseController
{
    private readonly IRepository<Category> _catgoryRepository;

    public CategoriesController(IRepository<Category> catgoryRepository)
    {
        _catgoryRepository = catgoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await _catgoryRepository.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategories([FromBody] CreateCategoryRequest req) {
        var entity = Mapper.Map(req, new Category());
        await _catgoryRepository.CreateAsync(entity);
        return StatusCode(StatusCodes.Status201Created);
    }
}
