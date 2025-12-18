USE master
IF DB_ID('CNPM_DATABASE_THUVIEN') IS NOT NULL
BEGIN
    DROP DATABASE CNPM_DATABASE_THUVIEN
END
GO

CREATE DATABASE CNPM_DATABASE_THUVIEN
GO
USE CNPM_DATABASE_THUVIEN
GO

CREATE TABLE TACGIA
(
    MATG CHAR(7) PRIMARY KEY,
    TENTG NVARCHAR(50) NOT NULL,
    NGAYSINH DATE CHECK(NGAYSINH < GETDATE()),
    QUOCTICH NVARCHAR(50) DEFAULT N'Việt Nam'
)

CREATE TABLE THELOAI
(
    MATHELOAI CHAR(7) PRIMARY KEY,
    TENTHELOAI NVARCHAR(50) UNIQUE NOT NULL,
    MOTA NVARCHAR(255) DEFAULT N'Chưa có mô tả'
);

CREATE TABLE NHAXUATBAN
(
    MAXB CHAR(7) PRIMARY KEY,
    TENNXB NVARCHAR(100) UNIQUE NOT NULL,
    DCHI NVARCHAR(200) NOT NULL,
    SDT CHAR(10) CHECK (SDT LIKE '%[0-9]%')
);
CREATE TABLE QLSACH
(
    MASACH CHAR(7) PRIMARY KEY,
    TENSACH NVARCHAR(50)NOT NULL,
    MATG CHAR(7),
    MATHELOAI CHAR(7),
    MAXB CHAR(7),
    NAMXB INT CHECK(NAMXB <= YEAR(GETDATE())),
    SL INT CHECK(SL >= 0),
    TINHTRANG INT CHECK(TINHTRANG >= 0),
    MOTA NVARCHAR(MAX) DEFAULT N'Đang cập nhật nội dung giới thiệu...',
    FOREIGN KEY (MATG) REFERENCES TACGIA (MATG),
    FOREIGN KEY (MATHELOAI) REFERENCES THELOAI (MATHELOAI),
    FOREIGN KEY (MAXB) REFERENCES NHAXUATBAN (MAXB),
    CONSTRAINT CK_TINHTRANG CHECK (TINHTRANG <= SL)
)

CREATE TABLE BIASACH
(
    MASACH CHAR(7) PRIMARY KEY,
    URL_ANH NVARCHAR(100),
    FOREIGN KEY (MASACH) REFERENCES QLSACH(MASACH)
)

CREATE TABLE TINTUC (
    MaTin INT IDENTITY(1,1) PRIMARY KEY,
    TieuDe NVARCHAR(255) NOT NULL,
    MoTaNgan NVARCHAR(500),
    HinhAnh NVARCHAR(255),
    NgayDang DATETIME DEFAULT GETDATE(),
    LoaiTin INT, 
    HienThi BIT DEFAULT 1,
    Link VARCHAR(500),
);

CREATE TABLE ROLE_USER
(
    ROLE_ID INT IDENTITY(1,1) PRIMARY KEY,
    ROLE_NAME NVARCHAR(20) UNIQUE, 
    DESCRIPT NVARCHAR(200)
)

CREATE TABLE TAIKHOAN
(
    USERNAME CHAR(7) PRIMARY KEY, 
    PASS VARBINARY(64) DEFAULT HASHBYTES('SHA2_256', '12345'),
    ROLE_ID INT,
    FOREIGN KEY (ROLE_ID) REFERENCES ROLE_USER(ROLE_ID)
)

CREATE TABLE DOCGIA
(
    MADG CHAR(7) PRIMARY KEY,
    TENDG NVARCHAR(30) NOT NULL,
    KHOA NVARCHAR(50), 
    LOP NVARCHAR(50),
    DIACHI NVARCHAR(100),
    SODT CHAR(10) UNIQUE CHECK (SODT NOT LIKE '%[^0-9]%'),
    MAIL NVARCHAR(100) UNIQUE CHECK (MAIL LIKE '%@%') 
);

CREATE TABLE THETHUVIEN
(
    MATHE CHAR(7) PRIMARY KEY,
    MADG CHAR(7) NULL UNIQUE,
    NGAYCAP DATE,
    NGAYHETHAN DATE,
    TRANGTHAI NVARCHAR(20) DEFAULT N'Hoạt động',
    FOREIGN KEY (MADG) REFERENCES DOCGIA (MADG),
    CONSTRAINT CK_NGAYHETHAN CHECK (NGAYHETHAN > NGAYCAP)
);

