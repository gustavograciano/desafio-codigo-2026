using System.ComponentModel.DataAnnotations;

namespace DesafioInventario.Api.DTOs;

public record CategoriaResponse(int Id, string Nome, string? Descricao);

public class CategoriaCreateRequest
{
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [MinLength(5, ErrorMessage = "O campo Nome deve possuir no mínimo 5 caracteres.")]
    [MaxLength(120, ErrorMessage = "O campo Nome deve possuir no máximo 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "O campo Descrição deve possuir no máximo 500 caracteres.")]
    public string? Descricao { get; set; }
}

public class CategoriaUpdateRequest : CategoriaCreateRequest { }
