using Application.Exceptions;
using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Auth;

public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = (AspNetUser)context.HttpContext.Items["User"];
        if (user == null)
        {
            throw new ForbiddenException();
        }
    }
}
