using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class BookViewHistory
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public int? ReaderId { get; set; }

        public string? SessionId { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(BookId))]
        public virtual Books? Book { get; set; }

        [ForeignKey(nameof(ReaderId))]
        public virtual Readers? Reader { get; set; }
    }
}
