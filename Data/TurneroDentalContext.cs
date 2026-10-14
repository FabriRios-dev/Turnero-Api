using System.Data.Entity;
using TurneroDentalApi.Models;

namespace TurneroDentalApi.Data
{
    public class TurneroDentalContext : DbContext
    {
        public TurneroDentalContext() : base("name=TurneroDentalConnection")
        {
            Database.SetInitializer<TurneroDentalContext>(null);
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<HorarioAtencion> HorariosAtencion { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
    }
}