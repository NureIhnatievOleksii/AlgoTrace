namespace AlgoTrace.Server.Models.DTO
{
    public class CreateFolderRequest
    {
        public string Name { get; set; }
        public int? ParentId { get; set; }
    }
}
