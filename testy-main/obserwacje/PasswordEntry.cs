namespace GeneratorHasel;

sealed record PasswordEntry(DateTime CreatedAt, string Password, int Length);
