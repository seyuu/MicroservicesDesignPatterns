namespace EventSourcing.API.DTOs
{
    public class ChangeProductNameDto
    {
        public Guid Id { get; set; }
        public string ChangedName { get; set; }
    }
}
