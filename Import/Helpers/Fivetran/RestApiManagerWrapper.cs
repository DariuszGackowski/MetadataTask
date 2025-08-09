using FivetranClient;

namespace Import.Helpers.Fivetran;

public sealed class RestApiManagerWrapper(RestApiManager restApiManager, string groupId) : IDisposable
{
    private readonly RestApiManager _restApiManager = restApiManager ?? throw new ArgumentNullException(nameof(restApiManager));
    private readonly string _groupId = groupId ?? throw new ArgumentNullException(nameof(groupId));

    public RestApiManager RestApiManager => _restApiManager;
    public string GroupId => _groupId;

    public void Dispose()
    {
        _restApiManager.Dispose();
        GC.SuppressFinalize(this);
    }
}