-- Cập nhật thông tin chi tiết tác giả (Tiểu sử, Ngày sinh, Quốc tịch)
SET NOCOUNT ON;

-- 1. Dale Carnegie
UPDATE Authors SET 
    DateOfBirth = '1888-11-24', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'Dale Carnegie (1888 - 1955) là tác gia và nhà thuyết trình lỗi lạc người Mỹ, cha đẻ của các phương pháp phát triển kỹ năng ứng xử xã hội. Ông là tác giả kiệt tác "Đắc Nhân Tâm" (How to Win Friends and Influence People) - tác phẩm kỹ năng sống bán chạy nhất mọi thời đại với hơn 30 triệu bản trên thế giới, và cuốn "Quẳng Gánh Lo Đi Và Vui Sống".'
WHERE Name LIKE '%Dale Carnegie%';

-- 2. Paulo Coelho
UPDATE Authors SET 
    DateOfBirth = '1947-08-24', 
    Nationality = N'Brazil', 
    Biography = N'Paulo Coelho (sinh năm 1947) là tiểu thuyết gia huyền thoại người Brazil. Tác phẩm xuất sắc nhất của ông, "Nhà Giả Kim" (The Alchemist), đã được dịch sang hơn 80 thứ tiếng và bán ra hơn 150 triệu bản, truyền cảm hứng mãnh liệt về hành trình theo đuổi ước mơ và tiếng gọi con tim.'
WHERE Name LIKE '%Paulo Coelho%';

-- 3. James Clear
UPDATE Authors SET 
    DateOfBirth = '1986-01-22', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'James Clear (sinh năm 1986) là tác giả và chuyên gia hàng đầu thế giới về tâm lý hành vi và thói quen. Tác phẩm "Atomic Habits" (Thay Đổi Tí Hon, Hiệu Quả Bất Ngờ) giữ vị trí Best-seller nhiều năm liên tiếp trên The New York Times, giúp hàng triệu độc giả thiết lập hệ thống thói quen xuất sắc.'
WHERE Name LIKE '%James Clear%';

-- 4. Robert C. Martin
UPDATE Authors SET 
    DateOfBirth = '1952-12-05', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'Robert Cecil Martin (sinh năm 1952), thường được biết đến là "Uncle Bob", là chuyên gia phần mềm kỳ cựu, người đồng sáng lập Tuyên ngôn Agile và bộ nguyên lý thiết kế SOLID. Ông là tác giả các cuốn sách gối đầu giường của lập trình viên thế giới: "Clean Code", "Clean Architecture", "The Clean Coder".'
WHERE Name LIKE '%Robert C. Martin%' OR Name LIKE '%Uncle Bob%';

-- 5. Yuval Noah Harari
UPDATE Authors SET 
    DateOfBirth = '1976-02-24', 
    Nationality = N'Israel', 
    Biography = N'Yuval Noah Harari (sinh năm 1976) là nhà sử học, giáo sư tại Đại học Hebrew Jerusalem. Ông nổi tiếng toàn cầu với bộ ba tác phẩm lịch sử - tương lai đồ sộ: "Sapiens: Lược Sử Loài Người", "Homo Deus" và "21 Bài Học Cho Thế Kỷ 21", mang đến góc nhìn sâu sắc về tiến trình tiến hóa và văn minh nhân loại.'
WHERE Name LIKE '%Yuval Noah Harari%';

-- 6. Robert Kiyosaki
UPDATE Authors SET 
    DateOfBirth = '1947-04-08', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'Robert Toru Kiyosaki (sinh năm 1947) là doanh nhân, nhà đầu tư và tác giả tài chính người Mỹ gốc Nhật. Bộ sách "Dạy Con Làm Giàu" (Rich Dad Poor Dad) của ông đã thay đổi tư duy tài chính của hàng triệu người khắp thế giới thông qua mô hình kim tứ đồ và tư duy tự do tài chính.'
WHERE Name LIKE '%Robert Kiyosaki%';

