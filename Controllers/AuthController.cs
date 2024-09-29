using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/auth")]
public class AuthController(IUserAppService userAppService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(UserLoginDto login)
    {
        var res = await userAppService.Authenticate(login);

        return res.Success ? Ok(res) : Unauthorized(res);
    }
}