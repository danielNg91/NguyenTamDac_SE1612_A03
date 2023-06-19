using Api.Auth;
using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace Api.Controllers;

[Authorize]
[Route("api/v1/categories")]
public class CategoriesController : BaseController
{
    private readonly IRepository<Category> _catgoryRepository;

    public CategoriesController(IRepository<Category> catgoryRepository)
    {
        _catgoryRepository = catgoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategoris()
    {
        return Ok(await _catgoryRepository.ToListAsync());
    }
}
