using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace Api.Controllers;

[Route("api/v1/suppliers")]
public class SuppliersController : BaseController
{
    private readonly IRepository<Supplier> _supplierRepository;

    public SuppliersController(IRepository<Supplier> supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategoris()
    {
        return Ok(await _supplierRepository.ToListAsync());
    }
}
