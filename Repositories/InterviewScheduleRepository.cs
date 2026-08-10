using Microsoft.EntityFrameworkCore;
using Recruitment_Project.Data;
using Recruitment_Project.Interfaces.Repositories;
using Recruitment_Project.Models.Entities;

namespace Recruitment_Project.Repositories
{
    public class InterviewScheduleRepository 
        : IInterviewScheduleRepository
    {
        private readonly AppDbContext _context;

        public InterviewScheduleRepository(
            AppDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            InterviewSchedule interview)
        {
            await _context.InterviewSchedules
                .AddAsync(interview);
        }


        public async Task<List<InterviewSchedule>> GetByEmployerAsync(
            int employerProfileId)
        {
            return await _context.InterviewSchedules
                .Include(x => x.JobApplication)
                .ThenInclude(x => x.Vacancy)
                .Where(x =>
                    x.JobApplication.Vacancy.EmployerProfileId 
                    == employerProfileId)
                .OrderByDescending(x => x.InterviewDate)
                .ToListAsync();
        }


        public async Task<List<InterviewSchedule>> GetByJobSeekerAsync(
            int jobSeekerProfileId)
        {
            return await _context.InterviewSchedules
                .Include(x => x.JobApplication)
                .Where(x =>
                    x.JobApplication.JobSeekerProfileId 
                    == jobSeekerProfileId)
                .OrderByDescending(x => x.InterviewDate)
                .ToListAsync();
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}