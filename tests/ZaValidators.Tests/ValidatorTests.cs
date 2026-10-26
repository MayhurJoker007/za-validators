using ZaValidators;

namespace ZaValidators.Tests;

public class SaIdNumberTests
{
    private static readonly DateOnly Today = new(2026, 10, 14);

    [Fact]
    public void Parses_date_of_birth_gender_and_citizenship()
    {
        var id = SaIdNumber.Parse("9504125123085", Today).GetValueOrThrow();

        Assert.Equal(new DateOnly(1995, 4, 12), id.DateOfBirth);
        Assert.Equal(Gender.Male, id.Gender);
        Assert.Equal(Citizenship.SouthAfrican, id.Citizenship);
        Assert.Equal(31, id.AgeOn(Today));
        Assert.Equal("950412*******", id.Masked);
    }

    [Fact]
    public void Two_digit_year_goes_to_the_right_century()
    {
        Assert.Equal(2002, SaIdNumber.Parse("0208155000085", Today).Value!.DateOfBirth.Year);
        Assert.Equal(1988, SaIdNumber.Parse("8801030119088", Today).Value!.DateOfBirth.Year);
        Assert.Equal(Gender.Female, SaIdNumber.Parse("8801030119088", Today).Value!.Gender);
    }

    [Fact]
    public void Leap_day_birthdays()
    {
        Assert.Equal(new DateOnly(2000, 2, 29), SaIdNumber.Parse("0002295000083", Today).Value!.DateOfBirth);
        var bad = SaIdNumber.Parse("0102290123088", Today);   // 29 Feb 2001 doesn't exist
        Assert.Equal("ID number does not start with a valid date of birth", bad.Error);
    }

    [Theory]
    [InlineData("950412 5123 085")]
    [InlineData("950412-5123-085")]
    public void Ignores_spaces_and_dashes(string input)
    {
        Assert.True(SaIdNumber.Parse(input, Today).IsValid);
    }

    [Theory]
    [InlineData("9504125123086", "check digit")]       // one digit off
    [InlineData("9504125123058", "check digit")]       // two digits swapped
    [InlineData("950412512308", "13 digits")]
    [InlineData("95041251230A5", "13 digits")]
    [InlineData("9513125123085", "date of birth")]
    [InlineData("9504125123385", "citizenship")]
    [InlineData(null, "13 digits")]
    public void Rejects_bad_numbers_with_a_useful_message(string? input, string expected)
    {
        var result = SaIdNumber.Parse(input, Today);
        Assert.False(result.IsValid);
        Assert.Contains(expected, result.Error);
    }
}

public class SarsNumberTests
{
    [Theory]
    [InlineData("0123456782")]
    [InlineData("3123456786")]
    [InlineData("0123 456 782")]
    public void Valid_tax_reference_numbers(string input) => Assert.True(TaxReferenceNumber.IsValid(input));

    [Theory]
    [InlineData("0123456783", "check digit")]
    [InlineData("4123456784", "start with")]
    [InlineData("012345678", "10 digits")]
    public void Invalid_tax_reference_numbers(string input, string expected) =>
        Assert.Contains(expected, TaxReferenceNumber.Parse(input).Error);

    [Theory]
    [InlineData("4123456784")]
    [InlineData("ZA4123456784")]
    public void Valid_vat_numbers(string input) => Assert.Equal("4123456784", VatNumber.Parse(input).Value);

    [Fact]
    public void Vat_number_must_start_with_4() =>
        Assert.Equal("VAT number must start with 4", VatNumber.Parse("0123456782").Error);
}

public class CompanyRegistrationTests
{
    [Theory]
    [InlineData("2019/123456/07", EntityType.PrivateCompany)]
    [InlineData("201912345607", EntityType.PrivateCompany)]
    [InlineData("1998/004417/23", EntityType.CloseCorporation)]
    [InlineData("2015/220031/08", EntityType.NonProfitCompany)]
    public void Parses_entity_type(string input, EntityType expected)
    {
        var reg = CompanyRegistrationNumber.Parse(input, 2026).GetValueOrThrow();
        Assert.Equal(expected, reg.EntityType);
    }

    [Fact]
    public void Formats_back_with_slashes() =>
        Assert.Equal("2019/123456/07", CompanyRegistrationNumber.Parse("201912345607", 2026).Value!.ToString());

    [Theory]
    [InlineData("2019/12345/07", "look like")]
    [InlineData("2031/123456/07", "year")]
    [InlineData("2019/123456/99", "entity type")]
    public void Rejects(string input, string expected) =>
        Assert.Contains(expected, CompanyRegistrationNumber.Parse(input, 2026).Error);
}

public class PhoneNumberTests
{
    [Theory]
    [InlineData("082 555 0123")]
    [InlineData("0825550123")]
    [InlineData("+27 82 555 0123")]
    [InlineData("27825550123")]
    [InlineData("0027825550123")]
    public void Normalises_mobile_formats(string input)
    {
        var phone = PhoneNumber.Parse(input).GetValueOrThrow();
        Assert.Equal("+27825550123", phone.E164);
        Assert.Equal(PhoneKind.Mobile, phone.Kind);
        Assert.Equal("082 555 0123", phone.National);
    }

    [Theory]
    [InlineData("(011) 555-0192", PhoneKind.Landline)]
    [InlineData("0860 123 456", PhoneKind.NonGeographic)]
    [InlineData("0800 222 333", PhoneKind.NonGeographic)]
    public void Classifies(string input, PhoneKind kind) => Assert.Equal(kind, PhoneNumber.Parse(input).Value!.Kind);

    [Theory]
    [InlineData("082 555 012")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("0925550123")]
    public void Rejects(string input) => Assert.False(PhoneNumber.Parse(input).IsValid);
}

public class LuhnTests
{
    [Fact]
    public void Matches_the_textbook_example() => Assert.True(Luhn.IsValid("79927398713"));

    [Fact]
    public void Check_digit_round_trips()
    {
        for (var n = 100000; n < 100200; n++)
        {
            var body = n.ToString();
            Assert.True(Luhn.IsValid(body + Luhn.CheckDigit(body)));
        }
    }
}
