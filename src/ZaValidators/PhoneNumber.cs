namespace ZaValidators;

public enum PhoneKind { Mobile, Landline, NonGeographic }

/// <summary>
/// South African phone numbers in E.164 (+27...). Accepts the formats people actually type:
/// 082 555 0123, 0825550123, +27 82 555 0123, 27825550123, (011) 555-0192.
/// </summary>
public sealed record PhoneNumber(string E164, PhoneKind Kind)
{
    public static ValidationResult<PhoneNumber> Parse(string? input)
    {
        var s = new string((input ?? "").Where(c => char.IsAsciiDigit(c) || c == '+').ToArray());

        if (s.StartsWith("+27")) s = s[3..];
        else if (s.StartsWith("0027")) s = s[4..];
        else if (s.StartsWith("27") && s.Length == 11) s = s[2..];
        else if (s.StartsWith('0')) s = s[1..];
        else return ValidationResult<PhoneNumber>.Fail("Phone number must start with 0 or +27");

        if (s.Length != 9 || !Input.AllDigits(s))
            return ValidationResult<PhoneNumber>.Fail("Phone number must have 10 digits including the leading 0");

        // Mobile ranges are 06x, 07x and 08x (except 080, 086 and 087 which are toll-free/shared-cost/VoIP)
        var prefix = s[..2];
        var kind = prefix switch
        {
            "80" or "86" or "87" => PhoneKind.NonGeographic,
            ['6' or '7' or '8', _] => PhoneKind.Mobile,
            ['1' or '2' or '3' or '4' or '5', _] => PhoneKind.Landline,
            _ => (PhoneKind?)null,
        };
        if (kind is null)
            return ValidationResult<PhoneNumber>.Fail("Not a South African number range");

        return ValidationResult<PhoneNumber>.Ok(new PhoneNumber("+27" + s, kind.Value));
    }

    /// <summary>The way it's normally written locally: 082 555 0123.</summary>
    public string National => $"0{E164[3..5]} {E164[5..8]} {E164[8..]}";

    public override string ToString() => E164;
}
