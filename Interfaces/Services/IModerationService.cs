namespace Recruitment_Project.Interfaces.Services
{
    public interface IModerationService
    {
        Task WarnEmployerAsync(
    int employerId,
    string decisionNote);

        Task SuspendEmployerAsync(
            int employerId,
            string decisionNote);

        Task DisableEmployerAsync(
            int employerId,
            string decisionNote);

        Task CloseVacancyAsync(
            int vacancyId,
            string decisionNote);
    }
}
