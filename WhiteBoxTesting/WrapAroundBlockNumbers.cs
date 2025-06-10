using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HayaiFE.Models;
using HayaiFE.Data;
namespace WhiteBoxTesting
{
    public class WrapAroundBlockNumbers
    {
        [Fact]
        public void CreateDocument_HandlesWrapAroundBlockNumbers()
        {
            var savedExams = new List<SavedExamDetails>
    {
        new() {
            Subject = "ML",
            ExamYear = "TE",
            ExamBranch = "Computer",
            StudentCount = 40,
            SavedExtractedSeatNumbers = Enumerable.Range(1, 40).Select(i => $"S{i:000}").ToList()
        }
    };
            var summary = new SummaryDetails
            {
                MaxStudentsPerBlock = 30,
                StartBlock = 35,
                ExamDate = DateTime.Now,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12)
            };

            var blocks = Enumerable.Range(1, 35).Select(i => new BlockData { BlockNumber = i }).ToList();
            var excel = new CreateExcel();

            var stream = excel.CreateDocument(savedExams, 40, summary, blocks);

            Assert.Equal("ML", blocks[35 % 35].Subject); // wraps back to Block 1
        }

    }
}
