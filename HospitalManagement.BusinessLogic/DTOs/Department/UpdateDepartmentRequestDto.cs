using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.BusinessLogic.DTOs.Department;

public class UpdateDepartmentRequestDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
