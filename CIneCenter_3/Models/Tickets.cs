using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_MVC_Prueba.Models
{
    public class Tickets
    {
        public Tickets()
        {
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Tickets { get; set; }
        
        [Required]
        public int HorarioId { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        [Required]
        public int ButacaId { get; set; }
        
        public DateTime? FechaCompra { get; set; }
        
        // Navigation properties
        [ForeignKey("HorarioId")]
        public virtual Horarios Horario { get; set; }
        
        [ForeignKey("UsuarioId")]
        public virtual Usuarios Usuario { get; set; }

        [ForeignKey("ButacaId")]
        public virtual Butacas Butaca { get; set; }

        public virtual Facturas Facturas { get; set; }
        public virtual Pagos Pagos { get; set; }
    }
}
