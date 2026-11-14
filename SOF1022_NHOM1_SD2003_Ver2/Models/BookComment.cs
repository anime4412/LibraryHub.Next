using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class BookComment
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public int ReaderId { get; set; }

        [Required]
        [StringLength(2000)]
        public string? Content { get; set; }

        public int? ParentCommentId { get; set; }

        public int Likes { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(BookId))]
        public virtual Books? Book { get; set; }

        [ForeignKey(nameof(ReaderId))]
        public virtual Readers? Reader { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public virtual BookComment? ParentComment { get; set; }

        public virtual ICollection<BookComment>? Replies { get; set; }
    }
}
