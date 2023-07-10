namespace EventSourcing.API.DTOs
{
    public class ChangeProductPriceDto
    {
        public Guid Id { get; set; }
        public decimal ChangedPrice { get; set; }
    }
}