CREATE TABLE QLNHANVIEN
(
    MANV CHAR(7) PRIMARY KEY,                       
    TENNV NVARCHAR(50) NOT NULL,                        
    NGSINH DATE CHECK (NGSINH < GETDATE()),               
    CHUCVU NVARCHAR(50) NOT NULL,
    SDIENTHOAI CHAR(10) UNIQUE CHECK (SDIENTHOAI LIKE '0%'), 
    MAIL NVARCHAR(100) UNIQUE CHECK (MAIL LIKE '%@%')        
);

CREATE TABLE PHIEUMUON
(
    MAPM CHAR(7) PRIMARY KEY,
    MATHE CHAR(7) NOT NULL,
    MANV CHAR(7) NULL,
    NgayMuon DATE DEFAULT GETDATE(),
    NgayDenHan DATE,
    TINHTRANG INT DEFAULT 0,
    FOREIGN KEY (MATHE) REFERENCES THETHUVIEN(MATHE),
    FOREIGN KEY (MANV) REFERENCES QLNHANVIEN(MANV),
    CONSTRAINT CK_NgayHan CHECK (NgayDenHan > NgayMuon)
);

CREATE TABLE CHITIETPM
( 
    MAPM CHAR(7),
    MASACH CHAR(7) NOT NULL,
    SLMUON INT CHECK (SLMUON > 0),
    TIENTHECHAN MONEY,
    TINHTRANG INT DEFAULT 0,
    PRIMARY KEY (MAPM,MASACH),
    FOREIGN KEY (MAPM) REFERENCES PHIEUMUON (MAPM),
    FOREIGN KEY (MASACH) REFERENCES QLSACH (MASACH)
);

CREATE TABLE PHIEUTRA
( 
    MAPT CHAR(7) PRIMARY KEY,
    MAPM CHAR(7) NOT NULL UNIQUE,
    MANV CHAR(7) NOT NULL,
    NGAYTRA DATE DEFAULT GETDATE(),
    TONG_TIENTHECHAN MONEY CHECK (TONG_TIENTHECHAN >= 0),
    TIENPHAT MONEY DEFAULT 0 CHECK (TIENPHAT >= 0),
    FOREIGN KEY (MAPM) REFERENCES PHIEUMUON (MAPM),
    FOREIGN KEY (MANV) REFERENCES QLNHANVIEN (MANV)
);

CREATE TABLE VIPHAM
(
    MAVP INT IDENTITY(1,1) PRIMARY KEY,
    MAPM CHAR(7) NOT NULL, 
    MASACH CHAR(7) NULL, 
    HINHTHUCVP NVARCHAR(50) NOT NULL CHECK (HINHTHUCVP IN (N'Trễ hẹn',N'Hỏng sách',N'Mất sách')),
    CHITIETVP NVARCHAR(500),
    SOTIENPHAT MONEY CHECK (SOTIENPHAT >= 0),
    NGAYGHINHAN DATE DEFAULT GETDATE(),
    FOREIGN KEY (MAPM) REFERENCES PHIEUMUON (MAPM)
);

CREATE TABLE PHONGHOP
(
    MAPHONG CHAR(7) NOT NULL PRIMARY KEY,
    VITRI NVARCHAR(20) NOT NULL CHECK (VITRI IN (N'Tầng 1',N'Tầng 2',N'Tầng 3',N'Tầng 4')),
    SL_NGUOITOIDA INT CHECK (SL_NGUOITOIDA > 0),
    TINHTRANG INT CHECK (TINHTRANG = 0 OR TINHTRANG = 1)
)

CREATE TABLE PHIEU_MUONPHONG
(
    MAPHIEU CHAR(7) NOT NULL PRIMARY KEY,
    MATHE CHAR(7) NOT NULL,
    MAPH CHAR(7) NOT NULL,
    NGAYMUON DATE,
    GIOMUON TIME,
    GIOTRA TIME,
    SL_NGUOITHAMGIA INT CHECK (SL_NGUOITHAMGIA > 0),
    MUCDICH NVARCHAR(500),
    TIENPHAT MONEY DEFAULT 0 CHECK (TIENPHAT >= 0),
    TINHTRANG INT DEFAULT 0,
    GHICHU_NV NVARCHAR(200),
    FOREIGN KEY(MAPH) REFERENCES PHONGHOP(MAPHONG),
    FOREIGN KEY (MATHE) REFERENCES THETHUVIEN(MATHE)
)
GO

