using Domain.Enums;

namespace Domain.Interfaces.Services.Export
{
    public interface IFileExportHelper
    {
        Task WriteCsvAsync<T>(
            List<T> records, 
            string targetPath, 
            CancellationToken ct
        );
        void WriteExcel<T>(
            List<T> records, 
            string targetPath
        );
        Task WriteDictionaryCsvAsync(
            List<Dictionary<string,
            object?>> rows,
            string targetPath
        );
        void WriteDictionaryExcel(
            List<Dictionary<string,
            object?>> rows,
            string targetPath
        );
        FileType GetFileTypeFromPath(string targetPath);
    }
}
