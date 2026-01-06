using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_MVC_Prueba.Models
{
    public class Salas
    {
        public Salas()
        {
            // Initialize collections to avoid null reference exceptions
            Butacas = new HashSet<Butacas>();
            Horarios = new HashSet<Horarios>();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Salas { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        
        [Required]
        public int Capacidad { get; set; }
        
        [StringLength(50)]
        public string Tipo { get; set; }
        
        // Navigation properties
        public virtual ICollection<Butacas> Butacas { get; set; }
        public virtual ICollection<Horarios> Horarios { get; set; }
    }
}