CREATE TABLE THAMSO (
    TENTHAMSO VARCHAR(50) PRIMARY KEY, 
    GIATRI INT,                        
    MOTA NVARCHAR(200)                 
);

INSERT INTO ROLE_USER VALUES
(N'Admin',N'Toàn quyền trên DB'),
(N'Librarian',N'Quản lý độc giả, sách, mượn trả sách, phòng họp'),
(N'Reader',N'Xem sách, mượn sách, phòng họp')
GO

INSERT INTO TACGIA VALUES
('TG00001', N'Allen B. Downey', '1970-01-01', N'Mỹ'),
('TG00002', N'Andrew S. Tanenbaum', '1944-03-16', N'Hà Lan'),
('TG00003', N'Ian Goodfellow', '1985-01-01', N'Mỹ'),
('TG00004', N'Haykin Simon', '1942-01-01', N'Canada'),
('TG00005', N'Nguyễn Thanh Tùng', '1978-05-12', N'Việt Nam'),
('TG00006', N'Tristan Nguyen', '1981-04-21', N'Việt Nam'),
('TG00007', N'Daniel Goleman', '1946-03-07', N'Mỹ'),
('TG00008', N'Dale Carnegie', '1888-11-24', N'Mỹ'),
('TG00009', N'Nguyễn Trung Hiếu', '1983-09-09', N'Việt Nam'),
('TG00010', N'Hồ Trung Thành', '1972-02-22', N'Việt Nam'),
('TG00011', N'Phạm Quốc Bảo', '1985-12-01', N'Việt Nam'),
('TG00012', N'Lê Thị Mỹ Hạnh', '1987-06-14', N'Việt Nam'),
('TG00013', N'Huỳnh Hữu Tuấn', '1989-08-18', N'Việt Nam'),
('TG00014', N'Nguyễn Hoàng Dũng', '1990-04-04', N'Việt Nam'),
('TG00015', N'Elon Musk', '1971-06-28', N'Mỹ');

INSERT INTO THELOAI VALUES
('TL00001', N'Sách chuyên ngành', N'Sách phục vụ nghiên cứu'),
('TL00002', N'Giáo trình', N'Sách phục vụ học tập'),
('TL00003', N'Tài liệu tham khảo', N'Tài liệu dùng để tra cứu'),
('TL00004', N'Sách kĩ năng', N'Sách phát triển bản thân'),
('TL00005', N'Luận văn - Khóa luận', N'Bài làm cuối khóa'),
('TL00006', N'Tạp chí khoa học', N'Tạp chí chuyên ngành khoa học');

INSERT INTO NHAXUATBAN VALUES
('NXB0001', N'NXB Giáo Dục', N'Hà Nội', '0283456789'),
('NXB0002', N'NXB Trẻ', N'TP.HCM', '0283456790'),
('NXB0003', N'NXB Khoa Học Tự Nhiên', N'Hà Nội', '0243876543'),
('NXB0004', N'NXB Đại Học Quốc Gia', N'TP.HCM', '0283451234'),
('NXB0005', N'NXB Reilly', N'Hoa Kỳ', '0012345678'),
('NXB0006', N'NXB McGraw-Hill', N'Hoa Kỳ', '0018765432'),
('NXB0007', N'NXB Pearson', N'Anh Quốc', '0044556677'),
('NXB0008', N'NXB DataSci Press', N'Singapore', '0066123456'),
('NXB0009', N'NXB Kinh Tế', N'Hà Nội', '0243688011'),
('NXB0010', N'NXB Tự Lực', N'TP.HCM', '0284562391');

