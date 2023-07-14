using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskLoymax.WebApi.Infrastructure;
using TaskLoymax.WebApi.Models;

namespace TaskLoymax.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ClientContext _context;

        public ClientsController(ClientContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
          if (_context.Clients == null)
          {
              return NotFound();
          }
          return await _context.Clients.ToListAsync();
        }

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(Guid id)
        {
          if (_context.Clients == null)
          {
              return NotFound();
          }
          var client = await _context.Clients.FindAsync(id);

          if (client == null)
          {
              return NotFound();
          }

          return client;
        }

        // PUT: api/Clients/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(Guid id, Client client)
        {
            if (id != client.ClientId)
            {
                return BadRequest();
            }

            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
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

        // POST: api/Clients
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(ClientRegistrationDto clientDto)
        {
            if (_context.Clients == null)
            {
                return Problem("Entity set 'ClientContext.Clients' is null.");
            }

            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                FirstName = clientDto.FirstName,
                LastName = clientDto.LastName,
                MiddleName = clientDto.MiddleName,
                DateOfBirth = clientDto.DateOfBirth,
                Balance = 0  // balance is 0 when a client is registered
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = client.ClientId }, client);
        }

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            if (_context.Clients == null)
            {
                return NotFound();
            }
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Clients/5/balance
        [HttpGet("{id}/balance")]
        public async Task<ActionResult<decimal>> GetClientBalance(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client.Balance;  
        }

        // PUT: api/Clients/5/deposit
        [HttpPut("{id}/deposit")]
        public async Task<IActionResult> Deposit(Guid id, [FromBody] decimal amount)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var client = await _context.Clients.FindAsync(id);

                if (client == null)
                {
                    return NotFound();
                }

                client.Balance += amount;  // начисление

                await _context.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            return NoContent();
        }

        // PUT: api/Clients/5/withdraw
        [HttpPut("{id}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid id, [FromBody] decimal amount)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var client = await _context.Clients.FindAsync(id);

                if (client == null)
                {
                    return NotFound();
                }

                if (client.Balance < amount) // проверка на достаточность средств
                {
                    return BadRequest("Insufficient funds");
                }

                client.Balance -= amount;  // списание

                await _context.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            return NoContent();
        }

        private bool ClientExists(Guid id)
        {
            return (_context.Clients?.Any(e => e.ClientId == id)).GetValueOrDefault();
        }
    }
}
