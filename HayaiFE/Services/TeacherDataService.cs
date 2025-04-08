using HayaiFE.Models;

namespace HayaiFE.Services
{
    public class TeacherDataService
    {
        public List<Teacher> ExtractedTeachersData { get; private set; } = new();

        public void SaveTeacherData(List<Teacher> teacherDetails)
        {
            ExtractedTeachersData = teacherDetails;
        }

        public List<Teacher> GetTeacherData()
        {
            return ExtractedTeachersData;
        }
    }
}
