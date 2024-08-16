using BookData.Entities;

namespace BookApplicationApi.Dto
{
    public class AdminGetBookSaveDBDto
    {
        public int StatusCode { get; set; } = default;
        public string? Message { get; set; } = default;
        public List<Book> Book { get; set; } = new List<Book>();
    }
}
