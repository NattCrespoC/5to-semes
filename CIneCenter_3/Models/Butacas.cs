using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_MVC_Prueba.Models
{
    public class Butacas
    {
        public Butacas()
        {
            // Initialize collections to avoid null reference exceptions
            Tickets = new HashSet<Tickets>();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Butacas { get; set; }
        
        [Required]
        public int SalaId { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Numero { get; set; }
        
        // Navigation properties
        [ForeignKey("SalaId")]
        public virtual Salas Sala { get; set; }

        public virtual ICollection<Tickets> Tickets { get; set; }
    }
}