INSERT INTO QLSACH VALUES
('S000001', N'Think Python', 'TG00001', 'TL00001', 'NXB0005', 2015, 12, 12, N'Cuốn sách kinh điển về lập trình Python, phù hợp cho người mới bắt đầu tìm hiểu về tư duy lập trình.'),
('S000002', N'Computer Networks', 'TG00002', 'TL00001', 'NXB0007', 2011, 10, 9, N'Giáo trình mạng máy tính căn bản, bao gồm mô hình OSI, TCP/IP và các giao thức mạng phổ biến.'),
('S000003', N'Deep Learning', 'TG00003', 'TL00001', 'NXB0006', 2016, 8, 8, N'Tài liệu chuyên sâu về Deep Learning, cung cấp kiến thức nền tảng toán học và các kiến trúc mạng nơ-ron.'),
('S000004', N'Neural Networks and Learning Machines', 'TG00004', 'TL00001', 'NXB0006', 2008, 7, 7, N'Tài liệu học thuật toàn diện về mạng nơ-ron, từ cơ bản đến nâng cao, bao gồm các giải thuật học máy.'),
('S000005', N'Giáo trình Cơ sở dữ liệu', 'TG00005', 'TL00002', 'NXB0001', 2020, 20, 19, N'Cung cấp kiến thức nền tảng về hệ quản trị cơ sở dữ liệu, ngôn ngữ truy vấn SQL và thiết kế dữ liệu chuẩn hóa.'),
('S000006', N'Giáo trình Mạng máy tính', 'TG00010', 'TL00002', 'NXB0004', 2021, 15, 14, N'Tài liệu giảng dạy về nguyên lý mạng, kiến trúc phân tầng và các kỹ thuật truyền thông số liệu.'),
('S000007', N'Giáo trình Thuật toán', 'TG00011', 'TL00002', 'NXB0003', 2019, 10, 10, N'Tổng hợp các cấu trúc dữ liệu cơ bản và giải thuật quan trọng, kèm theo phân tích độ phức tạp.'),
('S000008', N'Phân tích dữ liệu với Python', 'TG00006', 'TL00003', 'NXB0008', 2022, 12, 11, N'Hướng dẫn sử dụng thư viện Pandas, NumPy và Matplotlib để xử lý, trực quan hóa và khai phá dữ liệu.'),
('S000009', N'Hướng dẫn lập trình C++', 'TG00009', 'TL00003', 'NXB0001', 2018, 18, 17, N'Sách hướng dẫn chi tiết về lập trình hướng đối tượng, quản lý bộ nhớ và các kỹ thuật tối ưu hóa trong C++.'),
('S000010', N'Tài liệu học máy nâng cao', 'TG00004', 'TL00003', 'NXB0006', 2021, 9, 9, N'Tài liệu chuyên sâu dành cho nghiên cứu, tập trung vào các mô hình phức tạp và tối ưu hóa thuật toán.'),
('S000011', N'Emotional Intelligence', 'TG00007', 'TL00004', 'NXB0006', 1995, 10, 9, N'Khám phá vai trò của trí tuệ cảm xúc (EQ) trong việc điều hướng xã hội và quản lý cảm xúc cá nhân.'),
('S000012', N'How to Win Friends and Influence People', 'TG00008', 'TL00004', 'NXB0010', 1936, 20, 20, N'Cuốn sách nghệ thuật sống kinh điển, chia sẻ các nguyên tắc giao tiếp và xây dựng mối quan hệ.'),
('S000013', N'Sức mạnh tư duy tích cực', 'TG00012', 'TL00004', 'NXB0002', 2017, 25, 24, N'Sách hướng dẫn về tư duy tích cực, giúp thay đổi thái độ sống và đạt được thành công trong công việc.'),
('S000014', N'Khóa luận ứng dụng AI trong xử lý ảnh', 'TG00013', 'TL00005', 'NXB0004', 2023, 3, 3, N'Nghiên cứu ứng dụng Deep Learning và Computer Vision trong việc tự động hóa phân tích hình ảnh.'),
('S000015', N'Luận văn nhận diện chữ viết tay', 'TG00014', 'TL00005', 'NXB0004', 2022, 2, 2, N'Đề tài nghiên cứu về các kỹ thuật nhận dạng mẫu (OCR) và xử lý ảnh để số hóa văn bản viết tay.'),
('S000016', N'Khóa luận phân tích dữ liệu lớn', 'TG00012', 'TL00005', 'NXB0009', 2023, 4, 4, N'Nghiên cứu về các công nghệ lưu trữ và xử lý dữ liệu quy mô lớn (Big Data) như Hadoop và Spark.'),
('S000017', N'Journal of Machine Learning Research – Vol 1', 'TG00003', 'TL00006', 'NXB0006', 2019, 5, 5, N'Tập hợp các bài báo nghiên cứu hàn lâm chất lượng cao về những tiến bộ mới nhất trong lĩnh vực học máy.'),
('S000018', N'Journal of Data Science – Vol 2', 'TG00006', 'TL00006', 'NXB0008', 2020, 6, 6, N'Tạp chí chuyên ngành công bố các phương pháp luận và ứng dụng thực tiễn trong khoa học dữ liệu.'),
('S000019', N'Vietnam Journal of Science – Số 15', 'TG00009', 'TL00006', 'NXB0003', 2022, 4, 4, N'Ấn phẩm khoa học công nghệ, đăng tải các công trình nghiên cứu và triển khai ứng dụng tại Việt Nam.'),
('S000020', N'International AI Review – Vol 5', 'TG00003', 'TL00006', 'NXB0006', 2021, 7, 7, N'Tạp chí quốc tế tổng hợp các đánh giá và xu hướng phát triển mới nhất của trí tuệ nhân tạo toàn cầu.');

