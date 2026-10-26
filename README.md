# ZaValidators

![CI](https://github.com/MayhurJoker007/za-validators/actions/workflows/ci.yml/badge.svg)

A small .NET library for checking and parsing South African identifiers. I kept rewriting these checks in
different projects (onboarding forms, payroll imports, distributor setup), so I put them in one tested package.

| Type | What it checks | What you get back |
|---|---|---|
| `SaIdNumber` | 13 digits, real date of birth, citizenship digit, Luhn check digit | date of birth, gender, citizenship, age, masked form |
| `TaxReferenceNumber` | 10 digits, starts with 0/1/2/3/9, Luhn | cleaned number |
| `VatNumber` | 10 digits, starts with 4, Luhn, optional `ZA` prefix | cleaned number |
| `CompanyRegistrationNumber` | CIPC `YYYY/NNNNNN/NN`, plausible year, known entity type | year, sequence, entity type (Pty Ltd, CC, NPC...) |
| `PhoneNumber` | SA number ranges, any common way of typing it | E.164 (`+27...`), mobile / landline / non-geographic, local format |

```csharp
var result = SaIdNumber.Parse("950412 5123 085");
if (!result.IsValid)
{
    ModelState.AddModelError("IdNumber", result.Error);   // "ID number check digit is wrong, please check for typos"
    return;
}

var id = result.Value;
id.DateOfBirth;   // 1995-04-12
id.Gender;        // Male
id.Masked;        // 950412*******
```

```csharp
PhoneNumber.Parse("(082) 555-0123").Value.E164;              // +27825550123
CompanyRegistrationNumber.Parse("201912345607").Value;          // 2019/123456/07, PrivateCompany
```

## Design choices

- **No exceptions for bad input.** `Parse` returns a `ValidationResult<T>` with either the value or an error
  message written for the person filling in the form. `GetValueOrThrow()` is there if you'd rather throw.
- **It accepts what people actually type**: spaces, dashes, dots, `+27` / `0027` / `27` / `0` prefixes, and
  registration numbers without slashes. It's lenient about formatting and strict about the content.
- **The century of an ID number is resolved against today's date** (which you can pass in for tests), so
  `02...` is 2002 and `88...` is 1988.
- **No dependencies.** It targets `net8.0`.

## Building

```
dotnet test
dotnet pack src/ZaValidators -c Release -o artifacts
```

CI runs the tests and builds the NuGet package on every push.

All ID, tax and VAT numbers in the tests are made up. Their check digits were generated to be valid, so they
don't belong to anyone.
