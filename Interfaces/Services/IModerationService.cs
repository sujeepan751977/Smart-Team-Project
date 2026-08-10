namespace Recruitment_Project.Interfaces.Services
{
    public interface IModerationService
    {
        Task WarnEmployerAsync(
            int adminUserId,
            int employerId,
            string decisionNote);

        Task SuspendEmployerAsync(
            int adminUserId,
            int employerId,
            string decisionNote);

        Task DisableEmployerAsync(
            int adminUserId,
            int employerId,
            string decisionNote);

        Task CloseVacancyAsync(
            int adminUserId,
            int vacancyId,
            string decisionNote);
    }
}