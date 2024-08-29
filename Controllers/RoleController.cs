using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers
{
    [ApiController, Route("/api/role")]
    public class RoleController(IRoleAppService roleService) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole(RoleCreateDto dto)
        {
            return Ok(await roleService.CreateRole(dto));
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllRoles(int page = 1, int size = 10, 
            string? sortBy = "Id", 
            string? orderBy = SortOrder.ASC)
        {
            var paging = PageRequest.GetPaging(page, size, sortBy, orderBy);
            var result = await roleService.GetAllRoles(paging);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, RoleCreateDto dto)
        {
            await roleService.UpdateRole(id, dto);
            return Ok();
        }
    }
}
