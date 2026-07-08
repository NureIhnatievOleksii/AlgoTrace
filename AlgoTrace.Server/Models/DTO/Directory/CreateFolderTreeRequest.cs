namespace AlgoTrace.Server.Models.DTO.Directory
{
    public class CreateFolderTreeRequest
    {
        public Guid? ParentId { get; set; }
        public List<string> FolderPaths { get; set; }
    }
}