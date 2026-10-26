namespace ZaValidators;

public enum Gender { Female, Male }

public enum Citizenship { SouthAfrican, PermanentResident, Refugee }

/// <summary>
/// A South African ID number: YYMMDD SSSS C A Z.
/// <list type="bullet">
/// <item>YYMMDD date of birth</item>
/// <item>SSSS sequence, 0000-4999 female, 5000-9999 male</item>
/// <item>C 0 citizen, 1 permanent resident, 2 refugee</item>
/// <item>A historically a race digit, now normally 8</item>
/// <item>Z Luhn check digit</item>
/// </list>
/// </summary>
public sealed record SaIdNumber(string Value, DateOnly DateOfBirth, Gender Gender, Citizenship Citizenship)
{
    /// <param name="today">Used to decide the century: a two-digit year that would put the birth date in the future
    /// belongs to the previous century. Defaults to the current date.</param>
    public static ValidationResult<SaIdNumber> Parse(string? input, DateOnly? today = null)
    {
        var id = Input.Clean(input);
        if (id.Length != 13 || !Input.AllDigits(id))
            return ValidationResult<SaIdNumber>.Fail("ID number must be 13 digits");

        var now = today ?? DateOnly.FromDateTime(DateTime.Today);
        var yy = int.Parse(id[..2]);
        var month = int.Parse(id[2..4]);
        var day = int.Parse(id[4..6]);
        var year = 2000 + yy;
        if (year > now.Year) year -= 100;

        if (month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
            return ValidationResult<SaIdNumber>.Fail("ID number does not start with a valid date of birth");

        var dob = new DateOnly(year, month, day);
        if (dob > now)
        {
            dob = dob.AddYears(-100);
        }

        var citizenship = id[10] switch
        {
            '0' => Citizenship.SouthAfrican,
            '1' => Citizenship.PermanentResident,
            '2' => Citizenship.Refugee,
            _ => (Citizenship?)null,
        };
        if (citizenship is null)
            return ValidationResult<SaIdNumber>.Fail("ID number has an invalid citizenship digit");

        if (!Luhn.IsValid(id))
            return ValidationResult<SaIdNumber>.Fail("ID number check digit is wrong, please check for typos");

        var gender = int.Parse(id[6..10]) >= 5000 ? Gender.Male : Gender.Female;
        return ValidationResult<SaIdNumber>.Ok(new SaIdNumber(id, dob, gender, citizenship.Value));
    }

    public static bool IsValid(string? input) => Parse(input).IsValid;

    public int AgeOn(DateOnly date)
    {
        var age = date.Year - DateOfBirth.Year;
        if (DateOfBirth > date.AddYears(-age)) age--;
        return age;
    }

    /// <summary>First six digits and the rest hidden, for payslips and logs.</summary>
    public string Masked => Value[..6] + new string('*', 7);

    public override string ToString() => Value;
}
