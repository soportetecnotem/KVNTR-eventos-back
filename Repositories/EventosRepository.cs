using EventosBack.Data;
using EventosBack.Models;

namespace EventosBack.Repositories
{
    public class EventosRepository : IEventosRepository
    {
        private readonly ApplicationDbContext _db;

        public EventosRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> CrearEvento(Evento model)
        {
            string mess;
            if (model != null)
            {
                _db.Add(model);
                await _db.SaveChangesAsync();
                mess = "Se ha guardado la información satisfactoriamente.";
            }
            else
            {
                mess = "El modelo esta vacío. Imposible Actualizar.";
            }
            return mess;
        }

    }
}
