using System.ComponentModel.DataAnnotations;

namespace HayaiFE.Models.DTOs
{
    public class LoginViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Provide Username")]
        public string UserName { get; set; } = "";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Provide Password")]
        public string Password { get; set; } = "";

    }
}
