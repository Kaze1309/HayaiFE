namespace HayaiFE.Models
{
    public class SummaryDetails
    {
        public DateTime? ExamDate { get; set; } = DateTime.Now;
        public TimeSpan? StartTime { get; set; } = TimeSpan.Zero;
        public TimeSpan? EndTime { get; set; } = TimeSpan.Zero;
        public int MaxStudentsPerBlock { get; set; }
        public int StartBlock { get; set; }

    }

}
