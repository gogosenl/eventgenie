namespace EventGenie.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
        public DateOnly UserDateOfBirth { get; set; }
        public string Preferences { get; set; }

        public string EventRange { get; set; }
    }
}