-- 7. Napoleon Hill
UPDATE Authors SET 
    DateOfBirth = '1883-10-26', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'Oliver Napoleon Hill (1883 - 1970) là tác giả có tầm ảnh hưởng lớn nhất lịch sử trong thể loại văn học truyền cảm hứng thành công. Tác phẩm "Think and Grow Rich" (Nghĩ Giàu Và Làm Giàu, 1937) được ông đúc kết từ 20 năm nghiên cứu và phỏng vấn hơn 500 vĩ nhân nước Mỹ như Thomas Edison, Henry Ford.'
WHERE Name LIKE '%Napoleon Hill%';

-- 8. George Orwell
UPDATE Authors SET 
    DateOfBirth = '1903-06-25', 
    Nationality = N'Vương quốc Anh', 
    Biography = N'George Orwell (1903 - 1950, tên thật Eric Arthur Blair) là nhà văn, nhà báo và nhà phê bình văn học lỗi lạc người Anh thế kỷ 20. Ông là tác giả của hai kiệt tác kinh điển nhân loại: "1984" và "Trại Súc Vật" (Animal Farm), đưa ra những chiêm nghiệm sâu sắc về tự do tư tưởng và xã hội.'
WHERE Name LIKE '%George Orwell%';

-- 9. J.K. Rowling
UPDATE Authors SET 
    DateOfBirth = '1965-07-31', 
    Nationality = N'Vương quốc Anh', 
    Biography = N'J.K. Rowling (sinh năm 1965, tên thật Joanne Rowling) là nữ văn sĩ người Anh, tác giả của bộ tiểu thuyết kỳ ảo "Harry Potter". Tác phẩm bán hơn 600 triệu bản trên toàn cầu, được dịch ra gần 90 thứ tiếng và trở thành một trong những hiện tượng văn học lớn nhất mọi thời đại.'
WHERE Name LIKE '%Rowling%';

-- 10. Haruki Murakami
UPDATE Authors SET 
    DateOfBirth = '1949-01-12', 
    Nationality = N'Nhật Bản', 
    Biography = N'Haruki Murakami (sinh năm 1949) là một trong những tiểu thuyết gia đương đại nổi tiếng nhất của văn học Nhật Bản và thế giới. Phong cách viết của ông kết hợp độc đáo giữa hiện thực huyền ảo, âm nhạc và triết lý cô đơn, thể hiện qua các kiệt tác "Rừng Na Uy", "Kafka Bên Bờ Biển", "1Q84".'
WHERE Name LIKE '%Haruki Murakami%';

-- 11. Antoine de Saint-Exupéry
UPDATE Authors SET 
    DateOfBirth = '1900-06-29', 
    Nationality = N'Pháp', 
    Biography = N'Antoine de Saint-Exupéry (1900 - 1944) là nhà văn và phi công tiên phong người Pháp. Tác phẩm truyện ngụ ngôn bất hủ "Hoàng Tử Bé" (Le Petit Prince) của ông là một trong những cuốn sách được dịch nhiều nhất và yêu thích nhất trong lịch sử văn học thế giới.'
WHERE Name LIKE '%Antoine de Saint%' OR Name LIKE '%Exupéry%';

-- 12. Ernest Hemingway
UPDATE Authors SET 
    DateOfBirth = '1899-07-21', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'Ernest Miller Hemingway (1899 - 1961) là đại văn hào người Mỹ đoạt giải Nobel Văn học năm 1954 và giải Pulitzer năm 1953. Với nguyên lý "Tảng băng trôi" nổi tiếng, ông đã sáng tạo nên những kiệt tác bất hủ như "Ông Già Và Biển Cả", "Chuông Nguyện Hồn Ai", "Giã Từ Vũ Khí".'
WHERE Name LIKE '%Hemingway%';

