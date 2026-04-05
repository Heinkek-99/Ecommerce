using Ecommerce.Shared.Common;

namespace Ecommerce.Shared.Common;

public static class Guard
{
    public static Result NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            return Result.Failure($"'{paramName}' must not be null.");
        return Result.Success();
    }

    public static Result NotEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure($"'{paramName}' must not be empty.");
        return Result.Success();
    }

    public static Result GreaterThan(decimal value, decimal threshold, string paramName)
    {
        if (value <= threshold)
            return Result.Failure($"'{paramName}' must be greater than {threshold}.");
        return Result.Success();
    }

    public static Result GreaterThan(int value, int threshold, string paramName)
    {
        if (value <= threshold)
            return Result.Failure($"'{paramName}' must be greater than {threshold}.");
        return Result.Success();
    }
}