INSERT INTO BIASACH VALUES
('S000001', 'Sach1.jpg'), ('S000002', 'Sach2.jpg'), ('S000003', 'Sach3.jpg'), ('S000004', 'Sach4.jpg'),
('S000005', 'Sach5.jpg'), ('S000006','Sach6jpg'), ('S000007','Sach7.jpg'), ('S000008','Sach8.jpg'),
('S000009', 'Sach9.jpg'), ('S000010', 'Sach10.jpg'), ('S000011', 'Sach11.jpg'), ('S000012', 'Sach12.jpg'),
('S000013', 'Sach13.jpg'), ('S000014', 'Sach14.jpg'), ('S000015', 'Sach15.jpg'), ('S000016', 'Sach16.jpg'),
('S000017', 'Sach17.jpg'), ('S000018', 'Sach18.jpg'), ('S000019', 'Sach19.jpg'), ('S000020', 'Sach20.jpg');

INSERT INTO DOCGIA VALUES
('DG00001',N'Đỗ Thành Trung',N'Khoa Công Nghệ Thông Tin',N'14DHTH12',N'TP.HCM','0912345678','dothanhtrunf@gmail.com'),
('DG00002',N'Lê Thị Châu',N'Khoa Quản trị Kinh doanh',N'13DHKD',N'Long An','0914425125','chaithile@gmail.com'),
('DG00003',N'Nguyễn Hữu Cường',N'Khoa Tài chính - Kế toán',N'15DHKT',N'Binh Dương','0284470967','huucuong@gmail.com'),
('DG00004',N'Bùi Đắc Hoàng Phước',N'Khoa Ngoại Ngữ',N'14DHAV',N'TP.HCM','0922509415','hoangphuoc@gmail.com'),
('DG00005',N'Đinh Xuân Cường',N'Khoa Công nghệ Cơ khí',N'14DHCK',N'TP.HCM','0357721341','cuong123@gmail.com'),
('DG00006',N'Ngô Thị Hoa',N'Khoa Công nghệ Thực phẩm',N'12DHTP',N'An Giang','0967812443','hoangothi@gmail.com'),
('DG00007',N'Vũ Minh Hùng',N'Khoa Công nghệ Điện - Điện tử',N'14DHDT',N'Tiền Giang','0284043311','hungvu1@gmail.com'),
('DG00008',N'Bùi Thị Lan',N'Khoa Sinh học và Môi trường',N'12DHMT',N'TP.HCM','0354450010','buithilan@gmail.com'),
('DG00009',N'Đỗ Mạnh Quân',N'Khoa Công nghệ May và Thời trang',N'14DHTT',N'Long An','0990126641','doquan123@gmail.com'),
('DG00010',N'Phan Thị Thảo',N'Khoa Lý Luận Chính Trị',N'15DGCT',N'Bến Tre','0901254337','janny11@gmail.com');

