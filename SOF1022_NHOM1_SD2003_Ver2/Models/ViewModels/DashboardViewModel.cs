namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }

        public int AvailableBooks { get; set; }

        public int TotalReaders { get; set; }

        public int ActiveReaders { get; set; }

        public int BorrowingBooks { get; set; }

        public int OverdueBooks { get; set; }

        public int TotalAuthors { get; set; }

        public int TotalCategories { get; set; }

        public int TotalPublishers { get; set; }

        public int TotalShelves { get; set; }

        public int TotalReservations { get; set; }

        public int TotalFines { get; set; }

        public int TotalBorrowSlips { get; set; }

        public int PendingReservations { get; set; }

        public int NewReadersThisMonth { get; set; }

        public decimal UnpaidFineAmount { get; set; }

        public decimal TotalFineAmount { get; set; }

        public List<BorrowSlips> RecentBorrowSlips { get; set; } = new();

        public List<TopBookViewModel> TopBooksTable { get; set; } = new();

        public List<TopReaderViewModel> TopReadersTable { get; set; } = new();

        public List<Books> LowStockBooks { get; set; } = new();

        public List<BorrowSlips> OverdueBorrowSlips { get; set; } = new();

        public List<Readers> NewReaders { get; set; } = new();

        public List<Books> NewBooks { get; set; } = new();

        public List<string> MonthLabels { get; set; } = new();

        public List<int> MonthBorrowCounts { get; set; } = new();

        public List<string> CategoryLabels { get; set; } = new();

        public List<int> CategoryCounts { get; set; } = new();

        //public List<ActivityLogViewModel>? RecentActivities { get; set; }

        //public List<MonthlyStatisticViewModel>? MonthlyStatistics { get; set; }
    }
}