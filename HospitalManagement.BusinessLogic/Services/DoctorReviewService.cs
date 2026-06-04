using HospitalManagement.BusinessLogic.DTOs.Doctor;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class DoctorReviewService : IDoctorReviewService
{
    private readonly IUnitOfWork _uow;

    public DoctorReviewService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<DoctorReviewResponseDto> CreateReviewAsync(CreateDoctorReviewRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(request.DoctorId, ct)
            ?? throw new NotFoundException("Doctor", request.DoctorId);

        var patient = await _uow.Patients.GetByIdAsync(request.PatientId, ct)
            ?? throw new NotFoundException("Patient", request.PatientId);

        var review = new DoctorReview
        {
            DoctorId = request.DoctorId,
            PatientId = request.PatientId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _uow.DoctorReviews.AddAsync(review, ct);

        // Update doctor average rating
        var reviews = await _uow.DoctorReviews.Query().Where(r => r.DoctorId == request.DoctorId).ToListAsync(ct);
        if (reviews.Any())
        {
            doctor.Rating = (decimal)reviews.Average(r => r.Rating);
            _uow.Doctors.Update(doctor);
        }

        await _uow.CompleteAsync(ct);

        return new DoctorReviewResponseDto
        {
            Id = review.Id,
            DoctorId = review.DoctorId,
            PatientId = review.PatientId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<List<DoctorReviewResponseDto>> GetReviewsByDoctorIdAsync(Guid doctorId, CancellationToken ct = default)
    {
        var reviews = await _uow.DoctorReviews.Query()
            .Where(r => r.DoctorId == doctorId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return reviews.Select(r => new DoctorReviewResponseDto
        {
            Id = r.Id,
            DoctorId = r.DoctorId,
            PatientId = r.PatientId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();
    }
}
