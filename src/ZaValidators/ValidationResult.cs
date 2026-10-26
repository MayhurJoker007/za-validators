namespace ZaValidators;

/// <summary>
/// Either a parsed value or the reason it was rejected. Errors are written for end users, so they can be
/// shown on a form as-is ("ID number must be 13 digits", not "regex mismatch").
/// </summary>
public readonly record struct ValidationResult<T>(T? Value, string? Error)
{
    public bool IsValid => Error is null;

    public static ValidationResult<T> Ok(T value) => new(value, null);
    public static ValidationResult<T> Fail(string error) => new(default, error);

    public T GetValueOrThrow() => IsValid ? Value! : throw new FormatException(Error);
}

internal static class Input
{
    /// <summary>Drops the spaces, dashes and dots people type into numbers.</summary>
    public static string Clean(string? raw) =>
        raw is null ? "" : new string(raw.Where(c => !char.IsWhiteSpace(c) && c is not '-' and not '.').ToArray());

    public static bool AllDigits(string s) => s.Length > 0 && s.All(char.IsAsciiDigit);
}
