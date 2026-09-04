namespace T3mmyvsa.Exceptions;

public sealed class ForbiddenException(string message = "You do not have permission to perform this action.") : Exception(message);
