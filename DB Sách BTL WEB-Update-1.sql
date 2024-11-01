create database bookvip
use bookvip

-- Bảng Users để quản lý người dùng (bao gồm cả khách hàng và admin)
CREATE TABLE Users (
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    PhoneNumber CHAR(10),
    Address NVARCHAR(255),
    Password NVARCHAR(255),  -- Mã hóa mật khẩu
    Role INT,  -- 0: Khách hàng, 1: Admin
    CONSTRAINT pk_user PRIMARY KEY (PhoneNumber)
);

-- Bảng Categories (Danh mục)
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100)
);

-- Bảng Books (Sách)
CREATE TABLE Books (
    BookID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255),
    Author NVARCHAR(100),
    Publisher NVARCHAR(100),
    PublishedDate DATE,
    CategoryID INT,
    Price DECIMAL(10, 2),
    StockQuantity INT,
    Description NVARCHAR(1000),
    ImageURL NVARCHAR(255),
    CONSTRAINT fk_categories_books FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- Bảng Cart (Giỏ hàng)
CREATE TABLE Carts (
    CartID INT PRIMARY KEY IDENTITY(1,1),
    PhoneNumber CHAR(10),
    CONSTRAINT fk_users_cart FOREIGN KEY (PhoneNumber) REFERENCES Users(PhoneNumber)
);

-- Bảng CartItems (Chi tiết giỏ hàng)
CREATE TABLE CartItems (
    CartItemID INT PRIMARY KEY IDENTITY(1,1),
    CartID INT,
    BookID INT,
    Quantity INT,
    CONSTRAINT fk_cart_cartitems FOREIGN KEY (CartID) REFERENCES Cart(CartID),
    CONSTRAINT fk_books_cartitems FOREIGN KEY (BookID) REFERENCES Books(BookID)
);

-- Bảng Orders (Đơn hàng)
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    PhoneNumber CHAR(10),
    OrderDate DATE,
    TotalAmount DECIMAL(10, 2),
    Status NVARCHAR(50),
	ShippingAddress NVARCHAR(255),
    CONSTRAINT fk_users_orders FOREIGN KEY (PhoneNumber) REFERENCES Users(PhoneNumber)
);



-- Bảng OrderDetails (Chi tiết đơn hàng)
CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT,
    BookID INT,
    Quantity INT,
    Price DECIMAL(10, 2),
    CONSTRAINT fk_orders_orderdetails FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    CONSTRAINT fk_books_orderdetails FOREIGN KEY (BookID) REFERENCES Books(BookID)
);

-- Bảng Payments (Thanh toán)
CREATE TABLE Payments (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT,
    PaymentDate DATE,
    PaymentMethod NVARCHAR(50),
    PaymentStatus NVARCHAR(50),
    CONSTRAINT fk_orders_payments FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);

-- Bảng Reviews (Đánh giá)
CREATE TABLE Reviews (
    ReviewID INT PRIMARY KEY IDENTITY(1,1),
    BookID INT,
    PhoneNumber CHAR(10),
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(1000),
    ReviewDate DATE,
    CONSTRAINT fk_books_reviews FOREIGN KEY (BookID) REFERENCES Books(BookID),
    CONSTRAINT fk_users_reviews FOREIGN KEY (PhoneNumber) REFERENCES Users(PhoneNumber)
);

INSERT INTO Categories (CategoryName) 
VALUES
('Fiction'),
('Science'),
('History'),
('Math'),
('Biography'),
('Fantasy'),
('Mystery'),
('Romance'),
('Self-Help'),
('Cookbooks');

-- Bảng Revenue (Doanh thu)
CREATE TABLE Revenues (
    RevenueID INT PRIMARY KEY IDENTITY(1,1),  -- Khóa chính
    Date DATE NOT NULL,                        -- Ngày ghi nhận
    TotalSales DECIMAL(10, 2) NOT NULL,      -- Tổng doanh thu
    TotalOrders INT NOT NULL,                  -- Tổng số đơn hàng trong ngày
    TotalProfit DECIMAL(10, 2) NOT NULL       -- Tổng lợi nhuận
);

