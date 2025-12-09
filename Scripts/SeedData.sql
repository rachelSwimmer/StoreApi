-- ============================================
-- Store API Database Seed Script
-- ============================================

USE StoreApiDb;
GO

-- ============================================
-- Insert Categories
-- ============================================
INSERT INTO Categories (Name, Description, CreatedAt, UpdatedAt)
VALUES 
    ('Electronics', 'Electronic devices and gadgets', GETUTCDATE(), GETUTCDATE()),
    ('Clothing', 'Fashion and apparel', GETUTCDATE(), GETUTCDATE()),
    ('Books', 'Books and magazines', GETUTCDATE(), GETUTCDATE()),
    ('Home & Garden', 'Home improvement and garden supplies', GETUTCDATE(), GETUTCDATE()),
    ('Sports', 'Sports equipment and accessories', GETUTCDATE(), GETUTCDATE());
GO

-- ============================================
-- Insert Products
-- Note: CategoryId references the auto-generated IDs from Categories (1-5)
-- ============================================
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt, UpdatedAt)
VALUES 
    ('Laptop HP ProBook', 'High-performance laptop with 16GB RAM and 512GB SSD', 899.99, 15, 1, GETUTCDATE(), GETUTCDATE()),
    ('Wireless Mouse', 'Ergonomic wireless mouse with USB receiver', 29.99, 50, 1, GETUTCDATE(), GETUTCDATE()),
    ('USB-C Hub', '7-in-1 USB-C hub with HDMI and card reader', 45.99, 30, 1, GETUTCDATE(), GETUTCDATE()),
    ('Bluetooth Headphones', 'Noise-cancelling wireless headphones', 149.99, 25, 1, GETUTCDATE(), GETUTCDATE()),
    ('Cotton T-Shirt', '100% cotton, available in multiple colors', 19.99, 100, 2, GETUTCDATE(), GETUTCDATE()),
    ('Denim Jeans', 'Classic fit denim jeans', 49.99, 60, 2, GETUTCDATE(), GETUTCDATE()),
    ('Winter Jacket', 'Warm and waterproof winter jacket', 129.99, 20, 2, GETUTCDATE(), GETUTCDATE()),
    ('The Great Gatsby', 'Classic American novel by F. Scott Fitzgerald', 14.99, 40, 3, GETUTCDATE(), GETUTCDATE()),
    ('Programming in C#', 'Comprehensive guide to C# programming', 59.99, 25, 3, GETUTCDATE(), GETUTCDATE()),
    ('Cookbook: Italian Cuisine', 'Authentic Italian recipes', 24.99, 35, 3, GETUTCDATE(), GETUTCDATE()),
    ('LED Desk Lamp', 'Adjustable LED desk lamp with USB port', 34.99, 45, 4, GETUTCDATE(), GETUTCDATE()),
    ('Garden Tool Set', '5-piece garden tool set with carrying case', 39.99, 20, 4, GETUTCDATE(), GETUTCDATE()),
    ('Yoga Mat', 'Non-slip exercise yoga mat', 29.99, 50, 5, GETUTCDATE(), GETUTCDATE()),
    ('Dumbbell Set', 'Adjustable dumbbell set 5-25kg', 89.99, 15, 5, GETUTCDATE(), GETUTCDATE()),
    ('Running Shoes', 'Professional running shoes with cushioned sole', 79.99, 40, 5, GETUTCDATE(), GETUTCDATE());
GO

-- ============================================
-- Insert Users
-- ============================================
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Phone, Address, CreatedAt, UpdatedAt)
VALUES 
    ('John', 'Doe', 'john.doe@example.com', 'UGFzc3dvcmQxMjM=', '+1-555-0101', '123 Main St, New York, NY 10001', GETUTCDATE(), GETUTCDATE()),
    ('Jane', 'Smith', 'jane.smith@example.com', 'UGFzc3dvcmQxMjM=', '+1-555-0102', '456 Oak Ave, Los Angeles, CA 90001', GETUTCDATE(), GETUTCDATE()),
    ('Mike', 'Johnson', 'mike.johnson@example.com', 'UGFzc3dvcmQxMjM=', '+1-555-0103', '789 Pine Rd, Chicago, IL 60601', GETUTCDATE(), GETUTCDATE()),
    ('Sarah', 'Williams', 'sarah.williams@example.com', 'UGFzc3dvcmQxMjM=', '+1-555-0104', '321 Elm St, Houston, TX 77001', GETUTCDATE(), GETUTCDATE()),
    ('David', 'Brown', 'david.brown@example.com', 'UGFzc3dvcmQxMjM=', '+1-555-0105', '654 Maple Dr, Phoenix, AZ 85001', GETUTCDATE(), GETUTCDATE());
GO

-- ============================================
-- Insert Orders
-- Note: UserId references the auto-generated IDs from Users (1-5)
-- ============================================
DECLARE @User1Id INT = (SELECT Id FROM Users WHERE Email = 'john.doe@example.com');
DECLARE @User2Id INT = (SELECT Id FROM Users WHERE Email = 'jane.smith@example.com');
DECLARE @User3Id INT = (SELECT Id FROM Users WHERE Email = 'mike.johnson@example.com');
DECLARE @User4Id INT = (SELECT Id FROM Users WHERE Email = 'sarah.williams@example.com');
DECLARE @User5Id INT = (SELECT Id FROM Users WHERE Email = 'david.brown@example.com');

