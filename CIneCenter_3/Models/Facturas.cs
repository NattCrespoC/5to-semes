using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_MVC_Prueba.Models
{
    public class Facturas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Facturas { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(100)]
        public string CUF { get; set; }

        // Navigation property
        [ForeignKey("TicketId")]
        public virtual Tickets Ticket { get; set; }
    }
}