INSERT INTO THETHUVIEN VALUES
('TH00001','DG00001','2025-01-15','2027-01-15',N'Hoạt động'),
('TH00002','DG00002','2025-02-10','2027-02-10',N'Hoạt động'),
('TH00003','DG00003','2025-03-05','2027-03-05',N'Hoạt động'),
('TH00004','DG00004','2025-04-20','2027-04-20',N'Hoạt động'),
('TH00005','DG00005','2025-05-12','2027-05-12',N'Hoạt động'),
('TH00006','DG00006','2025-06-18','2027-06-18',N'Hoạt động'),
('TH00007','DG00007','2025-07-25','2026-07-25',N'Hoạt động'),
('TH00008','DG00008','2024-08-30','2026-08-30',N'Hoạt động'),
('TH00009','DG00009','2025-09-14','2027-09-14',N'Hoạt động'),
('TH00010','DG00010','2023-10-01','2026-10-01',N'Hoạt động');

INSERT INTO QLNHANVIEN VALUES
('NV00001',N'Nguyễn Văn A','1990-05-10',N'Thủ thư','0911222333','vana@gmail.com'),
('NV00002',N'Trần Thị B','1992-07-20',N'Quản lý','0911333444','thib@gmail.com'),
('NV00003',N'Lê Văn C','1988-03-15',N'Thủ thư','0911444555','vanc@gmail.com');

INSERT INTO PHIEUMUON VALUES
('PM00001','TH00001','NV00001','2023-10-01','2023-10-15', 1),
('PM00002','TH00002','NV00002','2023-06-01','2023-06-15', 0),
('PM00003','TH00003','NV00003','2023-07-15','2023-07-30', 0);

INSERT INTO CHITIETPM VALUES
('PM00001','S000001',2,20000, 1),
('PM00001','S000002',1,10000, 1),
('PM00002','S000003',1,10000, 0),
('PM00003','S000001',1,10000, 0),
('PM00003','S000004',3,30000, 0);

INSERT INTO PHIEUTRA VALUES
('PT00001','PM00001','NV00001','2023-10-14',30000,0),
('PT00002','PM00002','NV00003','2023-06-11',10000,200000),
('PT00003','PM00003','NV00002','2023-08-02',40000,100000);

INSERT INTO VIPHAM (MAPM, MASACH, HINHTHUCVP, CHITIETVP, SOTIENPHAT) VALUES
('PM00003', NULL, N'Trễ hẹn', N'Trả trễ 3 ngày so với ngày đến hạn (30/07/2023).', 30000),
('PM00002', 'S000003', N'Hỏng sách', N'Bị ướt và rách 2 trang đầu sách Deep Learning.', 170000);

INSERT INTO PHONGHOP VALUES 
('PH00001',N'Tầng 3',10,1),
('PH00002',N'Tầng 3',8,1),
('PH00003',N'Tầng 3',12,1),
('PH00004',N'Tầng 3',8,1);

INSERT INTO PHIEU_MUONPHONG VALUES
('MP00001','TH00001','PH00001','2025/02/11','09:50:00','10:50:00',6,N'Họp nhóm',0,0, NULL),
('MP00002','TH00002','PH00001','2025/10/14','12:30:00','13:50:00',6,N'Họp nhóm và thảo luận đồ án với giảng viên hướng dẫn',0,0, NULL),
('MP00003','TH00001','PH00001','2025/02/11','09:50:00','10:50:00',6,N'Báo cáo đồ án cuối kì online',0,0, NULL),
('MP00004','TH00001','PH00001','2025/02/11','09:50:00','10:50:00',6,N'Họp nhóm',0,0, NULL);

INSERT INTO TAIKHOAN VALUES 
('admin', HASHBYTES('SHA2_256', '12345'), 1)
GO

INSERT INTO TINTUC (TieuDe, MoTaNgan, HinhAnh, NgayDang, LoaiTin, HienThi, Link) VALUES 
(N'Thư viện Trường ĐH Công Thương TP. HCM tiếp nhận hơn 1.000 quyển sách ngoại văn', 
 N'Ngày sách và Văn hóa đọc Việt Nam năm 2025 - Lan tỏa tri thức', 
 'news_tiepnhan.jpg', '2025-04-20', 1, 1, 
 'https://thuvien.huit.edu.vn/News/NewDetail/tiep-nhan-sach-ngoai-van'),
(N'Trao giải Cuộc thi "Đại sứ văn hóa đọc HUIT" và "Xếp sách nghệ thuật"', 
 N'Ngày sách đồng hành cùng sinh viên - Tôn vinh văn hóa đọc', 
 'news_traogiai.jpg', '2024-04-21', 1, 1, 
 'https://thuvien.huit.edu.vn/News/NewDetail/trao-giai-dai-su-van-hoa-doc'),
