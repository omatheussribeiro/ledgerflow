using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Infrastructure.Persistence.Repositories.Models;

internal sealed class CategoryReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Type { get; set; }
    public string Color { get; set; } = string.Empty;

    public CategoryResponseDto ToDto() => new(Id, Name, (TransactionType)Type, Color);
}
