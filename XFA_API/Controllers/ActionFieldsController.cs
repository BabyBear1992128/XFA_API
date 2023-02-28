using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XFA_API.Models;

namespace XFA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionFieldsController : ControllerBase
    {
        private readonly XFAContext _context;

        public ActionFieldsController(XFAContext context)
        {
            _context = context;
        }

        // GET: api/ActionFields
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActionField>>> GetActionFields()
        {
          if (_context.ActionFields == null)
          {
              return NotFound();
          }
            return await _context.ActionFields.ToListAsync();
        }

        // GET: api/ActionFields/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ActionField>> GetActionField(long id)
        {
          if (_context.ActionFields == null)
          {
              return NotFound();
          }
            var actionField = await _context.ActionFields.FindAsync(id);

            if (actionField == null)
            {
                return NotFound();
            }

            return actionField;
        }

        // PUT: api/ActionFields/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActionField(long id, ActionField actionField)
        {
            if (id != actionField.id)
            {
                return BadRequest();
            }

            _context.Entry(actionField).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActionFieldExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ActionFields
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ActionField>> PostActionField(ActionField actionField)
        {
          if (_context.ActionFields == null)
          {
              return Problem("Entity set 'XFAContext.ActionFields'  is null.");
          }
            _context.ActionFields.Add(actionField);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetActionField", new { id = actionField.id }, actionField);
        }

        // DELETE: api/ActionFields/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActionField(long id)
        {
            if (_context.ActionFields == null)
            {
                return NotFound();
            }
            var actionField = await _context.ActionFields.FindAsync(id);
            if (actionField == null)
            {
                return NotFound();
            }

            _context.ActionFields.Remove(actionField);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ActionFieldExists(long id)
        {
            return (_context.ActionFields?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
