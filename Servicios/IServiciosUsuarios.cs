using EventosBack.Models;

namespace EventosBack.Servicios
{
    public interface IServiciosUsuarios
    {
        Task<Usuario?> ObtenerUsuario();
        Task<Usuario?> ObtenerUsuario(string username);
    }
}