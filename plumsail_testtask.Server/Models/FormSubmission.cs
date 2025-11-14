namespace plumsail_testtask.Server.Models
{
    public class FormSubmission
    {
        public Guid Id { get; set; }
        public string FormType { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string DataJson { get; set; } = string.Empty;
    }
}
