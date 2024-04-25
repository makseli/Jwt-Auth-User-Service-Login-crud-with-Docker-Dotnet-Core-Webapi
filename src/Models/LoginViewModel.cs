using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "User Email required!")]
    public string email { get; set; }

    [Required(ErrorMessage = "Password required!")]
    public string password { get; set; }

}
