namespace Recruitment_Project.DTOs.JobSeekers
{
    public class CvDocumentDto
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
