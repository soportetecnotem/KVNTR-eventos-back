namespace EventosBack.DTO
{
    public class ModuloDTO
    {
        public string MasterKey { get; set; } = null!;
        public string KeyCode { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public bool Status { get; set; }
        public int IDTypeResp { get; set; }
        public string Message { get; set; } = null!;
        public DateTime FStart { get; set; }
        public DateTime FStop { get; set; }
        public bool StatusPopUp { get; set; }
        public int IDTypeRespPopUp { get; set; }
        public string MessagePopUp { get; set; } = null!;
    }
}
