// SummaryService.cs
using HayaiFE.Models;

public class SummaryService
{
    private SummaryDetails _summaryDetails = new SummaryDetails();

    public void SaveSummary(SummaryDetails summary)
    {
        _summaryDetails = summary;
    }

    public SummaryDetails GetSummary()
    {
        return _summaryDetails;
    }

    public void ClearSummary()
    {
        _summaryDetails = new SummaryDetails();
    }
}
