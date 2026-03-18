namespace EventosBack.Models
{
    public class MS_Usuario
    {
        public int Id_Usuario { get; set; }
        public string Usuario { get; set; } = null!;
        public string Contrasena { get; set; } = null!;
        public string NCompleto { get; set; } = null!;
        public string FCreacion { get; set; } = null!;
        public string HCreacion { get; set; } = null!;
        public string Estatus { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string Servidor { get; set; } = null!;
        public string Catalogo { get; set; } = null!;
        public string Semilla { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string Catalogo_Autos { get; set; } = null!;
        public string Licencia { get; set; } = null!;
        public string Usuario_SICAS { get; set; } = null!;
        public string Contrasena_SICAS { get; set; } = null!;
        public string MSicas { get; set; } = null!;
        public string KeyEncryp { get; set; } = null!;
        public string TokenFB { get; set; } = null!;
    }
}
