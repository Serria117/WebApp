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
    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="dto">Input data</param>
    /// <returns>Created user</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserCreate"/> permission.
    /// </remarks>
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

/// <summary>
/// Retrieve a paginated list of all users
/// </summary>
/// <param name="req">The request parameters for pagination and filtering</param>
/// <returns>A paginated list of users or an error message if the parameters are invalid</returns>
/// <remarks>
/// Requires the <see cref="Permissions.UserView"/> permission.
/// </remarks>
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

    /// <summary>
    /// Retrieve a list of roles that a user belongs to.
    /// </summary>
    /// <param name="userId">The id of the user to retrieve roles for.</param>
    /// <returns>A list of roles that the user belongs to, or a 400 error if the user id is invalid.</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserView"/> permission.
    /// </remarks>
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

/// <summary>
/// Unlocks a user account.
/// </summary>
/// <param name="userId">The ID of the user to unlock.</param>
/// <returns>An IActionResult indicating the result of the unlock operation.</returns>
/// <remarks>
/// Requires the <see cref="Permissions.UserUpdate"/> permission.
/// </remarks>
    [HttpPut("unlock/{userId:guid}")] [HasAuthority(Permissions.UserUpdate)]
    public async Task<IActionResult> UnlockUser(Guid userId)
    {
        await userAppService.UnlockUser(userId);
        return Ok(new { message = "user unlocked" });
    }

    /// <summary>
    /// Updates the roles that a user belongs to.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="roleIds">A list of role IDs to add the user to.</param>
    /// <returns>An IActionResult indicating the result of the update operation.</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserUpdate"/> permission.
    /// </remarks>
    [HttpPut("role-update/{userId:guid}")] [HasAuthority(Permissions.UserUpdate)]
    public async Task<IActionResult> UpdateRoles(Guid userId, List<int> roleIds)
    {
        var result = await userAppService.ChangeUserRoles(userId, roleIds);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}