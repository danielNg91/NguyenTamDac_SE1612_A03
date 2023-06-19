using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Repository;
using System.Linq.Expressions;

namespace Api.Controllers;

[Route("[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    private IMapper _mapper;
    
    protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetService<IMapper>();

    protected int CurrentUserID => GetUserID();

    protected int GetUserID()
    {
        var userID = HttpContext.User.Claims.FirstOrDefault(a => a.Type == "id");
        if (userID is null)
        {
            return 0;
        }
        return int.Parse(userID.Value);
    }
}
