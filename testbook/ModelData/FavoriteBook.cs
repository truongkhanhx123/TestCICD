using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testbook.Data
{
    public class FavoriteBook : BaseEntityDatetime
    {
        [Key]
        public string? UserId { get; set; } = default;

        [Key]
        public int BookId { get; set; }


        [ForeignKey("UserId")]
        public virtual User? User { get; set; } = default;

        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; } = default;
    }
}

