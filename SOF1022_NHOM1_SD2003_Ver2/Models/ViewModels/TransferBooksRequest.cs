namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class TransferBooksRequest
    {
        public int SourceShelfId { get; set; }
        public int TargetShelfId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }
}
