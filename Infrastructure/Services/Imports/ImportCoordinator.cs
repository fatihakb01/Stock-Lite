using Domain.Enums;
using Domain.Services.Scheduler.Imports;

namespace Infrastructure.Services.Imports;

public class ImportCoordinator(IEnumerable<IImportHandler> handlers) : IImportCoordinator
{
    private readonly IEnumerable<IImportHandler> _handlers = handlers;

    public async Task RunAsync(ImportDataType type, string source, Guid companyId, CancellationToken ct)
    {
        var handler = _handlers.SingleOrDefault(x => x.DataType == type);

        if (handler == null)
            throw new InvalidOperationException($"No import handler for {type}");

        await using var stream = File.OpenRead(source);
        await handler.ImportAsync(stream, companyId, ct);
    }
}
