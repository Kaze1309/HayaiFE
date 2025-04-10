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
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using System.Drawing;
namespace HayaiFE.Data
{
    public class CreateExcel
    {

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

                string NormalizeKey(string rawKey)
                {
                    if (string.IsNullOrWhiteSpace(rawKey)) return "";

                    // Collapse multiple spaces → single space
                    string collapsed = System.Text.RegularExpressions.Regex.Replace(rawKey, @"\s+", " ");

                    // Remove leading/trailing whitespace and make consistent casing (optional)
                    return collapsed.Trim();
                }
                string subjectKey = NormalizeKey($"{subject.Code}_{subject.Name}_{subject.Type}");

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

        public MemoryStream FinalReport(List<BlockData> blocks, List<Teacher> teachers)
        {
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];
                sheet.Name = "Teacher Assignment";

                // Header
                sheet.Range["A1"].Text = "Block No";
                sheet.Range["D1"].Text = "Subject";
                sheet.Range["E1"].Text = "Branch";
                sheet.Range["F1"].Text = "Assigned Teacher";
                sheet.Range["G1"].Text = "Teacher Department";

                int row = 2;
                var availableTeachers = new List<Teacher>(teachers);
                Random rng = new Random();

                foreach (var block in blocks)
                {
                    // Skip unassignable blocks (null or empty branch)
                    if (string.IsNullOrWhiteSpace(block.Branch))
                    {
                        block.AssignedTeacher = "Branch not specified";
                        block.AssignedTeacherDepartment = "-";
                        continue;
                    }

                    // Randomize eligible teachers
                    var eligibleTeachers = availableTeachers
                        .Where(t => !string.Equals(t.Department?.Trim(), block.Branch?.Trim(), StringComparison.OrdinalIgnoreCase))
                        .OrderBy(_ => rng.Next()) // <- Random shuffle
                        .ToList();

                    if (eligibleTeachers.Any())
                    {
                        var selected = eligibleTeachers.First();
                        block.AssignedTeacher = selected.Name;
                        block.AssignedTeacherDepartment = selected.Department;

                        availableTeachers.Remove(selected);

                        // Refill teacher pool if exhausted
                        if (availableTeachers.Count == 0)
                            availableTeachers = new List<Teacher>(teachers);
                    }
                    else
                    {
                        block.AssignedTeacher = "No eligible teacher";
                        block.AssignedTeacherDepartment = "-";
                    }

                    // Write to Excel
                    sheet.Range[$"A{row}"].Number = block.BlockNumber;
                    sheet.Range[$"D{row}"].Text = block.Subject;
                    sheet.Range[$"E{row}"].Text = block.Branch;
                    sheet.Range[$"F{row}"].Text = block.AssignedTeacher;
                    sheet.Range[$"G{row}"].Text = block.AssignedTeacherDepartment;
                    row++;
                }

