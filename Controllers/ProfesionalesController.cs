using System.Linq;
using System.Web.Http;
using TurneroDentalApi.Data;
using TurneroDentalApi.Models;

namespace TurneroDentalApi.Controllers
{
    [RoutePrefix("api/profesionales")]
    public class ProfesionalesController : ApiController
    {
        private TurneroDentalContext db = new TurneroDentalContext();

        [HttpGet, Route("")]
        public IHttpActionResult GetTodos()
        {
            var profesionales = db.Profesionales
                .Select(p => new {
                    p.IdProfesional,
                    p.Nombre,
                    p.Apellido,
                    p.Matricula,
                    p.IdEspecialidad,
                    Especialidad = p.Especialidad.Nombre
                }).ToList();
            return Ok(profesionales);
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetPorId(int id)
        {
            var profesional = db.Profesionales.Find(id);
            if (profesional == null) return NotFound();
            return Ok(profesional);
        }

        [HttpPost, Route("")]
        public IHttpActionResult Crear(Profesional profesional)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            db.Profesionales.Add(profesional);
            db.SaveChanges();
            return Ok(profesional);
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Actualizar(int id, Profesional profesional)
        {
            var existente = db.Profesionales.Find(id);
            if (existente == null) return NotFound();

            existente.Nombre = profesional.Nombre;
            existente.Apellido = profesional.Apellido;
            existente.Matricula = profesional.Matricula;
            existente.IdEspecialidad = profesional.IdEspecialidad;

            db.SaveChanges();
            return Ok(existente);
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Eliminar(int id)
        {
            var existente = db.Profesionales.Find(id);
            if (existente == null) return NotFound();
            db.Profesionales.Remove(existente);
            db.SaveChanges();
            return Ok();
        }

        [HttpGet, Route("especialidades")]
        public IHttpActionResult GetEspecialidades() => Ok(db.Especialidades.ToList());
    }
}