INSERT INTO Revenues (Date, TotalSales, TotalOrders, TotalProfit)
VALUES
('2023-01-01', 1500.00, 30, 300000.00),
('2023-01-02', 2000.00, 50, 400000.00),
('2023-01-03', 1800.00, 40, 300050.00),
('2023-02-01', 2200.00, 60, 500000.00),
('2023-02-02', 2100.00, 55, 450000.00),
('2023-03-01', 2500.00, 70, 600000.00),
('2023-03-02', 2600.00, 75, 600050.00),
('2023-04-01', 3000.00, 90, 800000.00),
('2023-04-02', 2800.00, 85, 750000.00),
('2023-05-01', 3200.00, 100, 900000.00),
('2023-05-02', 3100.00, 95, 850000.00);

exec sp_adduser 'User','User'
grant all on database::[bookvip] to [User]

INSERT INTO Books (Title, Author, Publisher, PublishedDate, CategoryID, Price, StockQuantity, Description, ImageURL) VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', 'Scribner', '1925-04-10', 1, 10.99, 100, 'A story of the Jazz Age.', 'Bailu_1244056766.png'),
('To Kill a Mockingbird', 'Harper Lee', 'J.B. Lippincott', '1960-07-11', 1, 7.99, 80, 'A novel about racial injustice.', 'Bailu_1244056766.png'),
('1984', 'George Orwell', 'Secker & Warburg', '1949-06-08', 1, 9.99, 120, 'Dystopian future narrative.', 'Bailu_1244056766.png'),
('Pride and Prejudice', 'Jane Austen', 'T. Egerton', '1813-01-28', 1, 8.99, 90, 'A romantic novel.', 'Bailu_1244056766.png'),
('The Catcher in the Rye', 'J.D. Salinger', 'Little, Brown and Company', '1951-07-16', 1, 10.50, 70, 'A reflection on teenage angst.', 'Bailu_1244056766.png'),
('The Lord of the Rings', 'J.R.R. Tolkien', 'George Allen & Unwin', '1954-07-29', 1, 12.99, 60, 'Epic fantasy adventure.', 'Bailu_1244056766.png'),
('Animal Farm', 'George Orwell', 'Secker & Warburg', '1945-08-17', 1, 6.99, 150, 'Allegorical novella.', 'Bailu_1244056766.png'),
('The Picture of Dorian Gray', 'Oscar Wilde', 'Lippincott', '1890-07-01', 1, 8.50, 75, 'A tale of vanity and morality.', 'Bailu_1244056766.png'),
('Brave New World', 'Aldous Huxley', 'Chatto & Windus', '1932-08-31', 1, 10.00, 55, 'Dystopian society critique.', 'Bailu_1244056766.png'),
('The Alchemist', 'Paulo Coelho', 'HarperCollins', '1988-05-01', 1, 11.00, 80, 'A journey of self-discovery.', 'Bailu_1244056766.png');

