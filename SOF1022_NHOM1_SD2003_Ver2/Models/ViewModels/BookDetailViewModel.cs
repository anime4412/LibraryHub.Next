namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class BookDetailViewModel
    {
        public Books? Book { get; set; }

        public List<Books>? RelatedBooks { get; set; }

        public List<BookComment>? Comments { get; set; }

        public bool IsFavorited { get; set; }

        public decimal AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public int FavoriteCount { get; set; }

        public int BorrowCount { get; set; }

        public bool HasBorrowedBook { get; set; }

        public bool IsCurrentlyBorrowing { get; set; }

        public DateTime? CurrentBorrowDueDate { get; set; }

        public int RealAvailableCount { get; set; }

        public bool IsAvailable { get; set; }
    }
}
