using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_MVC_Prueba.Models
{
    public class Horarios
    {
        public Horarios()
        {
            Tickets = new HashSet<Tickets>();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Horarios { get; set; }
        
        [Required]
        public int SalaId { get; set; }
        
        [Required]
        public int PeliculaId { get; set; }
        
        [Required]
        public DateTime FechaHora { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }
        
        // Navigation properties
        [ForeignKey("SalaId")]
        public virtual Salas Sala { get; set; }
        
        [ForeignKey("PeliculaId")]
        public virtual Peliculas Pelicula { get; set; }
        
        public virtual ICollection<Tickets> Tickets { get; set; }
    }
}
