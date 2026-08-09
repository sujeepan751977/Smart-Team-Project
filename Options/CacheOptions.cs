namespace Recruitment_Project.Options
{
    public class CacheOptions
    {
        public const string SectionName = "Cache";

        public int DefaultExpirationMinutes { get; set; } = 10;
    }
}