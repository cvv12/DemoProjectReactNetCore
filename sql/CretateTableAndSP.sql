USE [ABCBank]
GO
/****** Object:  Table [dbo].[Account]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Account](
	[AccountId] [int] IDENTITY(1,1) NOT NULL,
	[AccountNumber] [nvarchar](20) NULL,
	[AccountType] [nvarchar](50) NULL,
	[EncryptedBalance] [nvarchar](256) NULL,
	[IsActive] [nvarchar](1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](30) NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](30) NULL,
PRIMARY KEY CLUSTERED 
(
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccountTypeCodes]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountTypeCodes](
	[AccountType] [nvarchar](50) NOT NULL,
	[Code] [nvarchar](2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AccountType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customer]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[CustomerId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerName] [nvarchar](100) NULL,
	[NRIC] [nvarchar](20) NULL,
	[IsActive] [nvarchar](1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](30) NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](30) NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerAccount]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerAccount](
	[CustomerId] [int] NOT NULL,
	[AccountId] [int] NOT NULL,
 CONSTRAINT [PK_CustomerAccount] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC,
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExceptionLog]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExceptionLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExceptionMessage] [nvarchar](max) NULL,
	[StackTrace] [nvarchar](max) NULL,
	[OccurredOn] [datetime] NULL,
	[Source] [nvarchar](200) NULL,
	[ExceptionType] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[AccountTypeCodes] ([AccountType], [Code]) VALUES (N'CHECKING', N'02')
INSERT [dbo].[AccountTypeCodes] ([AccountType], [Code]) VALUES (N'CREDIT', N'03')
INSERT [dbo].[AccountTypeCodes] ([AccountType], [Code]) VALUES (N'SAVINGS', N'01')
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Customer__E2F96E8757E6B7D4]    Script Date: 11/19/2023 11:59:39 PM ******/
ALTER TABLE [dbo].[Customer] ADD UNIQUE NONCLUSTERED 
(
	[NRIC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Account] ADD  DEFAULT ('Y') FOR [IsActive]
GO
ALTER TABLE [dbo].[Account] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Account] ADD  DEFAULT ('system') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[Customer] ADD  DEFAULT ('Y') FOR [IsActive]
GO
ALTER TABLE [dbo].[Customer] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Customer] ADD  DEFAULT ('system') FOR [CreatedBy]
GO
ALTER TABLE [dbo].[ExceptionLog] ADD  DEFAULT (getdate()) FOR [OccurredOn]
GO
ALTER TABLE [dbo].[CustomerAccount]  WITH CHECK ADD  CONSTRAINT [FK_CustomerAccount_Account] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Account] ([AccountId])
GO
ALTER TABLE [dbo].[CustomerAccount] CHECK CONSTRAINT [FK_CustomerAccount_Account]
GO
ALTER TABLE [dbo].[CustomerAccount]  WITH CHECK ADD  CONSTRAINT [FK_CustomerAccount_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([CustomerId])
GO
ALTER TABLE [dbo].[CustomerAccount] CHECK CONSTRAINT [FK_CustomerAccount_Customer]
GO
/****** Object:  StoredProcedure [dbo].[AddAccount]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddAccount]
    @CustomerId INT,
    @AccountNumber NVARCHAR(20),
    @AccountType NVARCHAR(50),
    @EncryptedBalance NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON; 
    DECLARE @NewAccountId INT;

    BEGIN TRANSACTION;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Customer WHERE CustomerId = @CustomerId)
        BEGIN
            RAISERROR ('Customer does not exist.', 16, 1);
            RETURN;
        END

        INSERT INTO Account (AccountNumber, AccountType, EncryptedBalance, CreatedDate, CreatedBy)
        VALUES (@AccountNumber, @AccountType, @EncryptedBalance, GETDATE(), 'SYSTEM');
        
        SET @NewAccountId = SCOPE_IDENTITY();
        INSERT INTO CustomerAccount (CustomerId, AccountId)
        VALUES (@CustomerId, @NewAccountId);

        COMMIT TRANSACTION;

        SELECT @NewAccountId AS NewAccountId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR ('Error occurred while adding account and linking customer: %s', 16, 1, @ErrMsg);
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[AddCustomer]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddCustomer]
    @CustomerName NVARCHAR(100),
    @NRIC NVARCHAR(20)
AS
BEGIN
    INSERT INTO Customer (CustomerName, NRIC,CreatedDate, CreatedBy)
    VALUES (@CustomerName, @NRIC,GETDATE(),'SYSTEM');
    
    SELECT SCOPE_IDENTITY() AS NewCustomerId;
END

GO
/****** Object:  StoredProcedure [dbo].[CheckCustomerAccountTypeExists]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CheckCustomerAccountTypeExists]
    @CustomerId INT,
    @AccountType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM CustomerAccount CA INNER JOIN Account A ON CA.AccountId = A.AccountId
			   WHERE CA.CustomerId = @CustomerId AND A.AccountType = @AccountType AND A.IsActive = 'Y')
    BEGIN
        SELECT CAST(1 AS BIT) 
    END
    ELSE
    BEGIN
        SELECT CAST(0 AS BIT) 
    END
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteAccount]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteAccount]
    @AccountId INT
AS
BEGIN
    UPDATE Account
	SET IsActive = 'N'
    WHERE AccountId = @AccountId
END
GO
/****** Object:  StoredProcedure [dbo].[GetAccountById]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAccountById]
    @AccountId INT
AS
BEGIN
    SELECT AccountId, AccountNumber, AccountType, EncryptedBalance
    FROM Account
    WHERE AccountId = @AccountId
END
GO
/****** Object:  StoredProcedure [dbo].[GetAccounts]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAccounts]
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    DECLARE @TotalCount INT;

    SELECT @TotalCount = COUNT(*)
    FROM [dbo].[Account] a
    INNER JOIN [dbo].[CustomerAccount] ca ON a.AccountId = ca.AccountId
    INNER JOIN [dbo].[Customer] c ON ca.CustomerId = c.CustomerId
    WHERE a.IsActive = 'Y' AND c.IsActive = 'Y';

    SELECT 
        a.AccountId, 
        a.AccountNumber, 
        a.AccountType, 
        a.EncryptedBalance, 
        c.CustomerName, 
        c.NRIC,
        TotalCount = @TotalCount 
    FROM [dbo].[Account] a
    INNER JOIN [dbo].[CustomerAccount] ca ON a.AccountId = ca.AccountId
    INNER JOIN [dbo].[Customer] c ON ca.CustomerId = c.CustomerId
    WHERE a.IsActive = 'Y' AND c.IsActive = 'Y'
    ORDER BY a.AccountId
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
/****** Object:  StoredProcedure [dbo].[GetCustomers]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomers]
    @PageNumber INT = 1,
    @PageSize INT = 100,
    @SearchTerm NVARCHAR(255) = NULL
AS
BEGIN
    DECLARE @SkipRows INT = (@PageNumber - 1) * @PageSize;

    WITH FilteredCustomers AS (
        SELECT 
            CustomerId, 
            CustomerName, 
            NRIC,
            ROW_NUMBER() OVER (ORDER BY CustomerName) AS RowNum
        FROM 
            Customer
        WHERE
            (@SearchTerm IS NULL OR CustomerName LIKE '%' + @SearchTerm + '%')
            OR
            (@SearchTerm IS NULL OR NRIC LIKE '%' + @SearchTerm + '%')
    )
    SELECT 
        CustomerId, 
        CustomerName, 
        NRIC
    FROM 
        FilteredCustomers
    WHERE 
        RowNum > @SkipRows AND RowNum <= (@SkipRows + @PageSize);
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateAccountBalance]    Script Date: 11/19/2023 11:59:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateAccountBalance]
    @AccountId INT,
    @NewEncryptedBalance NVARCHAR(256)
AS
BEGIN
    UPDATE Account
    SET EncryptedBalance = @NewEncryptedBalance
    WHERE AccountId = @AccountId
END
GO
