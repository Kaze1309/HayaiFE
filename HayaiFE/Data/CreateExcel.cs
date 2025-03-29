using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using HayaiFE.Models;
namespace HayaiFE.Data
{
    public class CreateExcel
    {
        public string FILEPATH = "Data\\Sample.pdf";
        public string ExtractYear(string filePath)
        {
            using FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);

            string firstPageText = loadedDocument.Pages[0].ExtractText();

            Regex yearRegex = new Regex(@"(F\.E\.|S\.E\.|T\.E\.|B\.E\.)", RegexOptions.Multiline);
            Match match = yearRegex.Match(firstPageText);

            return match.Success ? match.Groups[1].Value : "Unknown";
        }
        public static string ExtractBranchName(string filePath)
        {
            using FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);

            string firstPageText = loadedDocument.Pages[0].ExtractText();

            // More flexible regex to match different formats
            string pattern = @"(?:F\.E\.|S\.E\.|T\.E\.|B\.E\.)\s*\(\s*\d{4}\s*PAT\.\s*\)\s*\(\s*([^()]+)\s*\)";
            Match match = Regex.Match(firstPageText, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return "NA"; // Return "Unknown" if branch name is not found
        }

        public List<Subject> ExtractSubjectsFromTable(string filePath)
        {
            List<Subject> subjects = new List<Subject>();
            HashSet<string> seenSubjects = new HashSet<string>(); // ✅ Track unique subjects

            using FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);

            StringBuilder fullText = new StringBuilder();
            for (int i = 0; i < loadedDocument.PageCount; i++)
            {
                string pageText = loadedDocument.Pages[i].ExtractText();
                fullText.Append(pageText + "\n");
            }

            // ✅ Regex to match subject code, name, and multiple types within [[ ]]
            Regex subjectRegex = new Regex(
                @"SUB:\s*(\d{6,7}[A-Z]?)\s+([\w\s\(\)\-\&\,\.]+?)\s*(\[\[.+?\]\])",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            MatchCollection matches = subjectRegex.Matches(fullText.ToString());

            foreach (Match match in matches)
            {
                string subjectCode = match.Groups[1].Value.Trim();
                string subjectName = Regex.Replace(match.Groups[2].Value, @"\s+", " ").Trim();
                string subjectType = match.Groups[3].Value.Trim(); // ✅ Entire type block (e.g., `[[IN],[TH]]`)

                string uniqueKey = $"{subjectCode}_{subjectName}_{subjectType}";

                if (!seenSubjects.Contains(uniqueKey))
                {
                    seenSubjects.Add(uniqueKey);
                    subjects.Add(new Subject(subjectCode, subjectName, subjectType));
                }
            }

            return subjects;
        }
        public Dictionary<string, List<string>> ExtractStudentSeatNumbers(List<Subject> subjects, string year, string filePath)
        {
            Dictionary<string, List<string>> subjectSeats = new Dictionary<string, List<string>>();

            using FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);

            StringBuilder fullText = new StringBuilder();
            for (int i = 0; i < loadedDocument.PageCount; i++)
            {
                fullText.Append(loadedDocument.Pages[i].ExtractText() + "\n");
            }

            string seatPrefix = year switch
            {
                "F.E." => "F",
                "S.E." => "S",
                "T.E." => "T",
                "B.E." => "B",
                _ => ""
            };

            foreach (var subject in subjects)
            {
                string subjectKey = $"{subject.Code}_{subject.Name}_{subject.Type}";

                // ✅ Updated Regex to match subjects with specific type
                string pattern = @$"SUB:\s*{subject.Code}\s+{Regex.Escape(subject.Name)}\s*{Regex.Escape(subject.Type)}.*?NO\.OF STUDENTS:\s*\d+\s*([\s\S]+?)(?=\nSUB:\s*\d+|\nReport|$)";
                Match match = Regex.Match(fullText.ToString(), pattern, RegexOptions.Singleline);

                if (match.Success)
                {
                    string studentsText = match.Groups[1].Value
                        .Replace("\n", " ")
                        .Replace("\r", "")
                        .Trim();

                    string[] seatNumbers = studentsText
                        .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(s => s.StartsWith(seatPrefix) && s.Length > 1 && char.IsDigit(s[1])) // ✅ Checks next character after prefix is a digit
                        .ToArray();

                    subjectSeats[subjectKey] = seatNumbers.ToList();
                }
            }

