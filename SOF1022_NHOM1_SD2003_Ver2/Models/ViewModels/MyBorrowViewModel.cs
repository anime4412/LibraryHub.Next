using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;

namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class MyBorrowViewModel
    {
        public List<BorrowItemDto> CurrentBorrows { get; set; }
        public List<BorrowItemDto> History { get; set; }
        public bool HasUnpaidFines { get; set; }
    }
}
