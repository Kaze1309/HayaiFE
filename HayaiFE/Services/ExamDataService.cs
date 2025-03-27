using HayaiFE.Data;

public class ExamDataService
{
    public List<CreateExcel.ExamDetails> ExamDetailsList { get; private set; } = new();

    public void SetExamDetails(List<CreateExcel.ExamDetails> examDetails)
    {
        ExamDetailsList = examDetails;
    }

    public List<CreateExcel.ExamDetails> GetExamDetails()
    {
        return ExamDetailsList;
    }
}
