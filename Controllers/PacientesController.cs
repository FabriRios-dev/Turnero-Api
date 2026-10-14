using System.Linq;
using System.Web.Http;
using TurneroDentalApi.Data;
using TurneroDentalApi.Models;

namespace TurneroDentalApi.Controllers
{
    [RoutePrefix("api/pacientes")]
    public class PacientesController : ApiController
    {
        private TurneroDentalContext db = new TurneroDentalContext();

        [HttpGet, Route("")]
        public IHttpActionResult GetTodos() => Ok(db.Pacientes.ToList());

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetPorId(int id)
        {
            var paciente = db.Pacientes.Find(id);
            if (paciente == null) return NotFound();
            return Ok(paciente);
        }

        [HttpPost, Route("")]
        public IHttpActionResult Crear(Paciente paciente)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            db.Pacientes.Add(paciente);
            db.SaveChanges();
            return Ok(paciente);
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Actualizar(int id, Paciente paciente)
        {
            var existente = db.Pacientes.Find(id);
            if (existente == null) return NotFound();

            existente.Nombre = paciente.Nombre;
            existente.Apellido = paciente.Apellido;
            existente.DNI = paciente.DNI;
            existente.Telefono = paciente.Telefono;
            existente.Email = paciente.Email;

            db.SaveChanges();
            return Ok(existente);
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Eliminar(int id)
        {
            var existente = db.Pacientes.Find(id);
            if (existente == null) return NotFound();
            db.Pacientes.Remove(existente);
            db.SaveChanges();
            return Ok();
        }
    }
}