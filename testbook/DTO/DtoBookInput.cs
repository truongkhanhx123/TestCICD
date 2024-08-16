namespace testbook.DTO
{
    public class DtoBookInput
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string? Isbn { get; set; }
        public required int PageCount { get; set; }
        public required string[] Authors { get; set; }
    }
}
