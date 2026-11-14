USE [SOF1022_NHOM1_SD2003_Ver2];
GO

-- 1. Fix Authors
UPDATE Authors SET 
    Biography = N'Cây bút hiện thực bậc thầy của nền văn học Việt Nam thế kỷ 20, tác giả Chí Phèo, Đời Thừa (1915 - 1951)'
WHERE Id = 81 OR Name = N'Nam Cao';

UPDATE Authors SET 
    Biography = N'Đại văn hào Mỹ, nhận giải Pulitzer năm 1953 và giải Nobel Văn học năm 1954 với kiệt tác Ông Già và Biển Cả (1899 - 1961)'
WHERE Id = 83 OR Name = N'Ernest Hemingway';

UPDATE Authors SET 
    Biography = N'Nhà văn Pháp nổi tiếng thế giới với những thiên truyện giàu lòng nhân ái như Không Gia Đình, Trong Gia Đình (1830 - 1907)'
WHERE Id = 84 OR Name = N'Hector Malot';

UPDATE Authors SET 
    Biography = N'Đại thi hào, nhà văn lãng mạn vĩ đại của nước Pháp với các kiệt tác Những Người Khốn Khổ, Nhà Thờ Đức Bà Paris (1802 - 1885)'
WHERE Id = 85 OR (Name = N'Victor Hugo' AND Biography LIKE '%Ã%');

UPDATE Authors SET 
    Biography = N'Nhà văn người Scotland nổi tiếng với bộ tiểu thuyết trinh thám kinh điển về thám tử đại tài Sherlock Holmes (1859 - 1930)'
WHERE Id = 86 OR Name = N'Arthur Conan Doyle';

UPDATE Authors SET 
    Biography = N'Nhà văn Indonesia nổi tiếng với tiểu thuyết truyền cảm hứng giáo dục Chiến Binh Cầu Vồng đạt kỷ lục xuất bản'
WHERE Id = 88 OR Name = N'Andrea Hirata';

-- 2. Fix Books descriptions
UPDATE Books SET 
    Description = N'Atomic Habits (Thay Đổi Tí Hon, Hiệu Quả Bất Ngờ) của James Clear phân tích sâu sắc khoa học hành vi con người và đưa ra quy trình 4 bước để xây dựng thói quen tốt và loại bỏ thói quen xấu: Làm cho rõ ràng, Hấp dẫn, Dễ dàng và Thỏa mãn. Chỉ với 1% tiến bộ tích cực mỗi ngày, bạn sẽ tạo nên sự chuyển biến phi thường trong cuộc sống.'
WHERE Title LIKE '%Atomic Habits%';

UPDATE Books SET 
    Description = N'Clean Code (Mã Sạch) của Robert C. Martin (Uncle Bob) là cuốn sách kinh điển dành cho mọi lập trình viên phần mềm chuyên nghiệp. Tác phẩm hướng dẫn cách viết mã nguồn dễ đọc, dễ hiểu, dễ kiểm thử và bảo trì thông qua các nguyên tắc đặt tên, thiết kế hàm, xử lý lỗi và cấu trúc lớp.'
WHERE Title LIKE '%Clean Code%';

UPDATE Books SET 
    Description = N'Sapiens: Lược Sử Loài Người của giáo sư Yuval Noah Harari đưa độc giả qua hành trình tiến hóa ngoạn mục của loài người từ thời kỳ đồ đá cho đến kỷ nguyên hiện đại. Tác phẩm phân tích sâu sắc cách mà khả năng tư duy trừu tượng, niềm tin tập thể, cuộc cách mạng nông nghiệp và khoa học đã đưa Homo sapiens trở thành loài thống trị hành tinh.'
WHERE Title LIKE '%Sapiens%';

UPDATE Books SET 
    Description = N'Cha Giàu Cha Nghèo (Rich Dad Poor Dad) của Robert Kiyosaki là cuốn sách tài chính cá nhân bán chạy nhất mọi thời đại, làm thay đổi tư duy tài chính của hàng triệu người trên thế giới về đầu tư, tài sản và tiêu sản.'
