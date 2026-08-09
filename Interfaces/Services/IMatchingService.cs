using Recruitment_Project.DTOs.Matching;

namespace Recruitment_Project.Interfaces.Services
{
    public interface IMatchingService
    {
        Task<MatchResultDto> CalculateMatchAsync(
            int jobSeekerProfileId,
            int vacancyId);
    }
}