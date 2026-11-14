using SOF1022_NHOM1_SD2003_Ver2.Models;

namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class HomeViewModel
    {
        public int TotalBooks { get; set; }

        public int TotalReaders { get; set; }

        public int TotalBorrow { get; set; }

        public List<Books> NewBooks { get; set; } = new();

        public List<Books> HotBooks { get; set; } = new();

        public List<Books> FavoriteBooks { get; set; } = new();

        public List<Books> HighlyRatedBooks { get; set; } = new();

        public List<Books> RecommendBooks { get; set; } = new();

        public List<Books> FeaturedShelfBooks { get; set; } = new();

        public List<Categories> Categories { get; set; } = new();

        public List<Categories> ExploreCategories { get; set; } = new();

        public List<Authors> Authors { get; set; } = new();

        public List<Publishers> Publishers { get; set; } = new();
    }
}
