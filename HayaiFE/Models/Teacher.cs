using Syncfusion.XlsIO;
using System.ComponentModel;
namespace HayaiFE.Models
{
    public class Teacher
    {


        [DisplayNameAttribute("Name")]
        public string Name { get; set; }
        [DisplayNameAttribute("Department")]
        public string Department { get; set; }
        [DisplayNameAttribute("Designation")]
        public string Designation { get; set; }
        [DisplayNameAttribute("SrNo")]
        public int SrNo { get; set; }
        public Teacher()
        {

        }
    }

}
