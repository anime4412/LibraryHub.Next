using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class ProfileComment
    {
        public int Id { get; set; }

        public int ProfileOwnerId { get; set; }

        public int AuthorId { get; set; }

        [Required]
        [StringLength(500)]
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(ProfileOwnerId))]
        public virtual Readers? ProfileOwner { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public virtual Readers? Author { get; set; }
    }
}
