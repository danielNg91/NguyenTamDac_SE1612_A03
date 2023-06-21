using Api.Auth;
using Api.Models;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace Api.Controllers;

[Authorize(Roles = PolicyName.ADMIN)]
[Route("api/v1/suppliers")]
public class SuppliersController : BaseController
{
    private readonly IRepository<Supplier> _supplierRepository;

    public SuppliersController(IRepository<Supplier> supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetSupp()
    {
        return Ok(await _supplierRepository.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupp([FromBody] CreateSupplierRequest req) {
        var entity = Mapper.Map(req, new Supplier());
        await _supplierRepository.CreateAsync(entity);
        return StatusCode(StatusCodes.Status201Created);
    }
}
