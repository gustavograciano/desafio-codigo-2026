using DesafioInventario.Api.Data;
using DesafioInventario.Api.DTOs;
using DesafioInventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DesafioInventario.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProdutosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoResponse>>> Listar(CancellationToken ct)
    {
        var produtos = await _db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoResponse(
                p.Id,
                p.Nome,
                p.Descricao,
                p.Preco,
                p.CategoriaId,
                p.Categoria == null ? null : new ProdutoCategoriaResponse(p.Categoria.Id, p.Categoria.Nome)
            ))
            .ToListAsync(ct);

        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponse>> Obter(int id, CancellationToken ct)
    {
        var produto = await _db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Id == id)
            .Select(p => new ProdutoResponse(
                p.Id,
                p.Nome,
                p.Descricao,
                p.Preco,
                p.CategoriaId,
                p.Categoria == null ? null : new ProdutoCategoriaResponse(p.Categoria.Id, p.Categoria.Nome)
            ))
            .FirstOrDefaultAsync(ct);

        return produto is null ? NotFound() : Ok(produto);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoResponse>> Criar(
        [FromBody] ProdutoCreateRequest request,
        CancellationToken ct)
    {
        var categoriaExiste = await _db.Categorias.AnyAsync(c => c.Id == request.CategoriaId, ct);
        if (!categoriaExiste)
        {
            ModelState.AddModelError(nameof(request.CategoriaId), "Categoria não encontrada.");
            return ValidationProblem(ModelState);
        }

        var produto = new Produto
        {
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim(),
            Preco = request.Preco,
            CategoriaId = request.CategoriaId
        };

        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(produto).Reference(p => p.Categoria).LoadAsync(ct);

        var response = new ProdutoResponse(
            produto.Id,
            produto.Nome,
            produto.Descricao,
            produto.Preco,
            produto.CategoriaId,
            produto.Categoria is null ? null : new ProdutoCategoriaResponse(produto.Categoria.Id, produto.Categoria.Nome)
        );

        return CreatedAtAction(nameof(Obter), new { id = produto.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoResponse>> Atualizar(
        int id,
        [FromBody] ProdutoUpdateRequest request,
        CancellationToken ct)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (produto is null) return NotFound();

        if (produto.CategoriaId != request.CategoriaId)
        {
            var categoriaExiste = await _db.Categorias.AnyAsync(c => c.Id == request.CategoriaId, ct);
            if (!categoriaExiste)
            {
                ModelState.AddModelError(nameof(request.CategoriaId), "Categoria não encontrada.");
                return ValidationProblem(ModelState);
            }
        }

        produto.Nome = request.Nome.Trim();
        produto.Descricao = request.Descricao?.Trim();
        produto.Preco = request.Preco;
        produto.CategoriaId = request.CategoriaId;

        await _db.SaveChangesAsync(ct);
        await _db.Entry(produto).Reference(p => p.Categoria).LoadAsync(ct);

        var response = new ProdutoResponse(
            produto.Id,
            produto.Nome,
            produto.Descricao,
            produto.Preco,
            produto.CategoriaId,
            produto.Categoria is null ? null : new ProdutoCategoriaResponse(produto.Categoria.Id, produto.Categoria.Nome)
        );

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (produto is null) return NotFound();

        _db.Produtos.Remove(produto);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
