using Domain.Enums;

namespace Application.Entities.Core.ExportProfiles.DTOs
{
    public class BaseExportProfileDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }

        public ExportType? ExportType { get; set; }
        public CustomType? CustomType { get; set; }
        public string? CustomSource { get; set; }

        public FileType ExportFileType { get; set; }
        public string? TargetPath { get; set; }
    }
}
