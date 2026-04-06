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
        Task<MemoryStream> WriteStreamCsvAsync<T>(
            List<T> records,
            CancellationToken ct
        );
        void WriteExcel<T>(
            List<T> records, 
            string targetPath
        );
        MemoryStream WriteStreamExcel<T>(
            List<T> records
        );
        Task WriteDictionaryCsvAsync(
            List<Dictionary<string,
            object?>> rows,
            string targetPath
        );
        Task<MemoryStream> WriteStreamDictionaryCsvAsync(
            List<Dictionary<string,
            object?>> rows
        );
        void WriteDictionaryExcel(
            List<Dictionary<string,
            object?>> rows,
            string targetPath
        );
        MemoryStream WriteStreamDictionaryExcel(
            List<Dictionary<string,
            object?>> rows
        );
        FileType GetFileTypeFromPath(string targetPath);
    }
}
