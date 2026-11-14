using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Books
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "ISBN không được để trống")]
        [StringLength(20)]
        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(250)]
        [Display(Name = "Tên sách")]
        public string? Title { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Edition { get; set; }

        [StringLength(300)]
        public string? Keywords { get; set; }

        [Range(1000, 3000)]
        [Display(Name = "Năm xuất bản")]
        public int PublishYear { get; set; }

        public string? CreatedBy { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Số trang")]
        public int PageCount { get; set; }

        [Display(Name = "Ngôn ngữ")]
        public string? Language { get; set; }

        public DateTime? LastBorrowDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Range(0, 1000, ErrorMessage = "Tổng số lượng phải từ 0 đến 1,000 cuốn")]
        [Display(Name = "Tổng số lượng")]
        public int Quantity { get; set; }

        [Range(0, 1000, ErrorMessage = "Số lượng còn phải từ 0 đến 1,000 cuốn")]
        [Display(Name = "Số lượng còn")]
        public int AvailableQuantity { get; set; }

        [Range(0, 18)]
        [Display(Name = "Độ tuổi tối thiểu")]
        public int MinimumAge { get; set; } = 0;

        [Display(Name = "Ảnh bìa")]
        public string? CoverImage { get; set; }

        [Required(ErrorMessage = "Danh mục không được để trống")]
        public int CategoriesId { get; set; }

        [Required(ErrorMessage = "Tác giả không được để trống")]
        public int AuthorsId { get; set; }

        [Required(ErrorMessage = "Nhà xuất bản không được để trống")]
        public int PublishersId { get; set; }

        [Required(ErrorMessage = "Kệ sách không được để trống")]
        public int ShelvesId { get; set; }

        [ForeignKey(nameof(CategoriesId))]
        public virtual Categories? Category { get; set; }

        [ForeignKey(nameof(AuthorsId))]
        public virtual Authors? Author { get; set; }

        [ForeignKey(nameof(PublishersId))]
        public virtual Publishers? Publisher { get; set; }

        [ForeignKey(nameof(ShelvesId))]
        public virtual Shelves? Shelf { get; set; }

        public BooksStatus BooksStatus { get; set; } = BooksStatus.Available;

        public int ViewCount { get; set; } = 0;

        public virtual ICollection<BorrowDetails>? BorrowDetails { get; set; }

        public virtual ICollection<Reservations>? Reservations { get; set; }

        public virtual ICollection<BookFavorite>? BookFavorites { get; set; }

        public virtual ICollection<BookComment>? BookComments { get; set; }

        public virtual ICollection<BookRating>? BookRatings { get; set; }

        public virtual ICollection<BookViewHistory>? BookViewHistories { get; set; }

        // Soft-delete flag
        public bool IsDeleted { get; set; } = false;
    }
}
