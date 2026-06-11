using System.Text;
using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public partial class AdminService
{
    public async Task<UserSummaryDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("User", userId);
        return new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task UpdateUserEmailAsync(Guid userId, UpdateUserEmailRequestDto request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("User", userId);
        var cleanEmail = request.Email.ToLower().Trim();
        if (await _uow.Users.AnyAsync(u => u.Email == cleanEmail && u.Id != userId, ct))
            throw new HospitalManagement.DataAccess.Exceptions.BusinessRuleViolationException("DuplicateEmail", $"Email '{cleanEmail}' is already taken.");
        user.Email = cleanEmail;
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);
    }

    public async Task<ResetPasswordResponseDto> ResetUserPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("User", userId);
        var newPassword = GenerateTemporaryPassword();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);
        
        // Trigger email notification
        await _notificationService.NotifyPasswordResetAsync(user.Id, newPassword, ct);
        
        return new ResetPasswordResponseDto { NewPassword = newPassword };
    }

    public async Task<byte[]> ExportUsersCsvAsync(CancellationToken ct = default)
    {
        var users = await _uow.Users.Query().OrderBy(u => u.CreatedAt).ToListAsync(ct);
        var builder = new StringBuilder();
        builder.AppendLine("Id,Email,PhoneNumber,Role,IsActive,CreatedAt");
        foreach (var u in users)
        {
            builder.AppendLine($"{u.Id},{u.Email},{u.PhoneNumber},{u.Role},{u.IsActive},{u.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public async Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default)
    {
        var totalUsers = await _uow.Users.Query().CountAsync(ct);
        var activeUsers = await _uow.Users.Query().CountAsync(u => u.IsActive, ct);
        var roleGroups = await _uow.Users.Query()
            .GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(ct);
        
        return new UserStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            UsersByRole = roleGroups.ToDictionary(g => g.Role, g => g.Count)
        };
    }

    public async Task<BulkCreateUsersResponseDto> BulkCreateUsersAsync(BulkCreateUsersRequestDto request, CancellationToken ct = default)
    {
        var response = new BulkCreateUsersResponseDto();
        foreach (var userReq in request.Users)
        {
            try
            {
                var created = await CreateUserAsync(userReq, ct);
                response.CreatedUsers.Add(created);
                response.Successful++;
            }
            catch (Exception ex)
            {
                response.Failed++;
                response.Errors.Add($"Failed to create {userReq.Email}: {ex.Message}");
            }
        }
        return response;
    }
}
