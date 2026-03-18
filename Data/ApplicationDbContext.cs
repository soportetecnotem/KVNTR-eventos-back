using EventosBack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EventosBack.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Categoria_Actividades> Cat_Actividades { get; set; }
        public DbSet<Categoria_Recomendacion> Cat_Recomendaciones { get; set; }
        public DbSet<CategoriaUsuario> Cat_Usuarios { get; set; }
        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<ActividadUsuario> UsuariosActividades { get; set; }
        public DbSet<Hotel> Hoteles { get; set; }
        public DbSet<HotelUsuario> UsuariosHoteles { get; set; }
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<PerfilConvencionista> PerfilConvencionistas { get; set; }
        public DbSet<Recomendacion> Recomendaciones { get; set; }
        public DbSet<Respuesta> Respuestas { get; set; }
        public DbSet<Convencionista> Convencionistas { get; set; }
        public DbSet<Vuelo> Vuelos { get; set; }
        public DbSet<VueloUsuario> UsuariosVuelos { get; set; }
        public DbSet<Version_App> Versiones_App { get; set; }
        public DbSet<DeviceInfo> DevicesInfo { get; set; }
        public DbSet<AuthMicrosoft> MicrosoftOAuth2 { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Error> Errores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Convencionista>()
                .HasIndex(c => c.Clave)
                .IsUnique();

            // Aplicar DeleteBehavior.Restrict a TODAS las relaciones
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
