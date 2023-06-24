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

[Authorize(Roles = PolicyName.ADMIN)]
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

    [HttpGet]
    public async Task<IActionResult> GetCustomers() {
        return Ok(await _customerRepository.ToListAsync());
    }


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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id) {
        var target = await _customerRepository.FoundOrThrow(c => c.Id == id, new NotFoundException());
        return Ok(target);
    }

    [Authorize(Roles = $"{PolicyName.ADMIN},{PolicyName.CUSTOMER}")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile() {
        var target = await _customerRepository.FirstOrDefaultAsync(c => c.Id == CurrentUserID);
        return Ok(target);
    }

    [Authorize(Roles = $"{PolicyName.ADMIN},{PolicyName.CUSTOMER}")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest req) {
        if (!IsAdmin) {
            id = CurrentUserID;
        }
        var target = await _customerRepository.FoundOrThrow(c => c.Id == id, new NotFoundException());
        var entity = Mapper.Map(req, target);
        await _customerRepository.UpdateAsync(entity);
        return StatusCode(StatusCodes.Status204NoContent);
    }

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
