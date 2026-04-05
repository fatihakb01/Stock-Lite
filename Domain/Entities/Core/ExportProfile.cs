using Domain.Enums;

namespace Domain.Entities.Core
{
    public class ExportProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Description { get; set; }

        public ExportType? ExportType { get; set; }
        public CustomType? CustomType { get; set; }
        public string? CustomSource { get; set; }

        public string? TargetPath { get; set; }

        public Guid CompanyId { get; set; }
        public required Company Company { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
    }
}
