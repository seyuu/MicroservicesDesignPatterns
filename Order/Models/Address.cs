using Microsoft.EntityFrameworkCore;

namespace Order.Models
{
    [Owned] //order tablosunda olması için 
    public class Address
    {
        public string Line { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
    }
}