(N'Hướng dẫn sử dụng & tra cứu các nguồn CSDL Thư viện HUIT', 
 N'Thông báo dùng thử Cơ sở dữ liệu EBSCO miễn phí', 
 'notice_huongdan.jpg', '2025-01-10', 2, 1, 
 'https://thuvien.huit.edu.vn/News/NewDetail/huong-dan-tra-cuu-csdl');
GO
INSERT INTO THAMSO VALUES ('SoSachToiDa', 5, N'Số sách tối đa được mượn');
INSERT INTO THAMSO VALUES ('SoNgayMuon', 7, N'Số ngày được phép mượn');
INSERT INTO THAMSO VALUES ('TienPhat', 5000, N'Tiền phạt quá hạn mỗi ngày (VNĐ)');

CREATE PROCEDURE SP_REGISTER_DOCGIA
    @TENDG NVARCHAR(30),
    @KHOA NVARCHAR(50) = NULL,
    @LOP NVARCHAR(50) = NULL,
    @DIACHI NVARCHAR(100),
    @SODT CHAR(10),
    @MAIL NVARCHAR(100),
    @MATHE_OUT CHAR(7) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM DOCGIA WHERE SODT = @SODT)
    BEGIN
        THROW 50001, N'Số điện thoại này đã tồn tại trong hệ thống! Vui lòng kiểm tra lại.', 1;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM DOCGIA WHERE MAIL = @MAIL)
    BEGIN
        THROW 50002, N'Email này đã được đăng ký bởi người khác!', 1;
        RETURN;
    END

    DECLARE @MADG CHAR(7);
    EXEC AUTO_ID_DOCGIA @MADG OUTPUT;

    INSERT INTO DOCGIA (MADG, TENDG, KHOA, LOP, DIACHI, SODT, MAIL)
    VALUES (@MADG, @TENDG, @KHOA, @LOP, @DIACHI, @SODT, @MAIL);

    DECLARE @MATHE CHAR(7);
    EXEC AUTO_ID_THETHUVIEN @MATHE OUTPUT;
     
    DECLARE @NGAYCAP DATE = GETDATE();
    DECLARE @NGAYHETHAN DATE = DATEADD(YEAR, 2, @NGAYCAP);

    INSERT INTO THETHUVIEN (MATHE, MADG, NGAYCAP, NGAYHETHAN, TRANGTHAI)
    VALUES (@MATHE, @MADG, @NGAYCAP, @NGAYHETHAN, N'Hoạt động');

    SET @MATHE_OUT = @MATHE;
END
GO

CREATE PROCEDURE AUTO_CREATE_ID_TACGIA @MATG CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MATG, 3, 5) AS INT)) FROM TACGIA;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @MATG = 'TG' + RIGHT('00000' + CAST(@MAX_ID + 1 AS VARCHAR(5)), 5);
END
GO

CREATE PROCEDURE AUTO_ID_THELOAI @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MATHELOAI, 3, 5) AS INT)) FROM THELOAI;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'TL' + RIGHT('00000' + CAST(@MAX_ID + 1 AS VARCHAR(5)), 5);
END;
GO

CREATE PROCEDURE AUTO_ID_NXB @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MAXB, 4, 4) AS INT)) FROM NHAXUATBAN;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'NXB' + RIGHT('0000' + CAST(@MAX_ID + 1 AS VARCHAR(4)), 4);
END;
GO

CREATE PROCEDURE AUTO_ID_SACH @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MASACH, 2, 6) AS INT)) FROM QLSACH;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'S' + RIGHT('000000' + CAST(@MAX_ID + 1 AS VARCHAR(6)), 6);
END;
GO

CREATE PROCEDURE AUTO_ID_DOCGIA @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MADG, 3, 5) AS INT)) FROM DOCGIA;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'DG' + RIGHT('00000' + CAST(@MAX_ID + 1 AS VARCHAR(5)), 5);
END;
GO

CREATE PROCEDURE AUTO_ID_THETHUVIEN @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MATHE, 3, 5) AS INT)) FROM THETHUVIEN;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'TH' + RIGHT('00000' + CAST(@MAX_ID + 1 AS VARCHAR(5)), 5);
END;
GO

