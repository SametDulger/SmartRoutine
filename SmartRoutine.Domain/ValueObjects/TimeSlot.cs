using SmartRoutine.Domain.Exceptions;

namespace SmartRoutine.Domain.ValueObjects;

public class TimeSlot : ValueObject
{
    public TimeOnly Value { get; private set; }
    public string DisplayText { get; private set; }

    private TimeSlot(TimeOnly value)
    {
        Value = value;
        DisplayText = value.ToString("HH:mm");
    }

    public static TimeSlot Create(TimeOnly time)
    {
        // Business rule: Only allow times in 15-minute increments
        if (time.Minute % 15 != 0)
        {
            throw new DomainValidationException("Rutin saatleri 15 dakika aralıklarında olmalıdır (örn: 09:00, 09:15, 09:30, 09:45).");
        }

        return new TimeSlot(time);
    }

    public static TimeSlot Create(int hour, int minute)
    {
        if (hour < 0 || hour > 23)
        {
            throw new DomainValidationException("Saat 0-23 arasında olmalıdır.");
        }

        if (minute < 0 || minute > 59)
        {
            throw new DomainValidationException("Dakika 0-59 arasında olmalıdır.");
        }

        return Create(new TimeOnly(hour, minute));
    }

    public static TimeSlot Create(string timeString)
    {
        if (string.IsNullOrWhiteSpace(timeString))
        {
            throw new DomainValidationException("Saat bilgisi gereklidir.");
        }

        if (!TimeOnly.TryParse(timeString, out var time))
        {
            throw new DomainValidationException("Geçersiz saat formatı. Lütfen HH:MM formatında giriniz (örn: 09:30).");
        }

        return Create(time);
    }

    public bool IsInMorning => Value.Hour < 12;
    public bool IsInAfternoon => Value.Hour >= 12 && Value.Hour < 18;
    public bool IsInEvening => Value.Hour >= 18;

    public string GetPeriodName()
    {
        return Value.Hour switch
        {
            >= 0 and < 6 => "Gece",
            >= 6 and < 12 => "Sabah",
            >= 12 and < 18 => "Öğleden Sonra",
            >= 18 and < 22 => "Akşam",
            _ => "Gece"
        };
    }

    public static implicit operator TimeOnly(TimeSlot timeSlot)
    {
        return timeSlot.Value;
    }

    public static implicit operator string(TimeSlot timeSlot)
    {
        return timeSlot.DisplayText;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return DisplayText;
    }
} 