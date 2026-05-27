using DesafioInventario.Api.Data;
using DesafioInventario.Api.DTOs;
using DesafioInventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesafioInventario.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaResponse>>> Listar(CancellationToken ct)
    {
        var categorias = await _db.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaResponse(c.Id, c.Nome, c.Descricao))
            .ToListAsync(ct);

        return Ok(categorias);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaResponse>> Obter(int id, CancellationToken ct)
    {
        var categoria = await _db.Categorias
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoriaResponse(c.Id, c.Nome, c.Descricao))
            .FirstOrDefaultAsync(ct);

        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaResponse>> Criar(
        [FromBody] CategoriaCreateRequest request,
        CancellationToken ct)
    {
        var categoria = new Categoria
        {
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim()
        };

        _db.Categorias.Add(categoria);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { mensagem = "Já existe uma categoria com este nome." });
        }

        var response = new CategoriaResponse(categoria.Id, categoria.Nome, categoria.Descricao);
        return CreatedAtAction(nameof(Obter), new { id = categoria.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaResponse>> Atualizar(
        int id,
        [FromBody] CategoriaUpdateRequest request,
        CancellationToken ct)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (categoria is null) return NotFound();

        categoria.Nome = request.Nome.Trim();
        categoria.Descricao = request.Descricao?.Trim();

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { mensagem = "Já existe uma categoria com este nome." });
        }

        return Ok(new CategoriaResponse(categoria.Id, categoria.Nome, categoria.Descricao));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (categoria is null) return NotFound();

        var possuiProdutos = await _db.Produtos.AnyAsync(p => p.CategoriaId == id, ct);
        if (possuiProdutos)
        {
            return Conflict(new
            {
                mensagem = "Não é possível excluir uma categoria que possua produtos vinculados."
            });
        }

        _db.Categorias.Remove(categoria);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is Npgsql.PostgresException { SqlState: "23505" };
}
