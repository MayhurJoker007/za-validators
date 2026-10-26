namespace ZaValidators;

/// <summary>The Luhn (mod 10) check used by SA ID, income tax and VAT numbers.</summary>
public static class Luhn
{
    public static bool IsValid(ReadOnlySpan<char> digits)
    {
        if (digits.Length < 2) return false;
        return CheckDigit(digits[..^1]) == digits[^1] - '0';
    }

    /// <summary>The digit that makes <paramref name="body"/> pass the Luhn check when appended.</summary>
    public static int CheckDigit(ReadOnlySpan<char> body)
    {
        var sum = 0;
        for (int i = body.Length - 1, pos = 0; i >= 0; i--, pos++)
        {
            var d = body[i] - '0';
            if (d is < 0 or > 9) throw new ArgumentException("Only digits are allowed", nameof(body));
            if (pos % 2 == 0)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }
            sum += d;
        }
        return (10 - sum % 10) % 10;
    }
}