WHERE Title LIKE '%Cha Giàu Cha Nghèo%' OR Title LIKE '%Rich Dad%';

UPDATE Books SET 
    Description = N'1984 là kiệt tác văn học kinh điển của George Orwell khắc họa một xã hội tương lai giả tưởng đặt dưới sự giám sát toàn năng của Anh Cả (Big Brother). Tác phẩm cảnh báo sâu sắc về sự thao túng thông tin, bóp méo lịch sử, xâm phạm tự do cá nhân và sức mạnh kiểm soát tâm lý của các thể chế độc tài.'
WHERE Title LIKE '%1984%';

UPDATE Books SET 
    Description = N'Harry Potter và Tù nhân Azkaban là phần thứ ba trong bộ tiểu thuyết giả tưởng lừng danh của J.K. Rowling. Harry bước vào năm học thứ ba tại Hogwarts cùng bí mật về Sirius Black - tù nhân vượt ngục nguy hiểm nhất Azkaban.'
WHERE Title LIKE '%Tù nhân Azkaban%';

UPDATE Books SET 
    Description = N'Deep Work (Làm Ra Làm, Chơi Ra Chơi) của Cal Newport định hướng khả năng tập trung cao độ không xao nhãng trong một thế giới tràn ngập phiền nhiễu số.'
WHERE Title LIKE '%Deep Work%';

UPDATE Books SET 
    Description = N'Digital Minimalism (Tối Giản Kỹ Thuật Số) của Cal Newport đưa ra triết lý sử dụng công nghệ chọn lọc, tập trung vào giá trị cốt lõi và loại bỏ phiền nhiễu.'
WHERE Title LIKE '%Digital Minimalism%';

UPDATE Books SET 
    Description = N'Kafka bên bờ biển của Haruki Murakami là tác phẩm hiện thực huyền ảo xuất sắc, đưa người đọc vào cuộc phiêu lưu siêu thực của cậu bé 15 tuổi Kafka Tamura.'
WHERE Title LIKE '%Kafka%';

UPDATE Books SET 
    Description = N'Mắt Biếc là một trong những tác phẩm giàu cảm xúc nhất của nhà văn Nguyễn Nhật Ánh về tình yêu đơn phương trong sáng và day dứt của Ngạn dành cho Hà Lan.'
WHERE Title LIKE '%Mắt Biếc%';

UPDATE Books SET 
    Description = N'The Pragmatic Programmer của Andrew Hunt và David Thomas cung cấp những lời khuyên thực tế vô giá giúp lập trình viên nâng tầm tư duy và làm chủ nghề nghiệp.'
WHERE Title LIKE '%Pragmatic Programmer%';

UPDATE Books SET 
    Description = N'Refactoring (Tái Cấu Trúc Mã Nguồn) của Martin Fowler cung cấp danh mục các kỹ thuật cải tiến thiết kế mã nguồn mà không làm thay đổi hành vi bên ngoài.'
WHERE Title LIKE '%Refactoring%';

UPDATE Books SET 
    Description = N'Domain-Driven Design của Eric Evans giới thiệu phương pháp thiết kế phần mềm xoay quanh mô hình nghiệp vụ thực tế phức tạp.'
WHERE Title LIKE '%Domain Driven Design%' OR Title LIKE '%Domain-Driven Design%';

UPDATE Books SET 
    Description = N'Code Complete của Steve McConnell là cẩm nang toàn diện bậc nhất về kỹ nghệ xây dựng phần mềm chất lượng cao.'
WHERE Title LIKE '%Code Complete%';

UPDATE Books SET 
    Description = N'Working Effectively with Legacy Code của Michael Feathers giúp các kỹ sư thuần hóa và hiện đại hóa hệ thống mã nguồn cũ.'
WHERE Title LIKE '%Legacy Code%';

