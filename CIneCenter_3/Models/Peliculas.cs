using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_MVC_Prueba.Models
{
    public class Peliculas
    {
        public Peliculas()
        {
            // Initialize collections to avoid null reference exceptions
            //Horarios = new HashSet<Horarios>();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Peliculas { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Titulo { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Genero { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Duracion { get; set; }
        
        [StringLength(1000)]
        public string Sinopsis { get; set; }
        //public string Portada { get; set; }

        [StringLength(200)]
        public string Imagen { get; set; }

        [StringLength(200)]
        public String Trailer { get; set; }

        [StringLength(50)]
        public string? Formatos { get; set; }
        
        public bool? Estreno { get; set; }

        public bool? Estado { get; set; }
        public virtual ICollection<Horarios> Horarios { get; set; }
    }
}
