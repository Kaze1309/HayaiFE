using HayaiFE.Models;

namespace HayaiFE.Services
{
    public interface IYearBranchSubjectService
    {
        Task<List<Year>> GetYearsAsync();
        Task<List<Branch>> GetBranchesByYearAsync(int yearId);
        Task<List<Subject>> GetSubjectByBranchesAndYearAsync(int yearId , int branchId);
    }
}
