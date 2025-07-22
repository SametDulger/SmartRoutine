using SmartRoutine.Domain.Exceptions;

namespace SmartRoutine.Domain.ValueObjects;

public class RoutineTitle : ValueObject
{
    public string Value { get; private set; }

    private RoutineTitle(string value)
    {
        Value = value;
    }

    public static RoutineTitle Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Rutin başlığı boş olamaz.");
        }

        if (title.Length > 200)
        {
            throw new DomainValidationException("Rutin başlığı 200 karakterden uzun olamaz.");
        }

        if (title.Length < 3)
        {
            throw new DomainValidationException("Rutin başlığı en az 3 karakter olmalıdır.");
        }

        // Remove excessive whitespace and normalize
        var normalizedTitle = title.Trim();
        while (normalizedTitle.Contains("  "))
        {
            normalizedTitle = normalizedTitle.Replace("  ", " ");
        }

        return new RoutineTitle(normalizedTitle);
    }

    public static implicit operator string(RoutineTitle title)
    {
        return title.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
} 