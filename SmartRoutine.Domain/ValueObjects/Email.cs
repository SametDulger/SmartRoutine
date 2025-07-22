using System.Text.RegularExpressions;
using SmartRoutine.Domain.Exceptions;

namespace SmartRoutine.Domain.ValueObjects;

public class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException("Email adresi boş olamaz.");
        }

        if (email.Length > 255)
        {
            throw new DomainValidationException("Email adresi 255 karakterden uzun olamaz.");
        }

        if (!EmailRegex.IsMatch(email))
        {
            throw new DomainValidationException("Geçersiz email adresi formatı.");
        }

        return new Email(email.ToLowerInvariant().Trim());
    }

    public static implicit operator string(Email email)
    {
        return email.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
} 