using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.DTOs.Visit;

public class VisitFilterDto : PaginationFilter
{
    public VisitStatus? Status { get; set; }
    public VisitType? VisitType { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Sorting { get; set; } = "date_desc"; // e.g., date_desc, date_asc
}
