using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using TurneroDentalApi.Data;
using TurneroDentalApi.Models;

namespace TurneroDentalApi.Controllers
{
    [RoutePrefix("api/turnos")]
    public class TurnosController : ApiController
    {
        private TurneroDentalContext db = new TurneroDentalContext();

        // Hasta cuántos días hacia adelante se pueden sacar turnos.
        private const int DiasMaximosAnticipacion = 60;

        [HttpGet, Route("")]
        public IHttpActionResult GetTodos()
        {
            var turnos = db.Turnos
                .Select(t => new {
                    t.IdTurno,
                    t.Fecha,
                    t.Hora,
                    t.Estado,
                    Paciente = t.Paciente.Nombre + " " + t.Paciente.Apellido,
                    Profesional = t.Profesional.Nombre + " " + t.Profesional.Apellido
                }).ToList();
            return Ok(turnos);
        }

        // Turnos de un paciente (pantalla "Mis turnos").
        [HttpGet, Route("paciente/{idPaciente:int}")]
        public IHttpActionResult GetPorPaciente(int idPaciente)
        {
            var turnos = db.Turnos
                .Where(t => t.IdPaciente == idPaciente)
                .OrderByDescending(t => t.Fecha).ThenByDescending(t => t.Hora)
                .Select(t => new {
                    t.IdTurno,
                    t.Fecha,
                    t.Hora,
                    t.Estado,
                    Profesional = t.Profesional.Nombre + " " + t.Profesional.Apellido,
                    Especialidad = t.Profesional.Especialidad.Nombre
                }).ToList();
            return Ok(turnos);
        }

        // Profesionales que atienden en la fecha indicada (yyyy-MM-dd) con sus horarios libres.
        [HttpGet, Route("disponibilidad")]
        public IHttpActionResult GetDisponibilidad(string fecha)
        {
            DateTime dia;
            if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dia))
                return BadRequest("La fecha debe tener el formato yyyy-MM-dd.");

            var errorFecha = ValidarFecha(dia);
            if (errorFecha != null) return BadRequest(errorFecha);

            var diaSemana = (byte)dia.DayOfWeek;
            var horarios = db.HorariosAtencion.Where(h => h.DiaSemana == diaSemana).ToList();
            var idsProfesionales = horarios.Select(h => h.IdProfesional).Distinct().ToList();

            var profesionales = db.Profesionales
                .Where(p => idsProfesionales.Contains(p.IdProfesional))
                .Select(p => new { p.IdProfesional, p.Nombre, p.Apellido, Especialidad = p.Especialidad.Nombre })
                .ToList();

            var ocupados = db.Turnos
                .Where(t => t.Fecha == dia && t.Estado != "Cancelado")
                .Select(t => new { t.IdProfesional, t.Hora })
                .ToList();

            var resultado = profesionales
                .Select(p => new {
                    p.IdProfesional,
                    p.Nombre,
                    p.Apellido,
                    p.Especialidad,
                    Horarios = HorariosDelDia(horarios.Where(h => h.IdProfesional == p.IdProfesional), dia)
                        .Where(hora => !ocupados.Any(o => o.IdProfesional == p.IdProfesional && o.Hora == hora))
                        .ToList()
                })
                .OrderBy(p => p.Apellido)
                .ToList();

            return Ok(resultado);
        }

        [HttpPost, Route("reservar")]
        public IHttpActionResult Reservar(Turno nuevoTurno)
        {
            if (nuevoTurno == null) return BadRequest("Datos del turno inválidos.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dia = nuevoTurno.Fecha.Date;
            var errorFecha = ValidarFecha(dia);
            if (errorFecha != null) return BadRequest(errorFecha);

            if (!db.Pacientes.Any(p => p.IdPaciente == nuevoTurno.IdPaciente))
                return BadRequest("El paciente no existe.");

            // El horario tiene que corresponder a la agenda del profesional para ese día.
            var diaSemana = (byte)dia.DayOfWeek;
            var horarios = db.HorariosAtencion
                .Where(h => h.IdProfesional == nuevoTurno.IdProfesional && h.DiaSemana == diaSemana)
                .ToList();
            if (!HorariosDelDia(horarios, dia).Contains(nuevoTurno.Hora))
                return BadRequest("El profesional no atiende en ese horario.");

            bool horarioOcupado = db.Turnos.Any(t =>
                t.IdProfesional == nuevoTurno.IdProfesional &&
                t.Fecha == dia &&
                t.Hora == nuevoTurno.Hora &&
                t.Estado != "Cancelado");

            if (horarioOcupado)
                return BadRequest("Ese horario ya está reservado para el profesional elegido.");

            bool pacienteOcupado = db.Turnos.Any(t =>
                t.IdPaciente == nuevoTurno.IdPaciente &&
                t.Fecha == dia &&
                t.Hora == nuevoTurno.Hora &&
                t.Estado != "Cancelado");

            if (pacienteOcupado)
                return BadRequest("Ya tenés otro turno reservado en ese mismo horario.");

            nuevoTurno.Fecha = dia;
            nuevoTurno.Estado = "Confirmado";
            db.Turnos.Add(nuevoTurno);
            db.SaveChanges();
            return Ok(new { nuevoTurno.IdTurno, nuevoTurno.Fecha, nuevoTurno.Hora, nuevoTurno.Estado });
        }

        [HttpPut, Route("{id:int}/cancelar")]
        public IHttpActionResult Cancelar(int id)
        {
            var turno = db.Turnos.Find(id);
            if (turno == null) return NotFound();
            turno.Estado = "Cancelado";
            db.SaveChanges();
            return Ok(new { turno.IdTurno, turno.Estado });
        }

        private static string ValidarFecha(DateTime dia)
        {
            if (dia.Date < DateTime.Today) return "No se pueden sacar turnos para fechas pasadas.";
            if (dia.Date > DateTime.Today.AddDays(DiasMaximosAnticipacion))
                return "Solo se pueden sacar turnos con hasta " + DiasMaximosAnticipacion + " días de anticipación.";
            return null;
        }

        // Arma los horarios ("08:00", "08:30"...) según la agenda; si la fecha es hoy, omite los que ya pasaron.
        private static IEnumerable<string> HorariosDelDia(IEnumerable<HorarioAtencion> agenda, DateTime dia)
        {
            var horas = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var h in agenda)
            {
                TimeSpan inicio, fin;
                if (!TimeSpan.TryParse(h.HoraInicio, out inicio) || !TimeSpan.TryParse(h.HoraFin, out fin)) continue;
                if (h.DuracionMinutos <= 0) continue;

                var duracion = TimeSpan.FromMinutes(h.DuracionMinutos);
                for (var hora = inicio; hora + duracion <= fin; hora += duracion)
                {
                    if (dia.Date == DateTime.Today && dia.Date + hora <= DateTime.Now) continue;
                    horas.Add(hora.ToString(@"hh\:mm"));
                }
            }
            return horas;
        }
    }
}
