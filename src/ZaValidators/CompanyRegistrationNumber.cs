using System.Text.RegularExpressions;

namespace ZaValidators;

public enum EntityType
{
    PublicCompany = 6,
    PrivateCompany = 7,
    NonProfitCompany = 8,
    PersonalLiabilityCompany = 21,
    CloseCorporation = 23,
    CoOperative = 24,
    ExternalCompany = 10,
}

/// <summary>CIPC registration numbers, e.g. 2019/123456/07. The last part says what kind of entity it is.</summary>
public sealed partial record CompanyRegistrationNumber(int Year, string Sequence, EntityType EntityType)
{
    [GeneratedRegex(@"^(?<year>\d{4})/(?<seq>\d{6})/(?<type>\d{2})$")]
    private static partial Regex Pattern();

    public static ValidationResult<CompanyRegistrationNumber> Parse(string? input, int? currentYear = null)
    {
        var s = (input ?? "").Trim().Replace(" ", "").Replace('\\', '/');
        // People often leave the slashes out
        if (s.Length == 12 && s.All(char.IsAsciiDigit))
            s = $"{s[..4]}/{s[4..10]}/{s[10..]}";

        var m = Pattern().Match(s);
        if (!m.Success)
            return ValidationResult<CompanyRegistrationNumber>.Fail("Registration number must look like 2019/123456/07");

        var year = int.Parse(m.Groups["year"].Value);
        if (year < 1900 || year > (currentYear ?? DateTime.Today.Year))
            return ValidationResult<CompanyRegistrationNumber>.Fail("Registration year is not plausible");

        var typeCode = int.Parse(m.Groups["type"].Value);
        if (!Enum.IsDefined(typeof(EntityType), typeCode))
            return ValidationResult<CompanyRegistrationNumber>.Fail($"Unknown entity type code {typeCode:00}");

        return ValidationResult<CompanyRegistrationNumber>.Ok(
            new CompanyRegistrationNumber(year, m.Groups["seq"].Value, (EntityType)typeCode));
    }

    public override string ToString() => $"{Year}/{Sequence}/{(int)EntityType:00}";
}
