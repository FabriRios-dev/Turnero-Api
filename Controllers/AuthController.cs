using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using TurneroDentalApi.Data;
using TurneroDentalApi.Helpers;
using TurneroDentalApi.Models;

namespace TurneroDentalApi.Controllers
{
    public class LoginRequest
    {
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
    }

    public class RegistroRequest
    {
        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(100)]
        public string Apellido { get; set; }

        [Required, StringLength(15)]
        public string DNI { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [Required, StringLength(100), EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(50, MinimumLength = 4)]
        public string NombreUsuario { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public string Contrasena { get; set; }
    }

    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private TurneroDentalContext db = new TurneroDentalContext();

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            if (request == null) return BadRequest("Datos de inicio de sesión inválidos.");

            var usuario = db.Usuarios.FirstOrDefault(u => u.NombreUsuario == request.NombreUsuario);
            if (usuario == null || !PasswordHasher.Verificar(request.Contrasena, usuario.Contrasena))
                return Unauthorized();

            // Usuarios viejos con contraseña en texto plano: se migran a hash al iniciar sesión.
            if (!PasswordHasher.EsHash(usuario.Contrasena))
            {
                usuario.Contrasena = PasswordHasher.Hashear(request.Contrasena);
                db.SaveChanges();
            }

            return Ok(Sesion(usuario));
        }

        [HttpPost]
        [Route("registro")]
        public IHttpActionResult Registro(RegistroRequest request)
        {
            if (request == null) return BadRequest("Completá todos los datos.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var nombreUsuario = request.NombreUsuario.Trim();
            var dni = request.DNI.Trim();

            if (db.Usuarios.Any(u => u.NombreUsuario == nombreUsuario))
                return BadRequest("Ese nombre de usuario ya está en uso.");

            // Si el paciente ya estaba cargado por la clínica (sin cuenta), se le asocia la cuenta nueva.
            var paciente = db.Pacientes.FirstOrDefault(p => p.DNI == dni);
            if (paciente != null && paciente.IdUsuario != null)
                return BadRequest("Ya existe una cuenta registrada con ese DNI.");

            using (var transaccion = db.Database.BeginTransaction())
            {
                var usuario = new Usuario
                {
                    NombreUsuario = nombreUsuario,
                    Contrasena = PasswordHasher.Hashear(request.Contrasena),
                    Rol = "Paciente"
                };
                db.Usuarios.Add(usuario);
                db.SaveChanges();

                if (paciente == null)
                {
                    paciente = new Paciente { DNI = dni };
                    db.Pacientes.Add(paciente);
                }
                paciente.Nombre = request.Nombre.Trim();
                paciente.Apellido = request.Apellido.Trim();
                paciente.Telefono = request.Telefono?.Trim();
                paciente.Email = request.Email.Trim();
                paciente.IdUsuario = usuario.IdUsuario;
                db.SaveChanges();

                transaccion.Commit();
                return Ok(Sesion(usuario));
            }
        }

        // Datos que el frontend guarda como sesión del usuario.
        private object Sesion(Usuario usuario)
        {
            var paciente = db.Pacientes.FirstOrDefault(p => p.IdUsuario == usuario.IdUsuario);
            return new
            {
                usuario.IdUsuario,
                usuario.NombreUsuario,
                usuario.Rol,
                IdPaciente = paciente?.IdPaciente,
                Nombre = paciente != null ? paciente.Nombre + " " + paciente.Apellido : usuario.NombreUsuario
            };
        }
    }
}
