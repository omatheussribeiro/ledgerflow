using LedgerFlow.Application.Common;

namespace LedgerFlow.Application.Validators;

internal sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> errors = new(StringComparer.OrdinalIgnoreCase);

    public void AddIf(bool condition, string propertyName, string message)
    {
        if (!condition) return;
        if (!errors.TryGetValue(propertyName, out var messages))
        {
            messages = [];
            errors[propertyName] = messages;
        }

        messages.Add(message);
    }

    public void ThrowIfInvalid()
    {
        if (errors.Count == 0) return;
        throw new ApplicationValidationException(
            errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase));
    }
}
