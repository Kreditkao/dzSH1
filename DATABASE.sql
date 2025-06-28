-- 1. Создание базы данных
CREATE DATABASE LibraryDB;
GO

USE LibraryDB;
GO

-- 2. Таблица пользователей
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    FullName NVARCHAR(200) NOT NULL
);

-- 3. Таблица книг
CREATE TABLE Books (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(200) NOT NULL,
    IsAvailable BIT NOT NULL,
    DueDate DATETIME NULL,
    BorrowedByUserId INT NULL,
    FOREIGN KEY (BorrowedByUserId) REFERENCES Users(Id)
);

-- 4. Таблица транзакций (журнал выдачи/возврата)
CREATE TABLE Transactions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    ActionType NVARCHAR(20) NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (BookId) REFERENCES Books(Id)
);

-- 5. Заполнение таблицы книг
INSERT INTO Books (Title, Author, IsAvailable, DueDate, BorrowedByUserId)
VALUES 
('Clean Code', 'Robert C. Martin', 1, NULL, NULL),
('The Pragmatic Programmer', 'Andrew Hunt, David Thomas', 1, NULL, NULL),
('Introduction to Algorithms', 'Cormen et al.', 1, NULL, NULL),
('Design Patterns', 'Erich Gamma et al.', 1, NULL, NULL),
('Database Systems', 'Thomas Connolly', 1, NULL, NULL),
('C# in Depth', 'Jon Skeet', 1, NULL, NULL),
('Eloquent JavaScript', 'Marijn Haverbeke', 1, NULL, NULL);
