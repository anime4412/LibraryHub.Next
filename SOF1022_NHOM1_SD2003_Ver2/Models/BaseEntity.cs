namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
