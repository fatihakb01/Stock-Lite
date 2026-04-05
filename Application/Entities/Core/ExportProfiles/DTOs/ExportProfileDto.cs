using Domain.Enums;

namespace Application.Entities.Core.ExportProfiles.DTOs
{
    public class ExportProfileDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ExportType? ExportType { get; set; }
        public CustomType? CustomType { get; set; }
        public string? TargetPath { get; set; }
    }
}
