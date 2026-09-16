namespace LedgerFlow.Application.Interfaces.Services;

public interface IRequestValidator<in TRequest>
{
    void ValidateAndThrow(TRequest request);
}
