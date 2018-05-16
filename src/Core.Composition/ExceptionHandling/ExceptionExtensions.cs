namespace CustomCode.Core.Composition.ExceptionHandling
{
    using System;

    public static class ExceptionExtensions
    {
        public static UnresolvedDependencyException AsUnresolvedDependencyException(this Exception exception)
        {
            var invalidOperationException = ExtractUnresolvedDependencyException(exception);
            if (invalidOperationException != null)
            {
                var messagePart = ExtractMessagePart(invalidOperationException.Message, "Target Type");
                var serviceType = ExtractTypeName(messagePart);
                var serviceNamespace = ExtractNamespace(messagePart);

                messagePart = ExtractMessagePart(invalidOperationException.Message, "Parameter");
                var index = messagePart.IndexOf('(') + 1;
                messagePart = messagePart.Substring(index, messagePart.Length - 1 - index);
                var dependencyType = ExtractTypeName(messagePart);
                var dependencyNamespace = ExtractNamespace(messagePart);

                return new UnresolvedDependencyException(
                    dependencyNamespace,
                    dependencyType,
                    serviceNamespace,
                    serviceType,
                    invalidOperationException);
            }

            return null;
        }

        private static InvalidOperationException ExtractUnresolvedDependencyException(Exception exception)
        {
            var current = exception;
            while (current != null)
            {
                if (current is InvalidOperationException e)
                {
                    if (e.Message.StartsWith("Unresolved dependency", StringComparison.OrdinalIgnoreCase))
                    {
                        return e;
                    }
                }

                current = current.InnerException;
            }

            return null;
        }
        
        private static bool TryFindUnresolvedDependencyMessage(Exception exception, out string errorMessage)
        {
            var current = exception;
            while (current != null)
            {
                if (current is InvalidOperationException e)
                {
                    if (e.Message.StartsWith("Unresolved dependency", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = e.Message;
                        return true;
                    }
                }

                current = current.InnerException;
            }

            errorMessage = null;
            return false;
        }
        
        private static string ExtractMessagePart(string message, string partIdentifier)
        {
            var startIndex = message.IndexOf($"[{partIdentifier}: ");
            if (startIndex >= 0)
            {
                startIndex += partIdentifier.Length + 3;
                var endIndex = message.IndexOf("]]", startIndex);
                if (endIndex >= 0)
                {
                    return message.Substring(startIndex, endIndex - startIndex + 1);
                }

                endIndex = message.IndexOf("]", startIndex);
                if (endIndex >= 0)
                {
                    return message.Substring(startIndex, endIndex - startIndex);
                }
            }

            return null;
        }

        private static string ExtractTypeName(string @string)
        {
            var startIndex = @string.LastIndexOf('.');
            if (startIndex >= 0)
            {
                var typeName = @string.Substring(startIndex + 1);
                startIndex = typeName.LastIndexOf('+');
                if (startIndex >= 0)
                {
                    typeName = typeName.Substring(startIndex + 1);
                }

                startIndex = typeName.LastIndexOf("`1[");
                if (startIndex >= 0)
                {
                    typeName = typeName.Replace("`1[", "<");
                    typeName = typeName.Replace("]", ">");
                }

                return typeName;
            }

            return @string;
        }

        private static string ExtractNamespace(string @string)
        {
            var startIndex = @string.LastIndexOf('.');
            if (startIndex >= 0)
            {
                return @string.Substring(0, startIndex);
            }

            return null;
        }
    }
}