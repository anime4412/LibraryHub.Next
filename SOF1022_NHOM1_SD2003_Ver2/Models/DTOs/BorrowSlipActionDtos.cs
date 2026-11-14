namespace SOF1022_NHOM1_SD2003_Ver2.Models.DTOs
{
    public class IdRequestDto
    {
        public int Id { get; set; }
    }

    public class CancelItemRequestDto
    {
        public int DetailId { get; set; }
        public int BorrowDetailId { get; set; }

        public int ResolveDetailId() => DetailId > 0 ? DetailId : BorrowDetailId;
    }
}
