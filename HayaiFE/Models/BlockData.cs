namespace HayaiFE.Models
{
    public class BlockData
    {
        public int Id { get; set; }
        public string? RoomNo { get; set; }
        public int BlockNumber { get; set; }
        public int BlockFloor { get; set; }

        public string? Subject { get; set; }
        public string? Branch { get; set; }
        public string? StartingSeatNumber { get; set; }
        // students
        public string? EndingSeatNumber { get; set; }
        public string? AssignedTeacher { get; set; }
        public string? AssignedTeacherDepartment { get; set; }
    }
}
