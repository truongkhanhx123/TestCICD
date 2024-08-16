namespace BookData.Entities
{
    public class BaseEntityDatetime
    {
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
    }
}
