
namespace FivetranClient.Utilities
{
    public static class Guard
    {
        public static string NotNullOrWhiteSpace(string? value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} cannot be null, empty, or whitespace.", paramName);
            return value.Trim();
        }

        public static T NotNull<T>(T? value, string paramName)
        {
            if (value is null)
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");
            return value;
        }

        public static IEnumerable<T> NotNullOrContainsNull<T>(IEnumerable<T>? collection, string paramName)
        {
            if (collection is null)
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");

            if (collection.Any(item => item is null))
                throw new ArgumentException($"{paramName} cannot contain null elements.", paramName);

            return collection;
        }
    }
}