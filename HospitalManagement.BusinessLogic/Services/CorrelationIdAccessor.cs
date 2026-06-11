using HospitalManagement.DataAccess.Interfaces;

namespace HospitalManagement.BusinessLogic.Services;

public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

//It generates a unique Id for each transaction 