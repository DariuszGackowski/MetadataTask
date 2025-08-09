
namespace FivetranClient.Models;

public class NonPaginatedRoot<T>
{
    private T? _data;

    public T? Data
    {
        get => _data;
        set => _data = value;
    }
}