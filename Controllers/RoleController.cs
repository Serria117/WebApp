using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers
{
    [ApiController, Route("/api/role")][Authorize]
    public class RoleController(IRoleAppService roleService, IPermissionAppService permissionService) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole(RoleInputDto dto)
        {
            return Ok(await roleService.CreateRole(dto));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRoles([FromQuery] RequestParam req)
        {
            var paging = PageRequest.GetPage(req);
            var result = await roleService.GetAllRoles(paging);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, RoleInputDto dto)
        {
            await roleService.UpdateRole(id, dto);
            return Ok();
        }

        [HttpGet("permissions-in-role/{roleId:int}")]
        public async Task<IActionResult> GetPermissionsInRole(int roleId)
        {
            var result = await roleService.GetAllPermissionsInRole(roleId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all-permissions")]
        public async Task<IActionResult> GetAllPermissionsInSystem()
        {
            var result = await permissionService.GetAllPermissionsInSystem();
            return Ok(result);
        }
    }
}
