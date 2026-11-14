namespace SOF1022_NHOM1_SD2003_Ver2.Helpers
{
    public static class GuestbookTagCatalog
    {
        // Tag definition
        public class Tag
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public string? Emoji { get; set; }
        }

        // Category definition
        public class Category
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public List<Tag> Tags { get; set; }
        }

        // Preset message definition
        public class PresetMessage
        {
            public string Key { get; set; }
            public string Message { get; set; }
        }

        // All categories
        public static readonly List<Category> Categories = new List<Category>
        {
            new Category
            {
                Id = "life",
                Label = "Cuộc sống",
                Tags = new List<Tag>
                {
                    new Tag { Key = "life_night_owl", Label = "Cú đêm", Emoji = "" },
                    new Tag { Key = "life_early_bird", Label = "Người dậy sớm", Emoji = "" },
                    new Tag { Key = "life_student", Label = "Sinh viên", Emoji = "" },
                    new Tag { Key = "life_office_worker", Label = "Dân văn phòng", Emoji = "" },
                    new Tag { Key = "life_job_seeker", Label = "Đang tìm việc", Emoji = "" },
                    new Tag { Key = "life_introvert", Label = "Người hướng nội", Emoji = "" },
                    new Tag { Key = "life_travel_lover", Label = "Mê du lịch", Emoji = "" },
                    new Tag { Key = "life_newbie", Label = "Người mới đi làm", Emoji = "" }
                }
            },
            new Category
            {
                Id = "reading",
                Label = "Sở thích đọc",
                Tags = new List<Tag>
                {
                    new Tag { Key = "reading_mystery", Label = "Mê trinh thám", Emoji = "" },
                    new Tag { Key = "reading_romance", Label = "Fan ngôn tình", Emoji = "" },
                    new Tag { Key = "reading_healing", Label = "Đọc để chữa lành", Emoji = "" },
                    new Tag { Key = "reading_manga", Label = "Fan manga/LN", Emoji = "" },
                    new Tag { Key = "reading_poetry", Label = "Yêu thơ ca", Emoji = "" },
                    new Tag { Key = "reading_scifi", Label = "Khoa học viễn tưởng", Emoji = "" },
                    new Tag { Key = "reading_history", Label = "Sách lịch sử", Emoji = "" },
                    new Tag { Key = "reading_learning", Label = "Đọc để học hỏi", Emoji = "" }
                }
            },
            new Category
            {
                Id = "message",
                Label = "Gửi gắm",
                Tags = new List<Tag>
                {
                    new Tag { Key = "msg_need_motivation", Label = "Đang cần động lực", Emoji = "" },
                    new Tag { Key = "msg_reading_buddy", Label = "Tìm bạn đọc cùng", Emoji = "" },
                    new Tag { Key = "msg_ready_review", Label = "Sẵn sàng review sách", Emoji = "" },
                    new Tag { Key = "msg_inspiration", Label = "Tìm cảm hứng viết", Emoji = "" },
                    new Tag { Key = "msg_optimist", Label = "Người lạc quan", Emoji = "" },
                    new Tag { Key = "msg_hello", Label = "Chào bạn mới!", Emoji = "" }
                }
            }
        };

        // Preset messages (optional, max 1 per pin)
        public static readonly List<PresetMessage> PresetMessages = new List<PresetMessage>
        {
            new PresetMessage { Key = "pm_happy_reading", Message = "Chúc bạn đọc sách vui vẻ" },
            new PresetMessage { Key = "pm_star", Message = "Tặng bạn một ngôi sao" },
            new PresetMessage { Key = "pm_see_you", Message = "Hẹn gặp lại ở kệ sách khác" }
        };

        // Helper methods
        public static Tag? GetTagByKey(string key)
        {
            foreach (var category in Categories)
            {
                var tag = category.Tags.FirstOrDefault(t => t.Key == key);
                if (tag != null) return tag;
            }
            return null;
        }

        public static bool IsValidTagKey(string key)
        {
            return GetTagByKey(key) != null;
        }

        public static bool IsValidPresetMessageKey(string key)
        {
            return PresetMessages.Any(pm => pm.Key == key);
        }

        public static PresetMessage? GetPresetMessageByKey(string key)
        {
            return PresetMessages.FirstOrDefault(pm => pm.Key == key);
        }

        public static List<Tag> GetTagsByKeys(List<string> keys)
        {
            var result = new List<Tag>();
            foreach (var key in keys)
            {
                var tag = GetTagByKey(key);
                if (tag != null) result.Add(tag);
            }
            return result;
        }
    }
}
