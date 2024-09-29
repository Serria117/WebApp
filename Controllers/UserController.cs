using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/user")]
public class UserController(IUserAppService userAppService) : ControllerBase
{
    [HttpPost("create"), HasAuthority(Permissions.UserCreate)]
    public async Task<IActionResult> CreateUser(UserInputDto dto)
    {
        try
        {
            var res = await userAppService.CreateUser(dto);
            return Ok(res);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("all")] [HasAuthority(Permissions.UserView)]
    public async Task<IActionResult> GetAll([FromQuery] RequestParam req)
    {
        try
        {
            var paging = PageRequest.GetPage(req);
            var res = await userAppService.GetAllUsers(paging);
            return Ok(res);
        }
        catch (Exception)
        {
            return BadRequest(ErrorResponse.InvalidParams());
        }
    }

    [HttpGet("get-roles/{userId:guid}")] [HasAuthority(Permissions.UserView)]
    public async Task<IActionResult> GetRolesFromUser(Guid userId)
    {
        try
        {
            var res = await userAppService.FindRolesByUser(userId);
            return Ok(res);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "User Id not found" });
        }
    }

    [HttpPut("unlock/{userId:guid}")] [HasAuthority(Permissions.UserUpdate)]
    public async Task<IActionResult> UnlockUser(Guid userId)
    {
        await userAppService.UnlockUser(userId);
        return Ok(new { message = "user unlocked" });
    }

    [HttpPut("role-update/{userId:guid}")] [HasAuthority(Permissions.UserUpdate)]
    public async Task<IActionResult> UpdateRoles(Guid userId, List<int> roleIds)
    {
        var result = await userAppService.ChangeUserRoles(userId, roleIds);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}