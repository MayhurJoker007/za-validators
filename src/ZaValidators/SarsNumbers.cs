namespace ZaValidators;

/// <summary>SARS income tax reference numbers: 10 digits, starting 0, 1, 2, 3 or 9, with a Luhn check digit.</summary>
public static class TaxReferenceNumber
{
    public static ValidationResult<string> Parse(string? input)
    {
        var n = Input.Clean(input);
        if (n.Length != 10 || !Input.AllDigits(n))
            return ValidationResult<string>.Fail("Tax reference number must be 10 digits");
        if (n[0] is not ('0' or '1' or '2' or '3' or '9'))
            return ValidationResult<string>.Fail("Tax reference number must start with 0, 1, 2, 3 or 9");
        if (!Luhn.IsValid(n))
            return ValidationResult<string>.Fail("Tax reference number check digit is wrong");
        return ValidationResult<string>.Ok(n);
    }

    public static bool IsValid(string? input) => Parse(input).IsValid;
}

/// <summary>VAT vendor numbers: 10 digits starting with 4, with a Luhn check digit.</summary>
public static class VatNumber
{
    public static ValidationResult<string> Parse(string? input)
    {
        var n = Input.Clean(input);
        if (n.StartsWith("ZA", StringComparison.OrdinalIgnoreCase)) n = n[2..];
        if (n.Length != 10 || !Input.AllDigits(n))
            return ValidationResult<string>.Fail("VAT number must be 10 digits");
        if (n[0] != '4')
            return ValidationResult<string>.Fail("VAT number must start with 4");
        if (!Luhn.IsValid(n))
            return ValidationResult<string>.Fail("VAT number check digit is wrong");
        return ValidationResult<string>.Ok(n);
    }

    public static bool IsValid(string? input) => Parse(input).IsValid;
}