-- 13. Stephen Hawking
UPDATE Authors SET 
    DateOfBirth = '1942-01-08', 
    Nationality = N'Vương quốc Anh', 
    Biography = N'Stephen William Hawking (1942 - 2018) là nhà vật lý lý thuyết, nhà vũ trụ học và tác giả người Anh vĩ đại. Dù mắc căn bệnh xơ cứng teo cơ từ trẻ, ông đã cống hiến trọn đời cho khoa học với lý thuyết hố đen và cuốn sách phổ biến khoa học kinh điển "Lược Sử Thời Gian" (A Brief History of Time).'
WHERE Name LIKE '%Stephen Hawking%';

-- 14. Nguyễn Du
UPDATE Authors SET 
    DateOfBirth = '1766-01-03', 
    Nationality = N'Việt Nam', 
    Biography = N'Nguyễn Du (1766 - 1820, tự Tố Như, hiệu Thanh Hiên) là Đại thi hào dân tộc Việt Nam, Danh nhân văn hóa thế giới được UNESCO vinh danh. Kiệt tác "Đoạn Trường Tân Thanh" (Truyện Kiều) gồm 3.254 câu thơ lục bát là đỉnh cao chói lọi của nền thi ca và ngôn ngữ văn học Việt Nam.'
WHERE Name LIKE '%Nguyễn Du%';

-- 15. Nam Cao
UPDATE Authors SET 
    DateOfBirth = '1915-10-29', 
    Nationality = N'Việt Nam', 
    Biography = N'Nam Cao (1915 - 1951, tên thật Trần Hữu Tri) là nhà văn hiện thực phê phán xuất sắc nhất của văn học Việt Nam thế kỷ 20, Liệt sĩ kháng chiến chống Pháp. Các tác phẩm kinh điển của ông như "Chí Phèo", "Lão Hạc", "Đời Thừa", "Sống Mòn" đã khắc họa sâu sắc bi kịch con người và đề cao nhân phẩm.'
WHERE Name LIKE '%Nam Cao%';

-- 16. Vũ Trọng Phụng
UPDATE Authors SET 
    DateOfBirth = '1912-10-20', 
    Nationality = N'Việt Nam', 
    Biography = N'Vũ Trọng Phụng (1912 - 1939) được mệnh danh là "Ông vua phóng sự đất Bắc", bậc thầy tiểu thuyết châm biếm trào phúng của văn học Việt Nam thời kỳ 1930 - 1945. Những tác phẩm tiêu biểu như "Số Đỏ", "Giông Tố", "Kỹ Nghệ Lấy Tây", "Cơm Thầy Cơm Cô" là những bức tranh xã hội vô cùng sắc sảo.'
WHERE Name LIKE '%Vũ Trọng Phụng%';

-- 17. Ngô Tất Tố
UPDATE Authors SET 
    DateOfBirth = '1894-07-13', 
    Nationality = N'Việt Nam', 
    Biography = N'Ngô Tất Tố (1894 - 1954) là nhà văn, nhà báo, nhà nghiên cứu văn hóa uyên bác của Việt Nam. Tiểu thuyết hiện thực xuất sắc "Tắt Đèn" và phóng sự "Việc Làng" của ông đã phản ánh chân thực số phận cùng khổ của người nông dân dưới ách thực dân phong kiến.'
WHERE Name LIKE '%Ngô Tất Tố%';

-- 18. Tô Hoài
UPDATE Authors SET 
    DateOfBirth = '1920-09-27', 
    Nationality = N'Việt Nam', 
    Biography = N'Tô Hoài (1920 - 2014, tên thật Nguyễn Sen) là cây đại thụ của nền văn học hiện đại Việt Nam, Giải thưởng Hồ Chí Minh về Văn học Nghệ thuật (1996). Ông nổi tiếng với tác phẩm đồng thoại thiếu nhi "Dế Mèn Phiêu Lưu Ký" được dịch ra gần 40 thứ tiếng, cùng tập "Truyện Tây Bắc" (Vợ Chồng A Phủ).'
WHERE Name LIKE '%Tô Hoài%';

