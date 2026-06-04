using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.DTOs.Doctor;

public class DoctorPaginationFilter : PaginationFilter
{
    public Guid? DepartmentId { get; set; }
}
