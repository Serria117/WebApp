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
    public class RoleController(IRoleAppService roleService) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole(RoleCreateDto dto)
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
        public async Task<IActionResult> UpdateRole(int id, RoleCreateDto dto)
        {
            await roleService.UpdateRole(id, dto);
            return Ok();
        }
    }
}