-- 19. Tôn Vũ
UPDATE Authors SET 
    DateOfBirth = '0545-01-01', 
    Nationality = N'Trung Quốc', 
    Biography = N'Tôn Vũ (khoảng 545 TCN - 470 TCN, tự Trưởng Khanh) là danh tướng kiệt xuất, nhà quân sự và triết học cổ đại lỗi lạc thời Xuân Thu. Bộ sách "Binh Pháp Tôn Tử" gồm 13 thiên do ông biên soạn là tác phẩm binh pháp kinh điển sớm nhất và uyên bác nhất trong lịch sử nhân loại, được ứng dụng rộng rãi cả trong quản trị và chiến lược kinh doanh hiện đại.'
WHERE Name LIKE '%Tôn Vũ%';

-- 20. Andrea Hirata
UPDATE Authors SET 
    DateOfBirth = '1967-10-24', 
    Nationality = N'Indonesia', 
    Biography = N'Andrea Hirata (sinh năm 1967) là nhà văn danh tiếng người Indonesia. Cuốn tiểu thuyết đầu tay "Chiến Binh Cầu Vồng" (Laskar Pelangi) của ông dựa trên ký ức tuổi thơ nghèo khó trên đảo Belitung đã phá vỡ mọi kỷ lục xuất bản tại Indonesia, được dịch sang hơn 30 thứ tiếng và truyền cảm hứng mãnh liệt về sức mạnh của giáo dục.'
WHERE Name LIKE '%Andrea Hirata%';

-- 21. Victor Hugo
UPDATE Authors SET 
    DateOfBirth = '1802-02-26', 
    Nationality = N'Pháp', 
    Biography = N'Victor Hugo (1802 - 1885) là đại thi hào, tiểu thuyết gia và chính khách kiệt xuất người Pháp, ngọn cờ đầu của chủ nghĩa lãng mạn. Ông là tác giả của các thiên trường tiểu thuyết nhân đạo bất hủ "Những Người Khốn Khổ" (Les Misérables) và "Nhà Thờ Đức Bà Paris" (Notre-Dame de Paris).'
WHERE Name LIKE '%Victor Hugo%';

-- 22. Arthur Conan Doyle
UPDATE Authors SET 
    DateOfBirth = '1859-05-22', 
    Nationality = N'Vương quốc Anh', 
    Biography = N'Sir Arthur Conan Doyle (1859 - 1930) là nhà văn, bác sĩ người Scotland. Ông đã sáng tạo nên nhân vật thám tử lừng danh nhất lịch sử văn học "Sherlock Holmes" cùng người bạn thân bác sĩ Watson, tạo nên tượng đài bất hủ của thể loại truyện trinh thám thế giới.'
WHERE Name LIKE '%Conan Doyle%';

-- 23. Hector Malot
UPDATE Authors SET 
    DateOfBirth = '1830-05-20', 
    Nationality = N'Pháp', 
    Biography = N'Hector Malot (1830 - 1907) là nhà văn Pháp nổi tiếng với những tác phẩm giàu tính nhân văn và giáo dục thiếu nhi. Hai kiệt tác để đời của ông là "Không Gia Đình" (Sans Famille) và "Trong Gia Đình" (En Famille) đã sưởi ấm tâm hồn bao thế hệ độc giả về nghị lực phi thường và lòng nhân ái.'
WHERE Name LIKE '%Hector Malot%';

-- 24. Nguyễn Nhật Ánh
UPDATE Authors SET 
    DateOfBirth = '1955-05-07', 
    Nationality = N'Việt Nam', 
    Biography = N'Nguyễn Nhật Ánh (sinh năm 1955) là một trong những nhà văn thành công và được yêu thích nhất tại Việt Nam chuyên viết về tuổi thơ và tuổi học trò. Ông là chủ nhân của hàng loạt tác phẩm best-seller như "Mắt Biếc", "Kính Vạn Hoa", "Cho Tôi Xin Một Vé Đi Tuổi Thơ", "Tôi Thấy Hoa Vàng Trên Cỏ Xanh".'