INSERT INTO Orders (UserId, TotalAmount, Status, ShippingAddress, OrderDate, ShippedDate, DeliveredDate, CreatedAt, UpdatedAt)
VALUES 
    (@User1Id, 949.97, 'Delivered', '123 Main St, New York, NY 10001', DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -8, GETUTCDATE()), DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -5, GETUTCDATE())),
    (@User2Id, 224.97, 'Shipped', '456 Oak Ave, Los Angeles, CA 90001', DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()), NULL, DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE())),
    (@User3Id, 179.98, 'Processing', '789 Pine Rd, Chicago, IL 60601', DATEADD(DAY, -2, GETUTCDATE()), NULL, NULL, DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE())),
    (@User4Id, 169.98, 'Pending', '321 Elm St, Houston, TX 77001', DATEADD(DAY, -1, GETUTCDATE()), NULL, NULL, DATEADD(DAY, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE())),
    (@User5Id, 140.97, 'Pending', '654 Maple Dr, Phoenix, AZ 85001', GETUTCDATE(), NULL, NULL, GETUTCDATE(), GETUTCDATE());
GO

-- ============================================
-- Insert Order Items
-- Note: OrderId and ProductId reference auto-generated IDs
-- ============================================
DECLARE @Order1Id INT = (SELECT TOP 1 Id FROM Orders WHERE UserId = (SELECT Id FROM Users WHERE Email = 'john.doe@example.com'));
DECLARE @Order2Id INT = (SELECT TOP 1 Id FROM Orders WHERE UserId = (SELECT Id FROM Users WHERE Email = 'jane.smith@example.com'));
DECLARE @Order3Id INT = (SELECT TOP 1 Id FROM Orders WHERE UserId = (SELECT Id FROM Users WHERE Email = 'mike.johnson@example.com'));
DECLARE @Order4Id INT = (SELECT TOP 1 Id FROM Orders WHERE UserId = (SELECT Id FROM Users WHERE Email = 'sarah.williams@example.com'));
DECLARE @Order5Id INT = (SELECT TOP 1 Id FROM Orders WHERE UserId = (SELECT Id FROM Users WHERE Email = 'david.brown@example.com'));

DECLARE @Product1Id INT = (SELECT Id FROM Products WHERE Name = 'Laptop HP ProBook');
DECLARE @Product2Id INT = (SELECT Id FROM Products WHERE Name = 'Wireless Mouse');
DECLARE @Product3Id INT = (SELECT Id FROM Products WHERE Name = 'USB-C Hub');
DECLARE @Product4Id INT = (SELECT Id FROM Products WHERE Name = 'Bluetooth Headphones');
DECLARE @Product5Id INT = (SELECT Id FROM Products WHERE Name = 'Cotton T-Shirt');
DECLARE @Product6Id INT = (SELECT Id FROM Products WHERE Name = 'Denim Jeans');
DECLARE @Product7Id INT = (SELECT Id FROM Products WHERE Name = 'Winter Jacket');
DECLARE @Product8Id INT = (SELECT Id FROM Products WHERE Name = 'The Great Gatsby');
DECLARE @Product9Id INT = (SELECT Id FROM Products WHERE Name = 'Programming in C#');
DECLARE @Product11Id INT = (SELECT Id FROM Products WHERE Name = 'LED Desk Lamp');
DECLARE @Product13Id INT = (SELECT Id FROM Products WHERE Name = 'Yoga Mat');
DECLARE @Product14Id INT = (SELECT Id FROM Products WHERE Name = 'Dumbbell Set');
DECLARE @Product15Id INT = (SELECT Id FROM Products WHERE Name = 'Running Shoes');

-- Order 1 Items (John Doe - Delivered)
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Subtotal)
VALUES 
    (@Order1Id, @Product1Id, 1, 899.99, 899.99),
    (@Order1Id, @Product2Id, 1, 29.99, 29.99),
    (@Order1Id, @Product5Id, 1, 19.99, 19.99);

-- Order 2 Items (Jane Smith - Shipped)
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Subtotal)
VALUES 
    (@Order2Id, @Product4Id, 1, 149.99, 149.99),
    (@Order2Id, @Product8Id, 1, 14.99, 14.99),
    (@Order2Id, @Product13Id, 2, 29.99, 59.98);

-- Order 3 Items (Mike Johnson - Processing)
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Subtotal)
VALUES 
    (@Order3Id, @Product7Id, 1, 129.99, 129.99),
    (@Order3Id, @Product6Id, 1, 49.99, 49.99);

-- Order 4 Items (Sarah Williams - Pending)
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Subtotal)
VALUES 
    (@Order4Id, @Product14Id, 1, 89.99, 89.99),
    (@Order4Id, @Product15Id, 1, 79.99, 79.99);

-- Order 5 Items (David Brown - Pending)
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Subtotal)
VALUES 
    (@Order5Id, @Product9Id, 1, 59.99, 59.99),
    (@Order5Id, @Product11Id, 1, 34.99, 34.99),
    (@Order5Id, @Product3Id, 1, 45.99, 45.99);
GO

-- ============================================
-- Verification
-- ============================================
PRINT 'Categories Count: ' + CAST((SELECT COUNT(*) FROM Categories) AS VARCHAR);
PRINT 'Products Count: ' + CAST((SELECT COUNT(*) FROM Products) AS VARCHAR);
PRINT 'Users Count: ' + CAST((SELECT COUNT(*) FROM Users) AS VARCHAR);
PRINT 'Orders Count: ' + CAST((SELECT COUNT(*) FROM Orders) AS VARCHAR);
PRINT 'Order Items Count: ' + CAST((SELECT COUNT(*) FROM OrderItems) AS VARCHAR);
PRINT 'Database seeded successfully!';
GO
