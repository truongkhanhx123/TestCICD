using System.ComponentModel.DataAnnotations;

namespace BookData.Entities

{
    public class Book : BaseEntityDatetime
    {
        [Key]
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Isbn { get; set; }
        public int PageCount { get; set; }
        public required string[] Authors { get; set; }

        public virtual ICollection<FavoriteBook>? FavoriteBooks { get; set; } = new List<FavoriteBook>();
    }
}
