using Recruitment_Project.DTOs.InterviewSchedules;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Interfaces.Services;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Services
{
    public class InterviewScheduleService 
        : IInterviewScheduleService
    {
        private readonly IInterviewScheduleRepository _repository;
        private readonly IEmployerRepository _employerRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobSeekerRepository _jobSeekerRepository;


        public InterviewScheduleService(
            IInterviewScheduleRepository repository,
            IEmployerRepository employerRepository,
            IApplicationRepository applicationRepository,
            IJobSeekerRepository jobSeekerRepository)
        {
            _repository = repository;
            _employerRepository = employerRepository;
            _applicationRepository = applicationRepository;
            _jobSeekerRepository = jobSeekerRepository;
        }



        public async Task<InterviewScheduleDto> CreateAsync(
            int userId,
            int applicationId,
            CreateInterviewScheduleDto dto)
        {
            var employer =
                await _employerRepository
                .GetByUserIdAsync(userId);

            if (employer == null)
                throw new KeyNotFoundException(
                    "Employer profile not found.");


            var application =
                await _applicationRepository
                .GetByIdAsync(applicationId);


            if (application == null)
                throw new KeyNotFoundException(
                    "Application not found.");


            if (application.Vacancy.EmployerProfileId 
                != employer.Id)
            {
                throw new UnauthorizedAccessException(
                    "Not your application.");
            }


            var interview = new InterviewSchedule
            {
                JobApplicationId = applicationId,
                Title = dto.Title,
                InterviewDate = dto.InterviewDate,
                DurationMinutes = dto.DurationMinutes,
                Location = dto.Location,
                MeetingLink = dto.MeetingLink,
                Instructions = dto.Instructions
            };


            await _repository.AddAsync(interview);
            await _repository.SaveChangesAsync();


            return Map(interview);
        }



        public async Task<List<InterviewScheduleDto>>
            GetEmployerInterviewsAsync(int userId)
        {
            var employer =
                await _employerRepository
                .GetByUserIdAsync(userId);

            if (employer == null)
                throw new KeyNotFoundException();


            var data =
                await _repository
                .GetByEmployerAsync(employer.Id);


            return data.Select(Map).ToList();
        }



        public async Task<List<InterviewScheduleDto>>
            GetJobSeekerInterviewsAsync(int userId)
        {
            var seeker =
                await _jobSeekerRepository
                .GetProfileByUserIdAsync(userId);

            if (seeker == null)
                throw new KeyNotFoundException();


            var data =
                await _repository
                .GetByJobSeekerAsync(seeker.Id);


            return data.Select(Map).ToList();
        }



        private InterviewScheduleDto Map(
            InterviewSchedule x)
        {
            return new InterviewScheduleDto
            {
                Id = x.Id,
                JobApplicationId = x.JobApplicationId,
                Title = x.Title,
                InterviewDate = x.InterviewDate,
                DurationMinutes = x.DurationMinutes,
                Location = x.Location,
                MeetingLink = x.MeetingLink,
                Instructions = x.Instructions,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatedAt
            };
        }
    }
}