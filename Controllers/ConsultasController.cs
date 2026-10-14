using System;
using System.Linq;
using System.Web.Http;
using TurneroDentalApi.Data;
using TurneroDentalApi.Models;

namespace TurneroDentalApi.Controllers
{
    // Mensajes enviados desde el formulario de contacto de la página de inicio.
    [RoutePrefix("api/consultas")]
    public class ConsultasController : ApiController
    {
        private TurneroDentalContext db = new TurneroDentalContext();

        [HttpGet, Route("")]
        public IHttpActionResult GetTodas() =>
            Ok(db.Consultas.OrderByDescending(c => c.FechaEnvio).ToList());

        [HttpPost, Route("")]
        public IHttpActionResult Crear(Consulta consulta)
        {
            if (consulta == null) return BadRequest("Completá el formulario.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            consulta.IdConsulta = 0;
            consulta.FechaEnvio = DateTime.Now;
            consulta.Leida = false;
            db.Consultas.Add(consulta);
            db.SaveChanges();
            return Ok(new { consulta.IdConsulta });
        }
    }
}
