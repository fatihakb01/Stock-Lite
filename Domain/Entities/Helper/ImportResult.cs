namespace Domain.Entities.Helper
{
    public class ImportResult
    {
        public List<Guid> AffectedIds { get; set; } = [];
        public int RowsProcessed { get; set; }
        public bool HasAffectedIds { get; set; }
    }
}
