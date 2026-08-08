namespace Recruitment_Project.DTOs.Matching
{
    public class MatchResultDto
    {
        public double TotalMatchScore { get; set; }

        public MatchBreakdownDto Breakdown { get; set; } = new();

        public List<string> MatchedSkills { get; set; } = new();

        public List<string> MissingSkills { get; set; } = new();
    }
}