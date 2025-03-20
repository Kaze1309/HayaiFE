using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;

namespace HayaiFE.Data
{
    public class CreateExcel
    {
        private string filePath = "Data\\SampleFE.pdf"; // Path to the uploaded file
                                                        // ✅ Remove unwanted repeated text before processing
                                                        // ✅ Extract Year from PDF (F.E., S.E., T.E., B.E.)
        public string ExtractYear()
        {
            using FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);

            string firstPageText = loadedDocument.Pages[0].ExtractText();

            Regex yearRegex = new Regex(@"(F\.E\.|S\.E\.|T\.E\.|B\.E\.)", RegexOptions.Multiline);
            Match match = yearRegex.Match(firstPageText);

            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        // ✅ Extract Subject Names from All Pages
        public List<Subject> ExtractSubjectsFromTable()
        {
            List<Subject> subjects = new List<Subject>();
            HashSet<string> seenSubjects = new HashSet<string>(); // ✅ Track UNIQUE subjects by Code + Name + Type

            using FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);

            StringBuilder fullText = new StringBuilder();
            for (int i = 0; i < loadedDocument.PageCount; i++)
            {
                string pageText = loadedDocument.Pages[i].ExtractText();
                fullText.Append(pageText + "\n");
            }

            // ✅ Updated Regex to correctly capture subject codes, names, and types while handling multi-line names
            Regex subjectRegex = new Regex(
                @"SUB:\s*(\d{6,7}[A-Z]?)\s+([\w\s\(\)\-\&\,\.]+?)\s*(\[\[[A-Z,\s]+\]\](?:\s*,\s*\[\[[A-Z,\s]+\]\])*)",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            MatchCollection matches = subjectRegex.Matches(fullText.ToString());

            foreach (Match match in matches)
            {
                string subjectCode = match.Groups[1].Value.Trim();
                string subjectName = Regex.Replace(match.Groups[2].Value, @"\s+", " ").Trim(); // Normalize spaces
                string subjectType = match.Groups[3].Value.Trim(); // ✅ Preserve square brackets `[[IN],[TH]]`

                // ✅ Use Subject Type in Unique Key to capture all variations
                string uniqueKey = $"{subjectCode}_{subjectName}_{subjectType}";
                if (!seenSubjects.Contains(uniqueKey))
                {
                    seenSubjects.Add(uniqueKey);
                    subjects.Add(new Subject(subjectCode, subjectName, subjectType));
                }
            }

            return subjects;
        }

        // ✅ Extract Student Seat Numbers Based on Year
        public Dictionary<string, List<string>> ExtractStudentSeatNumbers(List<Subject> subjects, string year)
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
                string subjectKey = subject.ToString();

                // ✅ Updated Regex: Matches subject with exact Code, Name, and Type
                string pattern = @$"SUB:\s*{subject.Code}\s+{Regex.Escape(subject.Name)}\s*{Regex.Escape(subject.Type)}.*?NO\.OF STUDENTS:\s*\d+\s*([\s\S]+?)(?=\nSUB:\s*\d+|\nReport|$)";
                Match match = Regex.Match(fullText.ToString(), pattern, RegexOptions.Singleline);

                if (match.Success)
                {
                    string studentsText = match.Groups[1].Value
                        .Replace("\n", " ")
                        .Replace("\r", "")
                        .Trim();

                    string[] seatNumbers = studentsText.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Where(s => s.StartsWith(seatPrefix)) // ✅ Filter based on Year Prefix
                                                       .ToArray();

                    subjectSeats[subjectKey] = seatNumbers.ToList();
                }
            }

            return subjectSeats;
        }

        // ✅ Create Final PDF Report & Return as MemoryStream
        public MemoryStream CreateDocument()
        {
            string year = ExtractYear();
            List<Subject> subjects = ExtractSubjectsFromTable();
            Dictionary<string, List<string>> subjectSeats = ExtractStudentSeatNumbers(subjects, year);

            PdfDocument newPdf = new PdfDocument();
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
            PdfBrush brush = PdfBrushes.Black;
            PdfStringFormat format = new PdfStringFormat() { WordWrap = PdfWordWrapType.Word };

            PdfPage page = newPdf.Pages.Add();
            PdfGraphics graphics = page.Graphics;
            float yPosition = 20;

            graphics.DrawString($"Extracted Subjects & Seat Numbers ({year})",
                                new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold),
                                brush, new Syncfusion.Drawing.PointF(10, yPosition));
            yPosition += 30;

            foreach (var subject in subjects)
            {
                string subjectKey = subject.ToString();

                graphics.DrawString($"Code: {subject.Code}  -  Name: {subject.Name}  - Type: {subject.Type}",
                                    font, brush, new Syncfusion.Drawing.PointF(10, yPosition), format);
                yPosition += 20;

                if (subjectSeats.ContainsKey(subjectKey))
                {
                    string seats = string.Join(", ", subjectSeats[subjectKey]);
                    graphics.DrawString($"Seat Numbers: {seats}", font, brush, new Syncfusion.Drawing.PointF(10, yPosition), format);
                    yPosition += 40;
                }
                else
                {
                    graphics.DrawString("No seat numbers found!", font, brush, new Syncfusion.Drawing.PointF(10, yPosition), format);
                    yPosition += 20;
                }

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
    }

    public class Subject
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public Subject(string code, string name, string type)
        {
            Code = code;
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Code}_{Name}_{Type}";
        }
    }
}
