namespace HospitalManagement.DataAccess.Interfaces;

public interface ICorrelationIdAccessor
{
    string CorrelationId { get; set; }
}
