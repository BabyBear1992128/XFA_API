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
    public class InputFieldsController : ControllerBase
    {
        private readonly XFAContext _context;

        public InputFieldsController(XFAContext context)
        {
            _context = context;
        }

        // GET: api/InputFields
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InputField>>> GetInputFields()
        {
          if (_context.InputFields == null)
          {
              return NotFound();
          }
            return await _context.InputFields.ToListAsync();
        }

        // GET: api/InputFields/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InputField>> GetInputField(long id)
        {
          if (_context.InputFields == null)
          {
              return NotFound();
          }
            var inputField = await _context.InputFields.FindAsync(id);

            if (inputField == null)
            {
                return NotFound();
            }

            return inputField;
        }

        // PUT: api/InputFields/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInputField(long id, InputField inputField)
        {
            if (id != inputField.id)
            {
                return BadRequest();
            }

            _context.Entry(inputField).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InputFieldExists(id))
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

        // POST: api/InputFields
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<InputField>> PostInputField(InputField inputField)
        {
          if (_context.InputFields == null)
          {
              return Problem("Entity set 'XFAContext.InputFields'  is null.");
          }
            _context.InputFields.Add(inputField);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInputField", new { id = inputField.id }, inputField);
        }

        // DELETE: api/InputFields/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInputField(long id)
        {
            if (_context.InputFields == null)
            {
                return NotFound();
            }
            var inputField = await _context.InputFields.FindAsync(id);
            if (inputField == null)
            {
                return NotFound();
            }

            _context.InputFields.Remove(inputField);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InputFieldExists(long id)
        {
            return (_context.InputFields?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
