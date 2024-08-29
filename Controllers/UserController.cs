using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("create"), Authorize]
    public async Task<IActionResult> CreateUser(UserCreateDto dto)
    {
        try
        {
            var res = await userService.CreateUser(dto);
            return Ok(res);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("login")][AllowAnonymous]
    public async Task<IActionResult> Login(UserLoginDto login)
    {
        var res = await userService.Authenticate(login);
        
        return res.Success ? Ok(res) : Unauthorized(res);
    }

    [HttpGet("all")] [Authorize]
    public async Task<IActionResult> GetAll(
        int page, 
        int size, 
        string? sortBy = "Id", 
        string? sortOrder = "ASC")
    {
        try
        {
            var user = HttpContext.User.Identity!.Name;
            Console.WriteLine("Name: "+user);
            var paging = PageRequest.GetPaging(page, size, sortBy, sortOrder);
            var res = await userService.GetAllUsers(paging);
            return Ok(res);
        }
        catch (Exception e)
        {
            return BadRequest(ErrorResponse.InvalidParams());
        }
    }

    [HttpGet("get-roles/{userId:guid}")][Authorize]
    public async Task<IActionResult> GetRolesFromUser(Guid userId)
    {
        try
        {
            var res = await userService.FindRolesByUser(userId);
            return Ok(res);
        }
        catch (Exception)
        {
            return BadRequest("User Id not found");
        }
    }
    
    [HttpPut("unlock/{userId:guid}")][Authorize]
    public async Task<IActionResult> UnlockUser(Guid userId)
    {
        await userService.UnlockUser(userId);
        return Ok();
    }
}