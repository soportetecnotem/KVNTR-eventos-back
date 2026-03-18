using EventosBack.Models;

namespace EventosBack.Repositories
{
    public interface IEventosRepository
    {
        Task<string> CrearEvento(Evento model);
    }
}