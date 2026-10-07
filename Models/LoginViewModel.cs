using System.ComponentModel.DataAnnotations;

namespace Kt20Chat.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [StringLength(30, ErrorMessage = "Имя не должно быть длиннее 30 символов")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, ErrorMessage = "Пароль не должен быть длиннее 100 символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
