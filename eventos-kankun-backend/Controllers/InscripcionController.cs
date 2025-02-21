using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosKankun.Models;
using EventosKankun.Data;

namespace EventosKankun.Controllers
{
    [Route("api/inscripciones")]
    [ApiController]
    public class InscripcionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InscripcionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/inscripciones
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Participante>), 200)]
        public async Task<ActionResult<IEnumerable<Participante>>> GetInscripciones()
        {
            return await _context.Participantes
                .Include(p => p.Usuario)
                .Include(p => p.Evento)
                .ToListAsync();
        }

        // GET: api/inscripciones/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Participante), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Participante>> GetInscripcion(int id)
        {
            var inscripcion = await _context.Participantes
                .Include(p => p.Usuario)
                .Include(p => p.Evento)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (inscripcion == null)
            {
                return NotFound("La inscripción no existe.");
            }

            return inscripcion;
        }

        // POST: api/inscripciones
        [HttpPost]
        [ProducesResponseType(typeof(Participante), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Participante>> CreateInscripcion(Participante participante)
        {
            // Verificar si el evento existe
            var evento = await _context.Eventos.FindAsync(participante.EventoID);
            if (evento == null)
            {
                return BadRequest("El evento especificado no existe.");
            }

            // Verificar si el usuario existe
            var usuario = await _context.Usuarios.FindAsync(participante.UsuarioID);
            if (usuario == null)
            {
                return BadRequest("El usuario especificado no existe.");
            }

            // Verificar si ya existe una inscripción para este usuario en este evento
            var inscripcionExistente = await _context.Participantes
                .AnyAsync(p => p.UsuarioID == participante.UsuarioID &&
                              p.EventoID == participante.EventoID);
            if (inscripcionExistente)
            {
                return BadRequest("El usuario ya está inscrito en este evento.");
            }

            // Verificar disponibilidad de cupos
            var totalInscritos = await _context.Participantes
                .Where(p => p.EventoID == participante.EventoID)
                .SumAsync(p => p.NumeroAsistentes);

            if (totalInscritos + participante.NumeroAsistentes > evento.CapacidadMaxima)
            {
                return BadRequest("No hay suficientes cupos disponibles en el evento.");
            }

            // Establecer la fecha de registro
            participante.FechaRegistro = DateTime.UtcNow;

            _context.Participantes.Add(participante);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInscripcion),
                new { id = participante.ID }, participante);
        }

        // PUT: api/inscripciones/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateInscripcion(int id, Participante participante)
        {
            if (id != participante.ID)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del participante.");
            }

            var inscripcionExistente = await _context.Participantes.FindAsync(id);
            if (inscripcionExistente == null)
            {
                return NotFound("La inscripción no existe.");
            }

            // Mantener la fecha de registro original
            participante.FechaRegistro = inscripcionExistente.FechaRegistro;

            _context.Entry(inscripcionExistente).CurrentValues.SetValues(participante);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Participantes.Any(p => p.ID == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/inscripciones/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteInscripcion(int id)
        {
            var inscripcion = await _context.Participantes.FindAsync(id);
            if (inscripcion == null)
            {
                return NotFound("La inscripción no existe.");
            }

            _context.Participantes.Remove(inscripcion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/inscripciones/evento/{eventoId}
        [HttpGet("evento/{eventoId}")]
        [ProducesResponseType(typeof(IEnumerable<Participante>), 200)]
        public async Task<ActionResult<IEnumerable<Participante>>> GetInscripcionesPorEvento(int eventoId)
        {
            return await _context.Participantes
                .Include(p => p.Usuario)
                .Where(p => p.EventoID == eventoId)
                .ToListAsync();
        }

        // GET: api/inscripciones/usuario/{usuarioId}
        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(IEnumerable<Participante>), 200)]
        public async Task<ActionResult<IEnumerable<Participante>>> GetInscripcionesPorUsuario(int usuarioId)
        {
            return await _context.Participantes
                .Include(p => p.Evento)
                .Where(p => p.UsuarioID == usuarioId)
                .ToListAsync();
        }
    }
}