using FivetranClient.Utilities;

namespace FivetranClient.Models;

public class DataSchemas
{
    private Dictionary<string, Schema?> _schemas = [];

    public Dictionary<string, Schema?> Schemas
    {
        get => _schemas;
        set => _schemas = Guard.NotNull(value, nameof(Schemas));
    }
}