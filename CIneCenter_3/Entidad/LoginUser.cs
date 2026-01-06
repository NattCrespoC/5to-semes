using System.ComponentModel.DataAnnotations;

namespace ASP_MVC_Prueba.Entidad
{
    public class LoginUser
    {
        public LoginUser() { }
        
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string PasswordHash { get; set; }
    }
}
