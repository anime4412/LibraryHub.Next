using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Reservations
    {
        public int Id { get; set; }

        public int ReaderId { get; set; }

        public int BookId { get; set; }

        public DateTime ReservationDate { get; set; } = DateTime.Now;

        public DateTime? ExpirationDate { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Waiting";

        public string? Note { get; set; }

        public ReservationsStatus BooksStatus { get; set; } = ReservationsStatus.Waiting;

        public Readers? Reader { get; set; }

        public Books? Book { get; set; }
    }
}