INSERT INTO Books (Title, Author, Publisher, PublishedDate, CategoryID, Price, StockQuantity, Description, ImageURL) VALUES
('The Girl with the Dragon Tattoo', 'Stieg Larsson', 'Norstedts Förlag', '2005-08-01', 2, 9.99, 100, 'A gripping modern mystery.', 'Bailu_1244056766.png'),
('Gone Girl', 'Gillian Flynn', 'Crown Publishing Group', '2012-06-05', 2, 12.99, 80, 'A psychological thriller.', 'Bailu_1244056766.png'),
('The Da Vinci Code', 'Dan Brown', 'Doubleday', '2003-03-18', 2, 10.50, 90, 'A mystery involving art and religion.', 'Bailu_1244056766.png'),
('Big Little Lies', 'Liane Moriarty', 'Amy Einhorn Books', '2014-07-29', 2, 11.00, 70, 'A story of murder and secrets.', 'Bailu_1244056766.png'),
('The Woman in the Window', 'A.J. Finn', 'William Morrow', '2018-01-02', 2, 14.99, 60, 'A psychological thriller.', 'Bailu_1244056766.png'),
('In the Woods', 'Tana French', 'HarperCollins', '2007-05-17', 2, 9.00, 85, 'A haunting psychological mystery.', 'Bailu_1244056766.png'),
('The Cuckoos Calling', 'Robert Galbraith', 'Mulholland Books', '2013-04-30', 2, 10.00, 75, 'A detective novel set in London.', 'Bailu_1244056766.png'),
('The Silent Patient', 'Alex Michaelides', 'Celadon Books', '2019-02-05', 2, 12.50, 65, 'A psychological thriller.', 'Bailu_1244056766.png'),
('Sharp Objects', 'Gillian Flynn', 'Shaye Areheart Books', '2006-09-26', 2, 10.99, 80, 'A psychological thriller.', 'Bailu_1244056766.png'),
('The No. 1 Ladies Detective Agency', 'Alexander McCall Smith', 'Polygon', '2000-05-01', 2, 8.99, 90, 'A charming detective story.', 'Bailu_1244056766.png');

INSERT INTO Books (Title, Author, Publisher, PublishedDate, CategoryID, Price, StockQuantity, Description, ImageURL) VALUES
('Harry Potter and the Sorcerers Stone', 'J.K. Rowling', 'Bloomsbury', '1997-06-26', 3, 9.99, 200, 'A young wizards journey begins.', 'Bailu_1244056766.png'),
('A Game of Thrones', 'George R.R. Martin', 'Bantam', '1996-08-06', 3, 12.99, 150, 'Epic fantasy series starter.', 'Bailu_1244056766.png'),
('The Hobbit', 'J.R.R. Tolkien', 'George Allen & Unwin', '1937-09-21', 3, 10.50, 160, 'A tale of adventure and discovery.', 'Bailu_1244056766.png'),
('Mistborn: The Final Empire', 'Brandon Sanderson', 'Tor Books', '2006-07-17', 3, 11.00, 120, 'A unique magic system and heist.', 'Bailu_1244056766.png'),
('The Name of the Wind', 'Patrick Rothfuss', 'DAW Books', '2007-03-27', 3, 10.99, 110, 'The story of a gifted young man.', 'Bailu_1244056766.png'),
('The Way of Kings', 'Brandon Sanderson', 'Tor Books', '2010-08-31', 3, 14.99, 80, 'Epic fantasy with deep world-building.', 'Bailu_1244056766.png'),
('American Gods', 'Neil Gaiman', 'William Morrow', '2001-06-19', 3, 12.50, 90, 'A blend of fantasy and Americana.', 'Bailu_1244056766.png'),
('The Night Circus', 'Erin Morgenstern', 'Doubleday', '2011-09-13',3 , 11.00, 75, 'A magical competition between two young illusionists.', 'Bailu_1244056766.png'),
('The Lies of Locke Lamora', 'Scott Lynch', 'Bantam', '2006-06-27', 3, 10.00, 85, 'A thiefs adventure in a fantasy city.', 'Bailu_1244056766.png'),
('The Priory of the Orange Tree', 'Samantha Shannon', 'Bloomsbury', '2019-02-26', 3, 15.99, 70, 'Epic fantasy with dragons and politics.', 'Bailu_1244056766.png');