UPDATE Books SET 
    Description = N'Effective Java của Joshua Bloch là kim chỉ nam chứa 90 điều đúc kết giúp lập trình viên viết code Java tối ưu, an toàn và chuẩn mực.'
WHERE Title LIKE '%Effective Java%';

UPDATE Books SET 
    Description = N'Scrum: Nghệ Thuật Làm Việc Gấp Đôi Trong Nửa Thời Gian của Jeff Sutherland giới thiệu khung phương pháp linh hoạt (Agile) thành công nhất thế giới.'
WHERE Title LIKE '%Scrum%';

UPDATE Books SET 
    Description = N'The Phoenix Project đưa độc giả qua một câu chuyện sống động về ứng dụng DevOps và văn hóa hợp tác nhằm xoay chuyển tình thế dự án IT then chốt.'
WHERE Title LIKE '%Phoenix Project%';

UPDATE Books SET 
    Description = N'Algorithms của Robert Sedgewick và Kevin Wayne là giáo trình kinh điển về thuật toán và cấu trúc dữ liệu nền tảng.'
WHERE Title LIKE '%Algorithms%';

UPDATE Books SET 
    Description = N'Kubernetes in Action của Marko Luksa cung cấp hướng dẫn chuyên sâu từ cơ bản đến nâng cao về điều phối container trên nền tảng Kubernetes.'
WHERE Title LIKE '%Kubernetes in Action%';

UPDATE Books SET 
    Description = N'Microservices Patterns của Chris Richardson hướng dẫn toàn diện các mẫu kiến trúc microservices thực tế trong doanh nghiệp.'
WHERE Title LIKE '%Microservices Patterns%';

UPDATE Books SET 
    Description = N'Pride and Prejudice (Kiêu Hãnh và Định Kiến) của Jane Austen là kiệt tác văn học lãng mạn kinh điển của nước Anh thế kỷ 19.'
WHERE Title LIKE '%Pride and Prejudice%';

UPDATE Books SET 
    Description = N'Sense and Sensibility (Lý Trí và Tình Cảm) của Jane Austen khám phá tâm lý và lựa chọn của hai chị em Elinor và Marianne trước tình yêu và chuẩn mực xã hội.'
WHERE Title LIKE '%Sense and Sensibility%';

UPDATE Books SET 
    Description = N'The Great Gatsby (Gatsby Vĩ Đại) của F. Scott Fitzgerald khắc họa sâu sắc giấc mơ Mỹ hào nhoáng nhưng bi kịch trong thập niên 1920.'
WHERE Title LIKE '%Great Gatsby%';

UPDATE Books SET 
    Description = N'The Da Vinci Code (Mật Mã Da Vinci) của Dan Brown là tiểu thuyết trinh thám lịch sử tôn giáo kịch tính và hấp dẫn trên toàn thế giới.'
WHERE Title LIKE '%Da Vinci Code%';

UPDATE Books SET 
    Description = N'Origin (Nguồn Cội) của Dan Brown theo chân giáo sư Robert Langdon khám phá câu hỏi lớn nhất của nhân loại: Chúng ta đến từ đâu và chúng ta đang đi về đâu.'
WHERE Title LIKE '%Origin%';

UPDATE Books SET 
    Description = N'Số Đỏ là tác phẩm trào phúng kiệt xuất của văn học Việt Nam thế kỷ 20 của nhà văn Vũ Trọng Phụng. Tác phẩm xoay quanh nhân vật Xuân Tóc Đỏ từ một kẻ nhặt banh sân quần vợt hạ lưu, nhờ sự láu cá và làn sóng Âu hóa nhố nhăng của tầng lớp thượng lưu tư sản Hà thành thời thực dân nửa phong kiến mà thăng tiến vùn vụt thành đốc tờ, nhà cải cách xã hội và anh hùng cứu quốc.'
WHERE Title LIKE '%Số Đỏ%';

