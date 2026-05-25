using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Todos.OrderByDescending(t => t.CreatedAt).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await db.Todos.FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoRequest req)
    {
        var item = new TodoItem { Title = req.Title };
        db.Todos.Add(item);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoRequest req)
    {
        var item = await db.Todos.FindAsync(id);
        if (item is null) return NotFound();
        item.Title = req.Title ?? item.Title;
        item.Done = req.Done ?? item.Done;
        await db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Todos.FindAsync(id);
        if (item is null) return NotFound();
        db.Todos.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateTodoRequest(string Title);
public record UpdateTodoRequest(string? Title, bool? Done);
