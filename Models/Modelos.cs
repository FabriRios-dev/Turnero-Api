using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurneroDentalApi.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
        public string Rol { get; set; }
    }

    [Table("Especialidad")]
    public class Especialidad
    {
        [Key]
        public int IdEspecialidad { get; set; }
        public string Nombre { get; set; }
    }

    [Table("Profesional")]
    public class Profesional
    {
        [Key]
        public int IdProfesional { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Matricula { get; set; }
        public int IdEspecialidad { get; set; }

        [ForeignKey("IdEspecialidad")]
        public virtual Especialidad Especialidad { get; set; }
    }

    [Table("Paciente")]
    public class Paciente
    {
        [Key]
        public int IdPaciente { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public int? IdUsuario { get; set; }
    }

    [Table("Turno")]
    public class Turno
    {
        [Key]
        public int IdTurno { get; set; }
        public System.DateTime Fecha { get; set; }
        public string Hora { get; set; }
        public string Estado { get; set; }
        public int IdPaciente { get; set; }
        public int IdProfesional { get; set; }

        [ForeignKey("IdPaciente")]
        public virtual Paciente Paciente { get; set; }

        [ForeignKey("IdProfesional")]
        public virtual Profesional Profesional { get; set; }
    }

    [Table("HorarioAtencion")]
    public class HorarioAtencion
    {
        [Key]
        public int IdHorario { get; set; }
        public int IdProfesional { get; set; }
        public byte DiaSemana { get; set; }          // 0 = domingo ... 6 = sábado
        public string HoraInicio { get; set; }       // HH:mm
        public string HoraFin { get; set; }
        public int DuracionMinutos { get; set; }
    }

    [Table("Consulta")]
    public class Consulta
    {
        [Key]
        public int IdConsulta { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(100), EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [Required, StringLength(1000)]
        public string Mensaje { get; set; }

        public System.DateTime FechaEnvio { get; set; }
        public bool Leida { get; set; }
    }
}