WHERE Name LIKE '%Nguyễn Nhật Ánh%';

-- 25. Kim Lân
UPDATE Authors SET 
    DateOfBirth = '1920-08-01', 
    Nationality = N'Việt Nam', 
    Biography = N'Kim Lân (1920 - 2007, tên thật Nguyễn Văn Tài) là cây bút tài hoa của dòng văn học hiện thực Việt Nam. Ông có vốn am hiểu sâu sắc về phong tục tập quán và vẻ đẹp tâm hồn của người nông dân Bắc Bộ, thể hiện qua các truyện ngắn kiệt tác như "Vợ Nhặt", "Làng", "Con Chó Xấu Xí".'
WHERE Name LIKE '%Kim Lân%';

-- 26. Nguyễn Minh Châu
UPDATE Authors SET 
    DateOfBirth = '1930-10-20', 
    Nationality = N'Việt Nam', 
    Biography = N'Nguyễn Minh Châu (1930 - 1989) là nhà văn tiên phong, "người mở đường tinh anh và tài năng nhất" của văn học Việt Nam thời kỳ đổi mới. Các tác phẩm nổi bật của ông gồm "Chiếc Thuyền Ngoài Xa", "Bến Quê", "Cỏ Lau" mang đậm trăn trở thế sự và chiều sâu nhân bản.'
WHERE Name LIKE '%Nguyễn Minh Châu%';

-- 27. Dan Brown
UPDATE Authors SET 
    DateOfBirth = '1964-06-22', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'Dan Brown (sinh năm 1964) là tác giả tiểu thuyết ly kỳ bí ẩn nổi tiếng toàn cầu người Mỹ. Ông là cha đẻ của các tác phẩm chấn động văn đàn kết hợp nghệ thuật, biểu tượng học và mật mã học như "Mật Mã Da Vinci" (The Da Vinci Code), "Thiên Thần Và Ác Quỷ", "Hỏa Ngục".'
WHERE Name LIKE '%Dan Brown%';

-- 28. Walter Isaacson
UPDATE Authors SET 
    DateOfBirth = '1952-05-20', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'Walter Isaacson (sinh năm 1952) là nhà văn, nhà báo và nguyên Chủ tịch CNN. Ông được mệnh danh là người viết tiểu sử xuất sắc nhất thời hiện đại, tác giả của các cuốn tiểu sử nổi tiếng thế giới về Steve Jobs, Albert Einstein, Leonardo da Vinci và Elon Musk.'
WHERE Name LIKE '%Walter Isaacson%';

-- 29. Daniel Kahneman
UPDATE Authors SET 
    DateOfBirth = '1934-03-05', 
    Nationality = N'Israel', 
    Biography = N'Daniel Kahneman (1934 - 2024) là nhà tâm lý học lỗi lạc người Mỹ gốc Israel đoạt giải Nobel Kinh tế năm 2002. Cuốn sách kiệt tác "Thinking, Fast and Slow" (Tư Duy Nhanh Và Chậm) của ông là công trình kinh điển về tâm lý học hành vi và kinh tế học nhận thức.'
WHERE Name LIKE '%Daniel Kahneman%';

-- 30. Stephen R. Covey
UPDATE Authors SET 
    DateOfBirth = '1932-10-24', 
    Nationality = N'Hoa Kỳ', 
    Biography = N'Stephen Richards Covey (1932 - 2012) là chuyên gia quản trị và phát triển lãnh đạo nổi tiếng của Mỹ. Cuốn sách kinh điển "The 7 Habits of Highly Effective People" (7 Thói Quen Của Người Thành Đạt) của ông đã bán hơn 40 triệu bản và được tạp chí Forbes xếp vào nhóm các cuốn sách quản trị có ảnh hưởng nhất thế kỷ 20.'
WHERE Name LIKE '%Stephen R. Covey%' OR Name LIKE '%Stephen Covey%';