            return subjectSeats;
        }


        public MemoryStream CreateDocument()
        {
            string year = ExtractYear(FILEPATH);
            List<Subject> subjects = ExtractSubjectsFromTable(FILEPATH);
            Dictionary<string, List<string>> subjectSeats = ExtractStudentSeatNumbers(subjects, year, FILEPATH);
            string branchName = ExtractBranchName(FILEPATH);
            PdfDocument newPdf = new PdfDocument();
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
            PdfBrush brush = PdfBrushes.Black;
            PdfStringFormat format = new PdfStringFormat() { WordWrap = PdfWordWrapType.Word };

            PdfPage page = newPdf.Pages.Add();
            PdfGraphics graphics = page.Graphics;
            float yPosition = 20;

            graphics.DrawString($"Extracted Subjects & Seat Numbers ({year} {branchName})",
                                new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold),
                                brush, new Syncfusion.Drawing.PointF(10, yPosition));
            yPosition += 30;

            foreach (var subject in subjects)
            {
                string subjectKey = $"{subject.Code}_{subject.Name}_{subject.Type}";

                graphics.DrawString($"Code: {subject.Code}  -  Name: {subject.Name}  - Type: {subject.Type}",
                                    font, brush, new Syncfusion.Drawing.PointF(10, yPosition), format);
                yPosition += 20;

                if (subjectSeats.ContainsKey(subjectKey) && subjectSeats[subjectKey].Count > 0)
                {
                    graphics.DrawString("Seat Numbers:", font, brush, new Syncfusion.Drawing.PointF(10, yPosition), format);
                    yPosition += 20;

                    // ✅ Print seat numbers in chunks for readability
                    int chunkSize = 10; // Adjust the number of seats per line if necessary
                    var seatChunks = subjectSeats[subjectKey]
                        .Select((seat, index) => new { seat, index })
                        .GroupBy(x => x.index / chunkSize)
                        .Select(g => string.Join(", ", g.Select(x => x.seat)));

                    foreach (string seatLine in seatChunks)
                    {
                        graphics.DrawString(seatLine, font, brush, new Syncfusion.Drawing.PointF(10, yPosition), format);
                        yPosition += 20;

                        // ✅ Ensure new page if content exceeds the limit
                        if (yPosition > 750)
                        {
                            page = newPdf.Pages.Add();
                            graphics = page.Graphics;
                            yPosition = 20;
                        }
                    }
                }
                else
                {
                    graphics.DrawString("No seat numbers found!", font, brush, new Syncfusion.Drawing.PointF(10, yPosition), format);
                    yPosition += 20;
                }

                // ✅ Ensure new page if content exceeds the limit
                if (yPosition > 750)
                {
                    page = newPdf.Pages.Add();
                    graphics = page.Graphics;
                    yPosition = 20;
                }
            }

            MemoryStream pdfStream = new MemoryStream();
            newPdf.Save(pdfStream);
            newPdf.Close(true);
            pdfStream.Position = 0;

            return pdfStream;
        }
        public List<ExamDetails> ExtractExamDetails(string filePath)
        {
            string year = ExtractYear(filePath);
            string branchName = ExtractBranchName(filePath);
            List<Subject> extractedSubjects = ExtractSubjectsFromTable(filePath);
            Dictionary<string, List<string>> extractedSeats = ExtractStudentSeatNumbers(extractedSubjects, year, filePath);

            List<ExamDetails> examDetailsList = new List<ExamDetails>();

            foreach (var subject in extractedSubjects)
            {
                string subjectKey = $"{subject.Code}_{subject.Name}_{subject.Type}";

                examDetailsList.Add(new ExamDetails
                {
                    ExamYear = year,
                    ExtractedSubjects = new List<Subject> { subject },
                    ExamBranch = branchName,  // ✅ Branch is now stored correctly// ✅ Store only this subject
                    ExtractedSeatNumbers = extractedSeats.ContainsKey(subjectKey) ? new Dictionary<string, List<string>> { { subjectKey, extractedSeats[subjectKey] } } : new Dictionary<string, List<string>>()
                });
            }

            return examDetailsList;
        }


        // Class to store extracted details
        public class ExamDetails
        {
            public string ExamYear { get; set; }
            public List<Subject> ExtractedSubjects { get; set; }
            public string ExamBranch { get; set; }
            public Dictionary<string, List<string>> ExtractedSeatNumbers { get; set; }
        }

    }

}






