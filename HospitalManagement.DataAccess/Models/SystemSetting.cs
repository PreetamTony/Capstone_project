using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.DataAccess.Models;

public class SystemSetting : BaseEntity
{
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = "string"; // string, integer, boolean, decimal
}
