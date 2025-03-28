using HayaiFE.Data;
using HayaiFE.Models;

public class ExamDataService
{
    public List<CreateExcel.ExamDetails> ExamDetailsList { get; private set; } = new();
    public List<SavedExamDetails> SavedExams { get; private set; } = new();

    public void SetExamDetails(List<CreateExcel.ExamDetails> examDetails)
    {
        ExamDetailsList = examDetails;
    }

    public List<CreateExcel.ExamDetails> GetExamDetails()
    {
        return ExamDetailsList;
    }
    public void AddSavedExam(SavedExamDetails exam)
    {
        SavedExams.Add(exam);
    }

    public List<SavedExamDetails> GetSavedExams()
    {
        return SavedExams;
    }

}
