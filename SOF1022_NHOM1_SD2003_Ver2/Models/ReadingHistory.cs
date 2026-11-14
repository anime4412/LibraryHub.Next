using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class ReadingHistory
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public int ReaderId { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        public DateTime LastReadAt { get; set; } = DateTime.Now;

        public DateTime StartedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(BookId))]
        public virtual Books? Book { get; set; }

        [ForeignKey(nameof(ReaderId))]
        public virtual Readers? Reader { get; set; }
    }
}
