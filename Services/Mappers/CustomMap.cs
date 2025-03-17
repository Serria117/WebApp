using Riok.Mapperly.Abstractions;
using WebApp.Core.DomainEntities;
using WebApp.Services.OrganizationService.Dto;

namespace WebApp.Services.Mappers;

[Mapper]
public partial class CustomMap
{
    public partial OrganizationDisplayDto OrgToDto(Organization org);
    
}