using System.ComponentModel.DataAnnotations;

namespace DesafioInventario.Api.DTOs;

public record ProdutoCategoriaResponse(int Id, string Nome);

public record ProdutoResponse(
    int Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    int CategoriaId,
    ProdutoCategoriaResponse? Categoria
);

public class ProdutoCreateRequest
{
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [MinLength(5, ErrorMessage = "O campo Nome deve possuir no mínimo 5 caracteres.")]
    [MaxLength(160, ErrorMessage = "O campo Nome deve possuir no máximo 160 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "O campo Descrição deve possuir no máximo 1000 caracteres.")]
    public string? Descricao { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O campo Preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria válida.")]
    public int CategoriaId { get; set; }
}

public class ProdutoUpdateRequest : ProdutoCreateRequest { }
