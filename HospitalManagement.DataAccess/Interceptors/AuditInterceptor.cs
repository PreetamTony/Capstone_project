using System.Text.Json;
using HospitalManagement.DataAccess.Interfaces;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HospitalManagement.DataAccess.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public AuditInterceptor(ICurrentUserService currentUserService, ICorrelationIdAccessor correlationIdAccessor)
    {
        _currentUserService = currentUserService;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = new List<AuditLog>();
        
        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var entityName = entry.Entity.GetType().Name;
            AuditActionType action = AuditActionType.UPDATE; // default
            
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var changedFields = new List<string>();
            var recordId = string.Empty;

            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (primaryKey != null)
                recordId = primaryKey.CurrentValue?.ToString() ?? string.Empty;

            switch (entry.State)
            {
                case EntityState.Added:
                    foreach (var prop in entry.Properties)
                    {
                        if (!prop.IsTemporary)
                            newValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    action = AuditActionType.CREATE;
                    break;

                case EntityState.Deleted:
                    foreach (var prop in entry.Properties)
                    {
                        oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    }
                    action = AuditActionType.DELETE;
                    break;

                case EntityState.Modified:
                    // Check if it's a Soft Delete
                    var isDeletedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
                    if (isDeletedProp != null && isDeletedProp.IsModified && (bool)(isDeletedProp.CurrentValue ?? false))
                    {
                        action = AuditActionType.SOFT_DELETE;
                        oldValues["IsDeleted"] = isDeletedProp.OriginalValue;
                        newValues["IsDeleted"] = isDeletedProp.CurrentValue;
                        changedFields.Add("IsDeleted");
                    }
                    else
                    {
                        action = AuditActionType.UPDATE;
                        foreach (var prop in entry.Properties)
                        {
                            if (prop.IsModified)
                            {
                                oldValues[prop.Metadata.Name] = prop.OriginalValue;
                                newValues[prop.Metadata.Name] = prop.CurrentValue;
                                changedFields.Add(prop.Metadata.Name);
                            }
                        }
                    }
                    break;
            }

            var auditLog = new AuditLog
            {
                UserId = _currentUserService.UserId,
                UserEmail = _currentUserService.Email,
                UserRole = _currentUserService.Role,
                PerformedByName = _currentUserService.Name,
                Action = action,
                EntityName = entityName,
                RecordId = recordId,
                OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
                ChangedFields = changedFields.Count > 0 ? changedFields.ToArray() : null,
                CorrelationId = _correlationIdAccessor.CorrelationId,
                IpAddress = _currentUserService.IpAddress,
                UserAgent = _currentUserService.UserAgent,
                Timestamp = DateTime.UtcNow
            };

            auditEntries.Add(auditLog);
        }

        if (auditEntries.Any())
        {
            eventData.Context.Set<AuditLog>().AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
