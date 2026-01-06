using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_MVC_Prueba.Models
{
    public class Pagos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Pagos { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }

        public DateTime? FechaPago { get; set; }

        [StringLength(20)]
        public string Estado { get; set; }

        // Navigation property
        [ForeignKey("TicketId")]
        public virtual Tickets Ticket { get; set; }
    }
}
