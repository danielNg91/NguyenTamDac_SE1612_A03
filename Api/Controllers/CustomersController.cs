using Api.Auth;
using Api.Models;
using Application.Exceptions;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Repository;

namespace Api.Controllers;

[Authorize]
[Route("api/v1/customers")]
public class CustomersController : BaseController {
    private readonly IRepository<AspNetUser> _customerRepository;
    private readonly UserManager<AspNetUser> _userManager;
    private readonly RoleManager<AspNetRole> _roleManager;
    private readonly AppSettings _appSettings;

    public CustomersController(
        IOptions<AppSettings> appSettings,
        UserManager<AspNetUser> userManager,
        RoleManager<AspNetRole> roleManager,
        IRepository<AspNetUser> customerRepository
    ) {
        _appSettings = appSettings.Value;
        _userManager = userManager;
        _roleManager = roleManager;
        _customerRepository = customerRepository;
    }

    [Authorize(Roles = PolicyName.ADMIN)]
    [HttpGet]
    public async Task<IActionResult> GetCustomers() {
        return Ok(await _customerRepository.ToListAsync());
    }


    [Authorize(Roles = PolicyName.ADMIN)]
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] RegisterRequest req) {
        await ValidateRegisterFields(req);
        var user = Mapper.Map(req, new AspNetUser());
        user.SecurityStamp = Guid.NewGuid().ToString();
        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            throw new BadRequestException(result);
        await AddUserRoles(user, PolicyName.CUSTOMER);
        return Ok();
    }

    private async Task AddUserRoles(AspNetUser user, string role = PolicyName.CUSTOMER) {
        if (!await _roleManager.RoleExistsAsync(PolicyName.ADMIN)) {
            await _roleManager.CreateAsync(new AspNetRole(PolicyName.ADMIN));
        }
        if (!await _roleManager.RoleExistsAsync(PolicyName.CUSTOMER)) {
            await _roleManager.CreateAsync(new AspNetRole(PolicyName.CUSTOMER));
        }
        if (await _roleManager.RoleExistsAsync(role)) {
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    private async Task ValidateRegisterFields(RegisterRequest req) {
        if (req.Email.Equals(_appSettings.AdminAccount.Email)) {
            throw new BadRequestException("Email already existed");
        }

        var isEmailExisted = (await _userManager.FindByEmailAsync(req.Email)) != null;
        if (isEmailExisted) {
            throw new BadRequestException("Email already existed");
        }
    }


    [Authorize(Roles = PolicyName.ADMIN)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id) {
        var target = await _customerRepository.FoundOrThrow(c => c.Id == id, new NotFoundException());
        return Ok(target);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile() {
        var target = await _customerRepository.FirstOrDefaultAsync(c => c.Id == CurrentUserID);
        return Ok(target);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest req) {
        var target = await _customerRepository.FoundOrThrow(c => c.Id == CurrentUserID, new NotFoundException());
        await UpdateUser(req, target);
        return StatusCode(StatusCodes.Status204NoContent);
    }

    [Authorize(Roles = PolicyName.ADMIN)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest req) {
        var target = await _customerRepository.FoundOrThrow(c => c.Id == id, new NotFoundException());
        await UpdateUser(req, target);
        return StatusCode(StatusCodes.Status204NoContent);
    }

    private async Task UpdateUser(UpdateCustomerRequest req, AspNetUser user) {
        var entity = Mapper.Map(req, user);
        if (!string.IsNullOrEmpty(req.Password)) {
            entity.PasswordHash = _userManager.PasswordHasher.HashPassword(entity, req.Password);
        }
        await _userManager.UpdateAsync(entity);
    }

    [Authorize(Roles = PolicyName.ADMIN)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id) {
        var target = await _userManager.FindByIdAsync(id.ToString());
        if (target == null) {
            throw new NotFoundException();
        }
        await _userManager.DeleteAsync(target);
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
