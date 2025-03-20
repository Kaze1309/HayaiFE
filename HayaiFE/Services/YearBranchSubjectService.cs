using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using HayaiFE.Models;

namespace HayaiFE.Services
{
    public class YearBranchSubjectService : IYearBranchSubjectService
    {


        private List<Year> years = new()
        {
            new Year{ YearId = 1, YearName = "FE"},
            new Year{ YearId= 2, YearName = "SE" },
            new Year{ YearId = 3, YearName = "TE"},
            new Year{ YearId = 4, YearName = "BE"}
        };

        private List<Branch> branches = new()
        {
            new Branch{ branchId = -1, YearId =  1, branchName = "NA"},

            new Branch{ branchId= 1, YearId = 2, branchName = "Computer"},
            new Branch{ branchId = 2, YearId = 2, branchName = "AIDS" },
            new Branch{ branchId = 3, YearId = 2, branchName = "E & TC"},
            new Branch{ branchId = 4, YearId = 2, branchName = "Electrical"},

            new Branch{ branchId= 1, YearId = 3, branchName = "Computer"},
            new Branch{ branchId = 2, YearId = 3, branchName = "AIDS" },
            new Branch{ branchId = 3, YearId = 3, branchName = "E & TC"},
            new Branch{ branchId = 4, YearId = 3, branchName = "Electrical"},

            new Branch{ branchId= 1, YearId = 4, branchName = "Computer"},
            new Branch{ branchId = 2, YearId = 4, branchName = "AIDS" },
            new Branch{ branchId = 3, YearId = 4, branchName = "E & TC"},
            new Branch{ branchId = 4, YearId = 4, branchName = "Electrical"},
        };

        private List<Subject> subjects = new()
        {
            new Subject { branchId = -1, YearId = 1, subjectId = 101 , subjectName ="Engineering Mathematics- I" },
            new Subject { branchId = -1, YearId = 1, subjectId = 102 , subjectName = "Engineering Physics"},

            new Subject { branchId = 1, YearId = 2, subjectId = 211 , subjectName = "Fundamentals of Data Structures"},
            new Subject { branchId = 2, YearId = 2, subjectId = 221 , subjectName = "Software Engineering"},

            new Subject { branchId = 1, YearId = 3, subjectId = 311 , subjectName = "Database Management System"},
            new Subject { branchId = 2, YearId = 3, subjectId = 321 , subjectName = "Computer Networks"}
        };

        public Task<List<Year>> GetYearsAsync()
        {
            return Task.FromResult(years);
        }

        public Task<List<Branch>> GetBranchesByYearAsync(int yearId)
        {
            var filteredBranches = branches.Where(b => b.YearId == yearId).ToList();
            return Task.FromResult(filteredBranches);
        }

        public Task<List<Subject>> GetSubjectByBranchesAndYearAsync(int yearId, int branchId)
        {
            var filteredSubjects = subjects.Where(s => s.YearId == yearId && s.branchId == branchId).ToList();
            return Task.FromResult(filteredSubjects);
        }


    };


}

