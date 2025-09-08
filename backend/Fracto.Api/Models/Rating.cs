namespace Fracto.Api.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public int UserId { get; set; }
        public AppUser? User { get; set; }
        public int Score { get; set; } // 1-5
    }
}