                // Save to memory stream
                MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }
        public MemoryStream GenerateNoticeBoardReport(
    List<BlockData> blocks,
    List<SavedExamDetails> savedExamDetailsList,
    SummaryDetails summaryDetails)
        {
            using ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet sheet = workbook.Worksheets[0];

            // Date and Time
            DateTime dateTimeNormalized = Convert.ToDateTime(summaryDetails.ExamDate);
            string examDate = dateTimeNormalized.ToString("dddd dd-MM-yyyy");
            string examTime = $"{summaryDetails.StartTime:hh\\:mm} TO {summaryDetails.EndTime:hh\\:mm}";

            // Title
            sheet.Range["A1:G1"].Merge();
            sheet.Range["A1"].Text = $"SPPU NOV./DEC. 2024 THEORY EXAMINATION, Date : {examDate}, Time : {examTime}.";
            sheet.Range["A1"].CellStyle.Font.Bold = true;
            sheet.Range["A1"].CellStyle.Font.Size = 14;
            sheet.Range["A1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;

            // Headers
            string[] headers = {
        "YEAR, BRANCH", "SUBJECT WITH CODE", "BLOCK NO", "ROOM NO", "FLOOR", "SEAT NO. FROM", "SEAT NO. TO"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Range[2, i + 1].Text = headers[i];
                sheet.Range[2, i + 1].CellStyle.Font.Bold = true;
                sheet.Range[2, i + 1].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            int row = 3;

            // Filter out incomplete blocks and group by subject
            var subjectGroups = blocks
                .Where(b => !string.IsNullOrWhiteSpace(b.AssignedTeacher) &&
                            !string.IsNullOrWhiteSpace(b.Subject) &&
                            !string.IsNullOrWhiteSpace(b.StartingSeatNumber) &&
                            !string.IsNullOrWhiteSpace(b.EndingSeatNumber))
                .GroupBy(b => b.Subject);

            foreach (var subjectGroup in subjectGroups)
            {
                int groupStartRow = row;
                int groupRowCount = subjectGroup.Count();

                // Get savedExamDetails that matches subject
                var matchingDetails = savedExamDetailsList
                    .FirstOrDefault(s => s.Subject.Equals(subjectGroup.Key, StringComparison.OrdinalIgnoreCase));

                string yearBranch = matchingDetails != null
                    ? $"{matchingDetails.ExamYear}, {matchingDetails.ExamBranch}"
                    : "UNKNOWN";

                foreach (var block in subjectGroup)
                {
                    sheet.Range[$"A{row}"].Text = yearBranch;
                    sheet.Range[$"C{row}"].Number = block.BlockNumber;
                    sheet.Range[$"D{row}"].Text = block.RoomNo ?? "N/A";
                    sheet.Range[$"E{row}"].Text = block.BlockFloor.ToString();
                    sheet.Range[$"F{row}"].Text = block.StartingSeatNumber ?? "N/A";
                    sheet.Range[$"G{row}"].Text = block.EndingSeatNumber ?? "N/A";

                    row++;
                }

                // Merge subject cell vertically across all rows in this group
                string mergeRange = $"B{groupStartRow}:B{groupStartRow + groupRowCount - 1}";
                sheet.Range[mergeRange].Merge();
                sheet.Range[$"B{groupStartRow}"].Text = subjectGroup.Key;
                sheet.Range[mergeRange].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[mergeRange].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[mergeRange].CellStyle.Font.Bold = true;
            }

            // Borders only for the entire filled table (excluding merged titles)
            string fullTableRange = $"A2:G{row - 1}";
            sheet.Range[fullTableRange].CellStyle.Borders.LineStyle = ExcelLineStyle.Thin;

            // Auto fit
            sheet.UsedRange.AutofitColumns();

            // Save to memory stream
            using MemoryStream stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }




        public MemoryStream CreateDocument(List<SavedExamDetails> savedExamDetails, int totalStudents, SummaryDetails summaryDetails, List<BlockData> blockDatas)
        {
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;

                IWorkbook workbook = application.Workbooks.Create(1);
                int maxStudentsPerBlock = summaryDetails.MaxStudentsPerBlock;
                int globalBlockCounter = summaryDetails.StartBlock;
                int MaxBlockNumber = 35; // <<< wrap around if exceeded

                foreach (var exam in savedExamDetails)
                {
                    string year = exam.ExamYear;
                    string branch = exam.ExamBranch;
                    string subject = exam.Subject;
                    List<string> seatNumbers = exam.SavedExtractedSeatNumbers;

                    DateTime dateTimeNormalized = Convert.ToDateTime(summaryDetails.ExamDate);
                    string dayOfExam = dateTimeNormalized.DayOfWeek.ToString();
                    string dateOfExam = dateTimeNormalized.ToString("MM/dd/yyyy");
                    TimeSpan? startTimeOfExam = summaryDetails.StartTime;
                    TimeSpan? endTimeOfExam = summaryDetails.EndTime;

                    int studentCount = seatNumbers.Count;
                    int blockCount = (int)Math.Ceiling((double)studentCount / maxStudentsPerBlock);

                    for (int localBlock = 0; localBlock < blockCount; localBlock++)
                    {
                        int startIndex = localBlock * maxStudentsPerBlock;
                        int endIndex = Math.Min(startIndex + maxStudentsPerBlock, studentCount);

                        // Create worksheet for this block
                        IWorksheet sheet = workbook.Worksheets.Create();
                        sheet.Range["A1:C1"].Merge();
                        sheet.Range["A1"].Text = $"Block Number: {globalBlockCounter}";
                        sheet.Range["A2:C2"].Merge();
                        sheet.Range["A2"].Text = $"Day & Date: {dayOfExam} {dateOfExam}";
                        sheet.Range["A3:C3"].Merge();
                        sheet.Range["A3"].Text = $"Time: {startTimeOfExam} TO {endTimeOfExam}";
                        sheet.Range["A4:C4"].Merge();
                        sheet.Range["A4"].Text = $"{year} {branch} 2019 PATTERN";
                        sheet.Range["A5:C5"].Merge();
                        sheet.Range["A5"].Text = $"{subject}";

                        sheet.Range["A1"].ColumnWidth = 36;
                        sheet.Range["A1:C5"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range["A1:C5"].CellStyle.Font.Bold = true;
                        sheet.Range["A4:C4"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(220, 220, 220);
                        sheet.Range["A5:C5"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(220, 220, 220);

                        sheet.Range["A7"].Text = "Serial No.";
                        sheet.Range["B7"].Text = "Bench No.";
                        sheet.Range["C7"].Text = "Seat Numbers";
                        sheet.Range["A7:C7"].CellStyle.Font.Bold = true;

                        int row = 8;
                        for (int i = startIndex; i < endIndex; i++)
                        {
                            sheet.Range[$"C{row}"].Text = seatNumbers[i];
                            int serial = (i - startIndex) + 1;
                            sheet.Range[$"A{row}"].Number = serial;
                            sheet.Range[$"B{row}"].Number = serial;
                            row++;
                        }

                        // 🧠 Update BlockData using block number
                        var blockToUpdate = blockDatas.FirstOrDefault(b => b.BlockNumber == globalBlockCounter);
                        if (blockToUpdate != null)
                        {
                            blockToUpdate.Subject = subject;
                            blockToUpdate.Branch = branch;
                            blockToUpdate.StartingSeatNumber = seatNumbers[startIndex];
                            blockToUpdate.EndingSeatNumber = seatNumbers[endIndex - 1];
                            // Optional: Save to DB here if you're ready
                            // await _dbContext.SaveChangesAsync();
                        }

                        globalBlockCounter++;

                        if (globalBlockCounter > 35)
                            globalBlockCounter = 1; // wrap around if max block reached
                    }

                }

                // Convert workbook to PDF and return stream
                MemoryStream pdfStream = new MemoryStream();
                XlsIORenderer renderer = new XlsIORenderer();
                PdfDocument document = renderer.ConvertToPDF(workbook);
                document.Save(pdfStream);
                document.Close(true);
                pdfStream.Position = 0;
                return pdfStream;
            }
        }




        public List<Teacher> ExtractAssistantProfessors(string filePath)
        {
            List<Teacher> associateProfessors = new();

            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;

                using FileStream inputStream = new(filePath, FileMode.Open, FileAccess.Read);
                IWorkbook workbook = application.Workbooks.Open(inputStream);
                IWorksheet worksheet = workbook.Worksheets[0];

                int startRow = 2; // Assuming headers on row 1
                int lastRow = worksheet.UsedRange.LastRow;

                for (int row = startRow; row <= lastRow; row++)
                {
                    string srNoText = worksheet[row, 1].DisplayText?.Trim();
                    string name = worksheet[row, 2].DisplayText?.Trim();
                    string dept = worksheet[row, 3].DisplayText?.Trim();
                    string designation = worksheet[row, 4].DisplayText?.Trim();

                    if (!string.IsNullOrWhiteSpace(name) &&
                        designation.Equals("Assistant Professor", StringComparison.OrdinalIgnoreCase))
                    {
                        associateProfessors.Add(new Teacher
                        {
                            SrNo = int.TryParse(srNoText, out int sn) ? sn : 0,
                            Name = name,
                            Department = dept,
                            Designation = designation
                        });
                    }
                }
            }

            return associateProfessors;
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
                string NormalizeKey(string rawKey)
                {
                    if (string.IsNullOrWhiteSpace(rawKey)) return "";

                    // Collapse multiple spaces → single space
                    string collapsed = System.Text.RegularExpressions.Regex.Replace(rawKey, @"\s+", " ");

                    // Remove leading/trailing whitespace and make consistent casing (optional)
                    return collapsed.Trim();
                }
                string subjectKey = NormalizeKey($"{subject.Code}_{subject.Name}_{subject.Type}");

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