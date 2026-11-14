namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class QuickHelpCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public List<QuickHelpItem> Items { get; set; } = new();
    }

    public class QuickHelpItem
    {
        public string Question { get; set; } = string.Empty;
        public string AnswerHtml { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public string? ActionLabel { get; set; }
    }
}