UPDATE Books SET 
    Description = N'Tắt Đèn là bức tranh hiện thực bi thảm và cảm động sâu sắc về đời sống cơ cực của người nông dân Việt Nam trước năm 1945 dưới ách sưu cao thuế nặng của chế độ thực dân. Hình tượng Chị Dậu là biểu tượng cao đẹp của người phụ nữ nông dân giàu đức hy sinh, tần tảo thương chồng thương con và tiềm tàng tinh thần quật khởi phản kháng áp bức.'
WHERE Title LIKE '%Tắt Đèn%';

UPDATE Books SET 
    Description = N'Chí Phèo là truyện ngắn hiện thực bất hủ của nhà văn Nam Cao. Tác phẩm kể về bi kịch đau đớn của người nông dân lương thiện bị nhà tù thực dân và sự tàn bạo của tầng lớp cường hào làng Vũ Đại hủy hoại cả nhân hình lẫn nhân tính, biến thành con quỷ dữ. Cuộc gặp gỡ với Thị Nở cùng bát cháo hành ấm áp tình người đã đánh thức khát khao lương thiện nhưng cánh cửa trở về xã hội loài người đã vĩnh viễn khép lại.'
WHERE Title LIKE '%Chí Phèo%';

UPDATE Books SET 
    Description = N'Đoạn Trường Tân Thanh (Truyện Kiều) của Đại thi hào Nguyễn Du là đỉnh cao chói lọi nhất của nền thi ca và ngôn ngữ dân tộc Việt Nam gồm 3.254 câu thơ lục bát tài hoa. Tác phẩm ngợi ca vẻ đẹp nhan sắc, trí tuệ và tấm lòng hiếu nghĩa thủy chung của nàng Thúy Kiều, đồng thời cất lên tiếng kêu thương ai oán cho số phận con người và tố cáo thế lực đồng tiền tàn bạo.'
WHERE Title LIKE '%Truyện Kiều%';

UPDATE Books SET 
    Description = N'Hoàng Tử Bé (Le Petit Prince) của nhà văn kiêm phi công Antoine de Saint-Exupéry là tác phẩm văn học thiếu nhi và triết lý kinh điển được dịch ra hơn 300 ngôn ngữ trên thế giới. Tác phẩm gửi gắm thông điệp bất hủ về tình yêu thương, sự thuần khiết và trách nhiệm với những gì ta đã gắn kết: Người ta chỉ nhìn thấy thật rõ ràng bằng trái tim; điều cốt lõi thì mắt trần không thể thấy được.'
WHERE Title LIKE '%Hoàng Tử Bé%';

UPDATE Books SET 
    Description = N'Ông Già Và Biển Cả (The Old Man and the Sea) là kiệt tác đoạt giải Nobel Văn học 1954 của Ernest Hemingway. Với phong cách nguyên lý tảng băng trôi độc đáo, tác phẩm kể về chuyến ra khơi đơn độc kéo dài 84 ngày của lão ngư Santiago và cuộc chiến đấu nghẹt thở với con cá kiếm khổng lồ giữa vịnh Mexico, đúc kết chân lý hào hùng: Con người sinh ra không phải để dành cho thất bại; con người có thể bị hủy diệt nhưng không thể bị đánh bại.'
WHERE Title LIKE '%Ông Già Và Biển Cả%';

UPDATE Books SET 
    Description = N'Chiến Binh Cầu Vồng (The Rainbow Troops) của Andrea Hirata là câu chuyện có thật đầy xúc động về 10 học trò nghèo tại ngôi trường xập xệ trên đảo Belitong (Indonesia) cùng cô giáo trẻ Muhammadiyah hết lòng vì học trò. Bất chấp sự nghèo đói và đe dọa đóng cửa trường học, các em đã kiên cường vượt khó bằng niềm tin mãnh liệt vào sức mạnh giải phóng của giáo dục và con chữ.'
WHERE Title LIKE '%Chiến Binh Cầu Vồng%';
GO
