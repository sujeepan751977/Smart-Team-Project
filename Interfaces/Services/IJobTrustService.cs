namespace Recruitment_Project.Interfaces.Services
{
    public interface IJobTrustService
    {
        Task<string> GetTrustLabelAsync(int vacancyId);
    }
}