CREATE PROCEDURE AUTO_ID_NHANVIEN @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MANV, 3, 5) AS INT)) FROM QLNHANVIEN;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'NV' + RIGHT('00000' + CAST(@MAX_ID + 1 AS VARCHAR(5)), 5);
END;
GO

CREATE PROCEDURE AUTO_ID_PHIEUTRA @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MAPT, 3, 5) AS INT)) FROM PHIEUTRA;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'PT' + RIGHT('00000' + CAST(@MAX_ID + 1 AS VARCHAR(5)), 5);
END;
GO

CREATE PROCEDURE AUTO_ID_PHONG @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MAPHONG, 3, 5) AS INT)) FROM PHONGHOP;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'PH' + RIGHT('00000' + CAST(@MAX_ID + 1 AS VARCHAR(5)), 5);
END;
GO

CREATE PROCEDURE AUTO_ID_PHIEUMUONPHONG @NEWID CHAR(7) OUTPUT
AS
BEGIN
    DECLARE @MAX_ID INT;
    SELECT @MAX_ID = MAX(CAST(SUBSTRING(MAPHIEU, 3, 5) AS INT)) FROM PHIEU_MUONPHONG;
    IF @MAX_ID IS NULL SET @MAX_ID = 0;
    SET @NEWID = 'MP' + RIGHT('00000' + CAST(@MAX_ID + 1 AS VARCHAR(5)), 5);
END;
GO

CREATE TRIGGER UPDATE_SLC_SAUKHI_MUON ON CHITIETPM
AFTER INSERT 
AS
BEGIN
    DECLARE @SL INT, @MASACH CHAR(7);

    DECLARE cur_add_slc CURSOR FOR
        SELECT SLMUON, MASACH
        FROM inserted;

    OPEN cur_add_slc;
    FETCH NEXT FROM cur_add_slc INTO @SL, @MASACH;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        UPDATE QLSACH
        SET TINHTRANG = TINHTRANG - @SL
        WHERE MASACH = @MASACH

        FETCH NEXT FROM cur_add_slc INTO @SL, @MASACH;
    END 
    CLOSE cur_add_slc;
    DEALLOCATE cur_add_slc;
END
GO

CREATE TRIGGER TRG_PHIEUTRA
ON PHIEUTRA
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE S
    SET S.TINHTRANG = S.TINHTRANG + CT.SLMUON
    FROM QLSACH S
    INNER JOIN CHITIETPM CT ON S.MASACH = CT.MASACH
    INNER JOIN inserted I ON CT.MAPM = I.MAPM;
END;
GO

CREATE TRIGGER AUTO_CREATE_ACC_READER ON THETHUVIEN 
AFTER INSERT 
AS 
BEGIN
    DECLARE @MATHE CHAR(7);

    DECLARE cur CURSOR FOR
        SELECT MATHE
        FROM inserted;

    OPEN cur;
    FETCH NEXT FROM cur INTO @MATHE;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM TAIKHOAN WHERE USERNAME = @MATHE)
        BEGIN
            INSERT INTO TAIKHOAN (USERNAME, ROLE_ID)
            VALUES (@MATHE, 3);
        END

        FETCH NEXT FROM cur INTO @MATHE;
    END
    CLOSE cur
    DEALLOCATE cur
END
GO

CREATE TRIGGER AUTO_CREATE_ACC_LIB ON QLNHANVIEN 
AFTER INSERT 
AS 
BEGIN
    DECLARE @MANV CHAR(7);

    DECLARE cur CURSOR FOR
        SELECT MANV
        FROM inserted;

    OPEN cur;
    FETCH NEXT FROM cur INTO @MANV;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM TAIKHOAN WHERE USERNAME = @MANV)
        BEGIN
            INSERT INTO TAIKHOAN (USERNAME, ROLE_ID)
            VALUES (@MANV, 2);
        END

        FETCH NEXT FROM cur INTO @MANV;
    END
    CLOSE cur
    DEALLOCATE cur
END
GO

SELECT * FROM PHIEUMUON
SELECT * FROM CHITIETPM
SELECT * FROM PHIEUTRA
SELECT * FROM TAIKHOAN
INSERT INTO PHIEUMUON VALUES
('PM00004','TH00001','NV00001',GETDATE(),'2025/12/24',1)

INSERT INTO CHITIETPM VALUES
('PM00004','S000001',1,10000, 1),
('PM00004','S000004',2,20000, 1)