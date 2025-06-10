using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HayaiFE.Models;
using HayaiFE.Data;
namespace WhiteBoxTesting
{
    public class AssignTeacherCorrectly
    {
        [Fact]
        public void FinalReport_AssignsTeacherCorrectly()
        {
            // Arrange
            var blocks = new List<BlockData>
    {
        new() { BlockNumber = 1, Branch = "Computer", Subject = "AI" }
    };
            var teachers = new List<Teacher>
    {
        new() { Name = "John", Department = "Mechanical", Designation = "Assistant Professor" }
    };
            var service = new CreateExcel();

            // Act
            var stream = service.FinalReport(blocks, teachers);

            // Assert
            Assert.Equal("John", blocks[0].AssignedTeacher);
        }

    }
}
