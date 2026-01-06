namespace ASP_MVC_Prueba.Entidad
{
    public class CreateTickets
    {
        public CreateTickets() { }
        public CreateTickets( int idHorario, int idUsuario, DateTime fechaCompra, IEnumerable<int> idbutaca)
        {
            HorarioId = idHorario;
            UsuarioId = idUsuario;
            ButacaId = idbutaca;
            FechaCompra = fechaCompra;
        }
        public int HorarioId { get; set; }
        public int UsuarioId { get; set; }
        public IEnumerable<int> ButacaId { get; set; }
        public DateTime FechaCompra { get; set; }
    }
}
