namespace Domain.Interfaces.Services.Export.Records
{
    public sealed class ExportResult
    {
        public required byte[] Content { get; init; }
        public required string FileName { get; init; }
        public required string ContentType { get; init; }
    }
}
