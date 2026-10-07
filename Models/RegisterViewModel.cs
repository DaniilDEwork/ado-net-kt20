using System.ComponentModel.DataAnnotations;

namespace Kt20Chat.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [RegularExpression(@"^[\p{L}\p{Nd}_]{3,30}$", ErrorMessage = "Имя должно содержать от 3 до 30 букв, цифр или символов подчёркивания")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен содержать от 8 до 100 символов")]
    [RegularExpression(@"^(?=.*\p{L})(?=.*\p{Nd}).+$", ErrorMessage = "Пароль должен содержать буквы и цифры")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Подтвердите пароль")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}