INSERT INTO Books (Title, Author, Publisher, PublishedDate, CategoryID, Price, StockQuantity, Description, ImageURL) VALUES
('Dune', 'Frank Herbert', 'Chilton Books', '1965-08-01', 4, 12.99, 90, 'A science fiction epic set on desert planet Arrakis.', 'Bailu_1244056766.png'),
('Neuromancer', 'William Gibson', 'Ace Books', '1984-07-01', 4, 10.99, 80, 'A novel that defined cyberpunk.', 'Bailu_1244056766.png'),
('Foundation', 'Isaac Asimov', 'Gnome Press', '1951-06-01', 4, 9.99, 100, 'The first book in the Foundation series.', 'Bailu_1244056766.png'),
('The Left Hand of Darkness', 'Ursula K. Le Guin', 'Ace Books', '1969-03-01', 4, 11.00, 75, 'Explores gender and society on a distant planet.', 'Bailu_1244056766.png'),
('Snow Crash', 'Neal Stephenson', 'Bantam Spectra', '1992-06-01', 4, 10.50, 85, 'A fast-paced cyberpunk novel.', 'Bailu_1244056766.png'),
('Enders Game', 'Orson Scott Card', 'Tor Books', '1985-01-15', 4, 12.50, 95, 'A story about a young boy training for battle.', 'Bailu_1244056766.png'),
('The Hitchhikers Guide to the Galaxy', 'Douglas Adams', 'Pan Books', '1979-10-12', 4, 8.99, 110, 'A comedic science fiction adventure.', 'Bailu_1244056766.png'),
('The Martian', 'Andy Weir', 'Crown Publishing Group', '2014-02-11', 4, 14.99, 70, 'An astronaut stranded on Mars must survive.', 'Bailu_1244056766.png'),
('Fahrenheit 451', 'Ray Bradbury', 'Ballantine Books', '1953-10-19', 4, 9.00, 120, 'A dystopian novel about censorship.', 'Bailu_1244056766.png'),
('Hyperion', 'Dan Simmons', 'Doubleday', '1989-05-01', 4, 10.00, 85, 'A complex narrative set in a far-future universe.', 'Bailu_1244056766.png');

INSERT INTO Books (Title, Author, Publisher, PublishedDate, CategoryID, Price, StockQuantity, Description, ImageURL) VALUES
('Pride and Prejudice', 'Jane Austen', 'T. Egerton', '1813-01-28', 5, 8.99, 90, 'A classic romantic novel.', 'Bailu_1244056766.png'),
('Outlander', 'Diana Gabaldon', 'Dell Publishing', '1991-06-1', 5, 11.99, 75, 'Time travel romance set in Scotland.', 'Bailu_1244056766.png'),
('The Notebook', 'Nicholas Sparks', 'Warner Books', '1996-10-01', 5, 12.50, 80, 'A love story that spans decades.', 'Bailu_1244056766.png'),
('Me Before You', 'Jojo Moyes', 'Pamela Dorman Books', '2012-08-21', 5, 10.99, 70, 'A touching love story.', 'Bailu_1244056766.png'),
('The Hating Game', 'Sally Thorne', 'William Morrow', '2016-08-09', 5, 9.99, 85, 'Enemies to lovers romantic comedy.', 'Bailu_1244056766.png'),
('Red, White & Royal Blue', 'Casey McQuiston', 'St. Martins Griffin', '2019-05-14', 5, 12.00, 65, 'A romantic comedy involving a prince.', 'Bailu_1244056766.png'),
('Beach Read', 'Emily Henry', 'Berkley', '2020-05-19', 5, 11.99, 75, 'Two writers with different styles find love.', 'Bailu_1244056766.png'),
('It Ends with Us', 'Colleen Hoover', 'Atria Books', '2016-08-02', 5, 13.00, 80, 'A powerful love story.', 'Bailu_1244056766.png'),
('The Kiss Quotient', 'Helen Hoang', 'Berkley', '2018-05-29', 5, 10.99, 90, 'A unique love story with a twist.', 'Bailu_1244056766.png'),
('You Had Me at Hola', 'Alexis Daria', 'Avon', '2020-08-04', 5, 12.00, 70, 'A romantic comedy with a cultural backdrop.', 'Bailu_1244056766.png');

