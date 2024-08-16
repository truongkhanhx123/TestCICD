using System.ComponentModel.DataAnnotations;

namespace testbook.Data
{
    public class User : BaseEntityDatetime
    {
        [Key]
        public string? IdUser { get; set; } = default;
        public string? Account { get; set; } = default;
        public string? Password { get; set; } = default;
        public string? UserName { get; set; } = default;
        public DateTime LastLogin { get; set; }
        public string Role { get; set; } = "User";
        public virtual ICollection<FavoriteBook>? FavoriteBooks { get; set; } = new List<FavoriteBook>();
    }
}
