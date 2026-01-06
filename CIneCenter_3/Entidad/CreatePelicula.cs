namespace ASP_MVC_Prueba.Entidad
{
    public class CreatePelicula
    {
        public int Id { get; set; }
        public string ImageBase64 { get; set; }
        public CreatePelicula(){}
        // Funcion para guardar la imagen en el servidor, recuperar el path y luego guardar en la base de datos
        public string guardarImagen()
        {
            if (string.IsNullOrEmpty(ImageBase64))
            {
                throw new ArgumentException("La imagen en formato base64 no puede estar vacía");
            }
            try
            {
                // Convert base64 string to byte array
                byte[] imageBytes = Convert.FromBase64String(ImageBase64);
                // Definir la ruta donde se guardarán las imágenes
                string directorio = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "peliculas");
                // Crear el directorio si no existe
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }
                string nombreArchivo = $"{Id}.jpg";
                // Ruta completa del archivo
                string rutaCompleta = Path.Combine(directorio, nombreArchivo);
                // Guardar la imagen
                File.WriteAllBytes(rutaCompleta, imageBytes);
                
                // Devolver la ruta relativa para uso en la aplicación web
                return nombreArchivo;
            }
            catch (FormatException)
            {
                throw new FormatException("El formato de la imagen base64 no es válido");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar la imagen: {ex.Message}");
            }
        }
    }
}