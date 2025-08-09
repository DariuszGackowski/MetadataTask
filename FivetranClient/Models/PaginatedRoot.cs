using FivetranClient.Utilities;

namespace FivetranClient.Models;

public class PaginatedRoot<T>
{
    private Data<T> _data = default!;

    public Data<T> Data
    {
        get => _data;
        set => _data = Guard.NotNull(value, nameof(Data));
    }
}