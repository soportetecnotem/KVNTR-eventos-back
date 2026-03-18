namespace EventosBack.Models
{
    public class DatosValidacionEventos
    {
        public long Id { get; set; }
        public string Agente { get; set; } = string.Empty;
        public string Nick { get; set; } = string.Empty;
        public string Tipo_Usuario { get; set; } = string.Empty;
        public string Cod_Perfil { get; set; } = string.Empty;
        public string Nombre_Perfil { get; set; } = string.Empty;
        public string Perfil_Id { get; set; } = string.Empty;
        public string Evento { get; set; } = string.Empty;
        public string Evento_Id { get; set; } = string.Empty;
        public byte[] Imagen { get; set; } = [];
    }
}
