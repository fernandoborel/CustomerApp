using CustomerApp.API.Models;
using CustomerApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace CustomerApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly MongoDbService _mongoDbService;

    public CustomersController(MongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    /// <summary>
    /// ENDPOINT para cadastro de cliente
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] Customer request)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "O nome do cliente é obrigatório." });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "O e-mail do cliente é obrigatório." });
        }

        var existingCustomer = await _mongoDbService.Customers
            .Find(c => c.Email == request.Email && c.UserId == userId)
            .FirstOrDefaultAsync();

        if (existingCustomer is not null)
        {
            return Conflict(new { message = "Já existe um cliente cadastrado com este e-mail." });
        }

        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Document = request.Document,
            Status = request.Status == 0 ? 1 : request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            UserId = userId
        };

        await _mongoDbService.Customers.InsertOneAsync(customer);

        return CreatedAtAction(nameof(Find), new { id = customer.Id }, customer);
    }

    /// <summary>
    /// ENDPOINT para atualização de cliente
    /// </summary>
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Customer request)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "ID do cliente é obrigatório." });
        }

        var existingCustomer = await _mongoDbService.Customers
            .Find(c => c.Id == id && c.UserId == userId)
            .FirstOrDefaultAsync();

        if (existingCustomer is null)
        {
            return NotFound(new { message = "Cliente não encontrado." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "O nome do cliente é obrigatório." });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "O e-mail do cliente é obrigatório." });
        }

        var customerWithSameEmail = await _mongoDbService.Customers
            .Find(c => c.Email == request.Email && c.Id != id && c.UserId == userId)
            .FirstOrDefaultAsync();

        if (customerWithSameEmail is not null)
        {
            return Conflict(new { message = "Já existe outro cliente cadastrado com este e-mail." });
        }

        var update = Builders<Customer>.Update
            .Set(c => c.Name, request.Name)
            .Set(c => c.Email, request.Email)
            .Set(c => c.Phone, request.Phone)
            .Set(c => c.Document, request.Document)
            .Set(c => c.Status, request.Status)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        await _mongoDbService.Customers.UpdateOneAsync(
            c => c.Id == id && c.UserId == userId,
            update
        );

        var updatedCustomer = await _mongoDbService.Customers
            .Find(c => c.Id == id && c.UserId == userId)
            .FirstOrDefaultAsync();

        return Ok(updatedCustomer);
    }

    /// <summary>
    /// ENDPOINT para exclusão de cliente
    /// </summary>
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "ID do cliente é obrigatório." });
        }

        var result = await _mongoDbService.Customers.DeleteOneAsync(
            c => c.Id == id && c.UserId == userId
        );

        if (result.DeletedCount == 0)
        {
            return NotFound(new { message = "Cliente não encontrado." });
        }

        return Ok(new { message = "Cliente excluído com sucesso." });
    }

    /// <summary>
    /// ENDPOINT para consulta de clientes
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (page <= 0)
        {
            page = 1;
        }

        if (limit <= 0)
        {
            limit = 10;
        }

        if (limit > 100)
        {
            limit = 100;
        }

        var skip = (page - 1) * limit;

        var filter = Builders<Customer>.Filter.Eq(c => c.UserId, userId);

        var total = await _mongoDbService.Customers
            .CountDocumentsAsync(filter);

        var customers = await _mongoDbService.Customers
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        return Ok(new
        {
            page,
            limit,
            total,
            totalPages = (int)Math.Ceiling(total / (double)limit),
            data = customers
        });
    }

    /// <summary>
    /// ENDPOINT para consultar 1 cliente através do ID
    /// </summary>
    [HttpGet("find/{id}")]
    public async Task<IActionResult> Find(string id)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "ID do cliente é obrigatório." });
        }

        var customer = await _mongoDbService.Customers
            .Find(c => c.Id == id && c.UserId == userId)
            .FirstOrDefaultAsync();

        if (customer is null)
        {
            return NotFound(new { message = "Cliente não encontrado." });
        }

        return Ok(customer);
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("id");
    }
}