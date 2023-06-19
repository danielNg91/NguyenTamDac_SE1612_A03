using Api.Models;
using Api.Utils;
using Application.Exceptions;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Repository;

namespace Api.Controllers;


[Route("api/v1/customers")]
public class CustomersController : BaseController
{
    private readonly IRepository<AspNetUser> _customerRepository;
    private readonly IOptions<AppSettings> _appSettings;

    public CustomersController(IOptions<AppSettings> appSettings, IRepository<AspNetUser> customerRepository)
    {
        _appSettings = appSettings;
        _customerRepository = customerRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        return Ok(await _customerRepository.ToListAsync());
    }


    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomer req)
    {
        await ValidateRegisterFields(req);
        AspNetUser entity = Mapper.Map(req, new AspNetUser());
        entity.CustomerId = await GetUserId();
        await _customerRepository.CreateAsync(entity);
        return StatusCode(StatusCodes.Status201Created);
    }

    private async Task<int> GetUserId()
    {
        var user = (await _customerRepository.ToListAsync()).OrderByDescending(u => u.CustomerId).FirstOrDefault();
        return user == null ? 1 : (user.CustomerId + 1);
    }

    private async Task ValidateRegisterFields(CreateCustomer req)
    {
        if (req.Email.Equals(_appSettings.Value.AdminAccount.Email))
        {
            throw new BadRequestException("Email already existed");
        }

        var isEmailExisted = (await _customerRepository.FirstOrDefaultAsync(u => u.Email == req.Email)) != null;
        if (isEmailExisted)
        {
            throw new BadRequestException("Email already existed");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var target = await _customerRepository.FoundOrThrow(c => c.CustomerId == id, new NotFoundException());
        return Ok(target);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomer req)
    {
        var target = await _customerRepository.FoundOrThrow(c => c.CustomerId == id, new NotFoundException());
        AspNetUser entity = Mapper.Map(req, target);
        await _customerRepository.UpdateAsync(entity);
        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var target = await _customerRepository.FoundOrThrow(c => c.CustomerId == id, new NotFoundException());
        await _customerRepository.DeleteAsync(target);
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
