﻿namespace HayaiFE.Models
{
    public class SavedExamDetails
    {
        public string Subject { get; set; }
        public string ExamYear { get; set; }
        public string ExamBranch { get; set; }
        public int StudentCount { get; set; }
        public List<string> SavedExtractedSeatNumbers { get; set; }
    }

}