INSERT INTO Books (Title, Author, Publisher, PublishedDate, CategoryID, Price, StockQuantity, Description, ImageURL) VALUES
('The Book Thief', 'Markus Zusak', 'Picador', '2005-03-01', 6, 12.99, 80, 'A story set in Nazi Germany.', 'Bailu_1244056766.png'),
('All the Light We Cannot See', 'Anthony Doerr', 'Scribner', '2014-05-06', 6, 13.50, 75, 'A tale of a blind French girl during WWII.', 'Bailu_1244056766.png'),
('The Nightingale', 'Kristin Hannah', 'St. Martins Press', '2015-02-03', 6, 14.99, 90, 'Women’s stories during the war.', 'Bailu_1244056766.png'),
('The Pillars of the Earth', 'Ken Follett', 'Penguin Books', '1989-05-01', 6, 10.99, 100, 'A historical epic about building a cathedral.', 'Bailu_1244056766.png'),
('The Help', 'Kathryn Stockett', 'Putnam', '2009-02-10', 6, 9.99, 85, 'Stories of African American maids in the 1960s.', 'Bailu_1244056766.png'),
('The Alice Network', 'Kate Quinn', 'Berkley', '2017-06-06', 6, 11.50, 70, 'A female spy during WWI.', 'Bailu_1244056766.png'),
('Water for Elephants', 'Sara Gruen', 'Algonquin Books', '2006-05-26', 6, 12.00, 75, 'A love story set in a traveling circus.', 'Bailu_1244056766.png'),
('The Tea Girl of Hummingbird Lane', 'Lisa See', 'Scribner', '2017-03-21', 6, 13.00, 80, 'Cultural exploration in China.', 'Bailu_1244056766.png'),
('The Tattooist of Auschwitz', 'Heather Morris', 'Zaffre', '2018-01-11', 6, 11.99, 90, 'A poignant love story set in a concentration camp.', 'Bailu_1244056766.png'),
('Where the Crawdads Sing', 'Delia Owens', 'G.P. Putnams Sons', '2018-08-14', 6, 14.00, 75, 'A mystery set in the marshes of North Carolina.', 'Bailu_1244056766.png');

INSERT INTO Books (Title, Author, Publisher, PublishedDate, CategoryID, Price, StockQuantity, Description, ImageURL) VALUES
('The Girl on the Train', 'Paula Hawkins', 'Riverhead Books', '2015-01-13', 8, 12.99, 90, 'A psychological thriller.', 'Bailu_1244056766.png'),
('The Silent Patient', 'Alex Michaelides', 'Celadon Books', '2019-02-05', 8, 14.99, 80, 'A gripping psychological thriller.', 'Bailu_1244056766.png'),
('Big Little Lies', 'Liane Moriarty', 'Amy Einhorn Books', '2014-07-29', 8, 11.00, 75, 'A story of murder and secrets.', 'Bailu_1244056766.png'),
('The Couple Next Door', 'Shari Lapena', 'Pamela Dorman Books', '2016-08-23', 8, 9.99, 85, 'A thrilling tale of deception.', 'Bailu_1244056766.png'),
('Gone Girl', 'Gillian Flynn', 'Crown Publishing Group', '2012-06-05', 8, 12.99, 80, 'A psychological thriller about a missing wife.', 'Bailu_1244056766.png'),
('The Wife Between Us', 'Greer Hendricks & Sarah Pekkanen', 'St. Martins Press', '2018-01-09', 8, 10.99, 90, 'A twisted psychological thriller.', 'Bailu_1244056766.png'),
('Behind Closed Doors', 'B.A. Paris', 'HQ', '2016-02-11', 8, 11.50, 70, 'A chilling domestic thriller.', 'Bailu_1244056766.png'),
('The Woman in the Window', 'A.J. Finn', 'William Morrow', '2018-01-02', 8, 13.00, 75, 'A psychological thriller with twists.', 'Bailu_1244056766.png'),
('The Chain', 'Adrian McKinty', 'Orion', '2019-07-25', 8, 10.00, 85, 'A thriller about a kidnapping chain.', 'Bailu_1244056766.png'),
('The Guest List', 'Lucy Foley', 'HarperCollins', '2020-02-20', 8, 14.00, 80, 'A wedding day thriller.', 'Bailu_1244056766.png');