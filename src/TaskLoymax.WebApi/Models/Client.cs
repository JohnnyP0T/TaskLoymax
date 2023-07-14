namespace TaskLoymax.WebApi.Models
{
    public class Client
    {
        public Guid ClientId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public decimal Balance { get; set; }
    }
}
