using Microsoft.Data.Sqlite;
using ReadCity.Models;
using System.Data;
using System.Globalization;
using System.Net.NetworkInformation;
namespace ReadCity.Data
{
    public class Database
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ReadCity.db");
        public static string ConnectionString => $"Data Source = {DbPath}";
        
        public static void InitializeDatabase()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            var sql = @"CREATE TABLE IF NOT EXISTS Users(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Login TEXT NOT NULL UNIQUE,
            Password TEXT NOT NULL,
            FullName TEXT NOT NULL,
            Role TEXT NOT NULL CHECK(Role IN('Гость','Клиент','Менеджер','Администратор'))
            );
            
            CREATE TABLE IF NOT EXISTS Categories(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Manufacturers(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Suppliers(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS PickupPoints(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Products(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Article TEXT NOT NULL UNIQUE,
            BookName TEXT NOT NULL,
            Unit TEXT NOT NULL,
            Price DECIMAL(10,2) NOT NULL DEFAULT 0,
            SupplierId INTEGER NOT NULL REFERENCES Suppliers(Id),            
            ManufacturerId INTEGER NOT NULL REFERENCES Manufacturers(Id),           
            CategoryId INTEGER NOT NULL REFERENCES Categories(Id),            
            Discount DECIMAL(5,2) DEFAULT 0,
            StockQuantity INTEGER NOT NULL DEFAULT 0,
            Description TEXT,
            ImagePath TEXT
            );

            CREATE TABLE IF NOT EXISTS Orders(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            OrderDate TEXT NOT NULL,
            DeliveryDate TEXT NOT NULL,
            PickupPointId INTEGER NOT NULL REFERENCES PickupPoints(Id),             
            UserId INTEGER NOT NULL REFERENCES Users(Id),
            Code INTEGER NOT NULL,
            Status TEXT NOT NULL            
            );

            CREATE TABLE IF NOT EXISTS OrderItems(
            ProductId INTEGER NOT NULL REFERENCES Products(Id),
            OrderId INTEGER NOT NULL REFERENCES Orders(Id),           
            Quantity INTEGER NOT NULL,
            PRIMARY KEY(OrderId,ProductId)
            );";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            try { ImportUsers(); } 
            catch (Exception ex) { Console.WriteLine($"Ошибка импорта Users: {ex.Message}"); }
            try { ImportCategories(); } 
            catch (Exception ex) { Console.WriteLine($"Ошибка импорта Categories: {ex.Message}"); }
            try { ImportManufacturers(); } 
            catch (Exception ex) { Console.WriteLine($"Ошибка импорта Manufacturers: {ex.Message}"); }
            try { ImportSuppliers(); } 
            catch (Exception ex) { Console.WriteLine($"Ошибка импорта Suppliers: {ex.Message}"); }
            try { ImportProducts(); } 
            catch (Exception ex) { Console.WriteLine($"Ошибка импорта Products: {ex.Message}"); }
            try { ImportPickupPoints(); } 
            catch (Exception ex) { Console.WriteLine($"Ошибка импорта PickupPoints: {ex.Message}"); }
            try { ImportOrders(); } 
            catch (Exception ex) { Console.WriteLine($"Ошибка импорта Orders: {ex.Message}"); }
            try { ImportOrderItems(); } 
            catch (Exception ex) { Console.WriteLine($"Ошибка импорта OrderItems: {ex.Message}"); }
        }

        private static SqliteConnection GetConn()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;",conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        private static void ImportUsers()
        {
            var csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imports", "csv", "Users.csv");
            using var conn = GetConn();
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Users", conn);            

            if ((long)checkCmd.ExecuteScalar() == 0)
            {
                string[] lines = File.ReadAllLines(csvFilePath);
                for (int i = 1; i < lines.Length; i++)
                {                    
                    string currentLine = lines[i];
                    string[] parts = currentLine.Split(';');

                    if (parts.Length<4 || 
                        string.IsNullOrWhiteSpace(parts[0]) ||
                        string.IsNullOrWhiteSpace(parts[1]) ||
                        string.IsNullOrWhiteSpace(parts[2]) ||
                        string.IsNullOrWhiteSpace(parts[3])
                        ) continue;
                    
                    var user = new User
                    {
                        Role = parts[0].Trim(),
                        FullName = parts[1].Trim(),
                        Login = parts[2].Trim(),
                        Password = parts[3].Trim()
                    };
                    using var insertCmd = new SqliteCommand("INSERT INTO Users(Login,Password,FullName,Role) VALUES (@l,@p,@f,@r);", conn);
                    insertCmd.Parameters.AddWithValue("@r", user.Role);
                    insertCmd.Parameters.AddWithValue("@f", user.FullName);
                    insertCmd.Parameters.AddWithValue("@l", user.Login);
                    insertCmd.Parameters.AddWithValue("@p", user.Password);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
        
        private static void ImportCategories()
        {
            var list = new List<string>();
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"imports","csv","Tovar.csv");
            using var conn = GetConn();
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Categories",conn);
            if((long)checkCmd.ExecuteScalar()==0)
            {
                string[] lines = File.ReadAllLines(filePath);
                for(var i=1;i<lines.Length;i++)
                {
                    string currentLines = lines[i];
                    string[] parts = currentLines.Split(';');

                    if (parts.Length < 7 || string.IsNullOrWhiteSpace(parts[6])) 
                        continue;
                    list.Add(parts[6].Trim().Trim('"'));                    
                }
                var categoryList = list.Distinct().ToList();
                foreach(var cl in categoryList)
                {
                    using var insertCmd = new SqliteCommand("INSERT INTO Categories(Name) VALUES (@n);", conn);
                    insertCmd.Parameters.AddWithValue("@n", cl);
                    insertCmd.ExecuteNonQuery();
                }                
            }
        }

        private static void ImportManufacturers()
        {
            var list = new List<string>();
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imports", "csv", "Tovar.csv");
            using var conn = GetConn();
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Manufacturers", conn);
            if((long)checkCmd.ExecuteScalar()==0)
            {
                string[] lines = File.ReadAllLines(filePath);
                for(int i=1;i<lines.Length;i++)
                {
                    string currentLines = lines[i];
                    string[] parts = currentLines.Split(';');
                    if (parts.Length < 6 || string.IsNullOrWhiteSpace(parts[5]))
                        continue;
                    list.Add(parts[5].Trim());
                }
                var manufacturerList = list.Distinct().ToList();
                foreach(var ml in manufacturerList)
                {
                    using var insertCmd = new SqliteCommand("INSERT INTO Manufacturers(Name) VALUES (@n);", conn);
                    insertCmd.Parameters.AddWithValue("@n",ml);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private static void ImportSuppliers()
        {
            var list = new List<string>();
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imports", "csv", "Tovar.csv");
            using var conn = GetConn();
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Suppliers", conn);
            if ((long)checkCmd.ExecuteScalar() == 0)
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    string currentLines = lines[i];
                    string[] parts = currentLines.Split(';');
                    if (parts.Length < 5 || string.IsNullOrWhiteSpace(parts[4]))
                        continue;
                    list.Add(parts[4].Trim());
                }
                var supplierList = list.Distinct().ToList();
                foreach (var sl in supplierList)
                {
                    using var insertCmd = new SqliteCommand("INSERT INTO Suppliers(Name) VALUES (@n);", conn);
                    insertCmd.Parameters.AddWithValue("@n", sl);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private static void ImportProducts()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imports", "csv", "Tovar.csv");
            using var conn = GetConn();
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Products", conn);
            if ((long)checkCmd.ExecuteScalar() == 0)
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    string currentLines = lines[i];
                    string[] parts = currentLines.Split(';');
                    if (parts.Length < 11 || 
                        string.IsNullOrWhiteSpace(parts[0]) ||
                        string.IsNullOrWhiteSpace(parts[1]) ||
                        string.IsNullOrWhiteSpace(parts[2]) ||
                        string.IsNullOrWhiteSpace(parts[3]) ||
                        string.IsNullOrWhiteSpace(parts[4]) ||
                        string.IsNullOrWhiteSpace(parts[5]) ||
                        string.IsNullOrWhiteSpace(parts[6]) ||
                        string.IsNullOrWhiteSpace(parts[7]) ||
                        string.IsNullOrWhiteSpace(parts[8]) 
                    )
                        continue;
                    var product = new Product 
                    {
                        Article = parts[0].Trim(),
                        BookName = parts[1].Trim().Replace("\"\"", "\"").Trim('"'),
                        Unit = parts[2].Trim(),
                        Price = decimal.Parse(parts[3].Trim()),                        
                        SupplierName = parts[4].Trim(),                        
                        ManufacturerName = parts[5].Trim(),                        
                        CategoryName = parts[6].Trim(),
                        Discount = decimal.Parse(parts[7].Trim()),
                        StockQuantity = int.Parse(parts[8].Trim()),
                        Description = parts[9].Trim().Replace("\"\"", "\"").Trim('"'),
                        ImagePath = string.IsNullOrWhiteSpace(parts[10].Trim()) ? "/images/picture.png" : $"/images/{parts[10].Trim()}"
                    }
                ;
                    var sql = @"INSERT INTO Products(
                    Article,BookName,Unit,Price,SupplierId,
                    ManufacturerId,CategoryId,
                    Discount,StockQuantity,Description,ImagePath) 
                    VALUES (@art,@bn,@u,@p,
                    (SELECT Id FROM Suppliers WHERE Name=@sn),                    
                    (SELECT Id FROM Manufacturers WHERE Name=@mn),                    
                    (SELECT Id FROM Categories WHERE Name=@cn),                    
                    @disc,@sq,@desc,@img);";
                    using var insertCmd = new SqliteCommand(sql, conn);
                    insertCmd.Parameters.AddWithValue("@art", product.Article);
                    insertCmd.Parameters.AddWithValue("@bn", product.BookName);
                    insertCmd.Parameters.AddWithValue("@u", product.Unit);
                    insertCmd.Parameters.AddWithValue("@p", product.Price);                    
                    insertCmd.Parameters.AddWithValue("@sn", product.SupplierName);                    
                    insertCmd.Parameters.AddWithValue("@mn", product.ManufacturerName);                    
                    insertCmd.Parameters.AddWithValue("@cn", product.CategoryName);
                    insertCmd.Parameters.AddWithValue("@disc", product.Discount);
                    insertCmd.Parameters.AddWithValue("@sq", product.StockQuantity);
                    insertCmd.Parameters.AddWithValue("@desc", product.Description);
                    insertCmd.Parameters.AddWithValue("@img", product.ImagePath);
                    insertCmd.ExecuteNonQuery();
                }     
            }
        }

        private static void ImportPickupPoints()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"imports","csv", "PickupPoints.csv");
            using var conn = GetConn();
            string[] lines = File.ReadAllLines(filePath);
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM PickupPoints",conn);
            if((long)checkCmd.ExecuteScalar()==0)
            {
                foreach (var l in lines)
                {
                    if (string.IsNullOrWhiteSpace(l))
                        continue;
                    using var cmd = new SqliteCommand("INSERT INTO PickupPoints(Name) VALUES(@name)", conn);
                    cmd.Parameters.AddWithValue("@name", l.Trim());
                    cmd.ExecuteNonQuery();
                }
            }  
        }

        private static void ImportOrders()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imports", "csv", "Orders.csv");
            using var conn = GetConn();
            string[] lines = File.ReadAllLines(filePath);
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Orders", conn);
            if ((long)checkCmd.ExecuteScalar() == 0)
            {
                for(int i=1; i<lines.Length; i++)
                {
                    var currentLines = lines[i];
                    string[] parts = currentLines.Split(';');
                    
                        if (parts.Length < 8 ||
                            string.IsNullOrWhiteSpace(parts[2]) ||
                            string.IsNullOrWhiteSpace(parts[3]) ||
                            string.IsNullOrWhiteSpace(parts[4]) ||
                            string.IsNullOrWhiteSpace(parts[5]) ||
                            string.IsNullOrWhiteSpace(parts[6])
                            )
                            continue;

                    // Конвертируем даты из dd.MM.yyyy в yyyy-MM-dd для корректной работы input type="date"
                         string orderDate;
                         string deliveryDate;
                         try
                         {
                             orderDate = DateTime.ParseExact(parts[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                 .ToString("yyyy-MM-dd");
                             deliveryDate = DateTime.ParseExact(parts[3].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                 .ToString("yyyy-MM-dd");
                         }
                         catch (FormatException)
                         {
                             continue; // пропускаем строку с битой датой
                         }

                    var order = new Order 
                        {
                            Id = int.Parse(parts[0]),
                            OrderDate = orderDate,
                            DeliveryDate = deliveryDate,
                            PickupPointId = int.Parse(parts[4]),
                            FullName = parts[5].Trim(),
                            Code = int.Parse(parts[6]),
                            Status = parts[7].Trim()
                        };
                    var sql = @"INSERT INTO Orders(Id,OrderDate,DeliveryDate,PickupPointId,UserId,Code,Status) 
                        VALUES(@id,@od,@dd,@ppId,(SELECT Id FROM Users WHERE FullName=@un),@c,@s);";
                    using var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", order.Id);
                    cmd.Parameters.AddWithValue("@od", order.OrderDate);
                    cmd.Parameters.AddWithValue("@dd", order.DeliveryDate);
                    cmd.Parameters.AddWithValue("@ppId", order.PickupPointId);
                    cmd.Parameters.AddWithValue("@un", order.FullName);
                    cmd.Parameters.AddWithValue("@c", order.Code);
                    cmd.Parameters.AddWithValue("@s", order.Status);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void ImportOrderItems()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imports", "csv", "Orders.csv");
            using var conn = GetConn();
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM OrderItems", conn);
            if ((long)checkCmd.ExecuteScalar() == 0)
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    var currentLines = lines[i];
                    string[] parts = currentLines.Split(';');
                    if (parts.Length < 8 || string.IsNullOrWhiteSpace(parts[1])) continue;

                    int orderId = int.Parse(parts[0].Trim());
                    string[] part = parts[1].Split(',');
                    if (part.Length < 2) continue;

                    for (int j = 0; j < part.Length; j += 2)
                    {
                        string article = part[j].Trim();
                        if (string.IsNullOrWhiteSpace(article)) continue;
                        int quantity = int.Parse(part[j + 1].Trim());

                        // Вставляем только если и заказ, и товар реально существуют в БД.
                        // Иначе (заказ был пропущен в ImportOrders или товар отсутствует) —
                        // строка просто не вставится, без FK-ошибки.
                        var sql = @"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity)
                    SELECT @oId, p.Id, @q
                    FROM Products p
                    WHERE p.Article = @art
                      AND EXISTS (SELECT 1 FROM Orders WHERE Id = @oId);";

                        using var cmd = new SqliteCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@oId", orderId);
                        cmd.Parameters.AddWithValue("@art", article);
                        cmd.Parameters.AddWithValue("@q", quantity);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static User GetUser(string login,string password)
        {
            using var conn = GetConn();
            var sql = "SELECT Id, Login, Password, FullName, Role FROM Users WHERE Login = @l AND Password = @p;";
            using var cmd = new SqliteCommand(sql,conn);
            cmd.Parameters.AddWithValue("@l", login);
            cmd.Parameters.AddWithValue("@p", password);
            using var reader = cmd.ExecuteReader();
            if(reader.Read())
            return new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                Password = reader.GetString(2),
                FullName = reader.GetString(3),
                Role = reader.GetString(4)
            };
            return null;
        }

        public static List<Category> GetCategories()
        {
            var list = new List<Category>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT Id,Name FROM Categories ORDER BY Name;", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            return list;            
        }

        public static List<Manufacturer> GetManufacturers()
        {
            var list = new List<Manufacturer>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT Id,Name FROM Manufacturers ORDER BY Name;", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Manufacturer
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            return list;
        }

        public static List<Supplier> GetSuppliers()
        {
            var list = new List<Supplier>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT Id,Name FROM Suppliers ORDER BY Name;", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Supplier
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            return list;
        }

        public static List<PickupPoint> GetPickupPoints()
        {
            var list = new List<PickupPoint>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT Id,Name FROM PickupPoints ORDER BY Name;", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new PickupPoint
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            return list;
        }

        public static List<Product> GetProducts(string search="",string discountRange="",string sortColumn="p.Price",string sortDirection="ASC", int offset=0)
        {
            var list = new List<Product>();
            using var conn = GetConn();
            var sql = @"SELECT 
            p.Id,
            p.Article,
            p.BookName,
            p.Unit,
            p.Price,
            p.SupplierId,
            s.Name AS SupplierName,
            p.ManufacturerId,
            m.Name AS ManufacturerName,
            p.CategoryId,
            c.Name AS CategoryName,
            p.Discount,
            p.StockQuantity,
            p.Description,
            p.ImagePath
            FROM Products p
            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
            LEFT JOIN Manufacturers m ON p.ManufacturerId = m.Id
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE 1=1
            ";

            var parameters = new List<SqliteParameter>();

            if(!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND(p.Article LIKE @s OR p.BookName LIKE @s OR p.Description LIKE @s OR s.Name LIKE @s OR m.Name LIKE @s OR c.Name LIKE @s)";
                parameters.Add(new SqliteParameter("@s", $"%{search}%"));
            }
            
            if(discountRange== "0-12")
            {
                sql +=" AND p.Discount BETWEEN 0 AND 12.99";
            }
            else if (discountRange == "13-16")
            {
                sql += " AND p.Discount BETWEEN 13 AND 16.99";
            }
            else if (discountRange == "17+")
            {
                sql += " AND p.Discount >=17";
            }

            sql += $" ORDER BY {sortColumn} {sortDirection}";

            sql += $" LIMIT @limit OFFSET @offset";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@limit",5);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddRange(parameters.ToArray());
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Product
                {
                    
                    Id = reader.GetInt32(0),                    
                    Article = reader.GetString(1),
                    BookName = reader.GetString(2),
                    Unit = reader.GetString(3),
                    Price = reader.GetDecimal(4),
                    SupplierId = reader.GetInt32(5),
                    SupplierName = reader.GetString(6),
                    ManufacturerId = reader.GetInt32(7),
                    ManufacturerName = reader.GetString(8),
                    CategoryId = reader.GetInt32(9),
                    CategoryName = reader.GetString(10),
                    Discount = reader.GetDecimal(11),
                    StockQuantity = reader.GetInt32(12),
                    Description = reader.IsDBNull(13) ? "" : reader.GetString(13),
                    ImagePath = reader.IsDBNull(14) ? null : reader.GetString(14)
                });
            return list;
        }

        public static List<Order> GetOrders()
        {
            var list = new List<Order>();
            using var conn = GetConn();
            var sql = @"SELECT o.Id, o.OrderDate, o.DeliveryDate, o.PickupPointId, 
                pp.Name AS PickupPointName, o.UserId,u.FullName AS FullName, o.Code, o.Status 
                FROM Orders o 
                LEFT JOIN Users u ON o.UserId = u.Id
                LEFT JOIN PickupPoints pp ON o.PickupPointId = pp.Id;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Order
                {
                    Id = reader.GetInt32(0),
                    OrderDate = reader.GetString(1),
                    DeliveryDate = reader.GetString(2),
                    PickupPointId = reader.GetInt32(3),
                    PickupPointName = reader.GetString(4),
                    UserId = reader.GetInt32(5),
                    FullName = reader.GetString(6),
                    Code = reader.GetInt32(7),
                    Status = reader.GetString(8)
                });
            return list;
        }
        
        public static void InsertOrder(Order o)
        {
            using var conn = GetConn();
            var sql = @"INSERT INTO Orders(OrderDate, DeliveryDate, PickupPointId, UserId, Code,Status) 
                    VALUES(@od,@dd,@ppId,@uId,(SELECT COALESCE(MAX(Code), 0) + 1 FROM Orders),@s)";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@od", o.OrderDate);
            cmd.Parameters.AddWithValue("@dd", o.DeliveryDate);
            cmd.Parameters.AddWithValue("@ppId", o.PickupPointId);
            cmd.Parameters.AddWithValue("@uId", o.UserId);            
            cmd.Parameters.AddWithValue("@s", o.Status);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateOrder(Order o)
        {
            using var conn = GetConn();
            var sql = @"UPDATE Orders SET DeliveryDate=@dd, 
            PickupPointId=@ppId,Status=@s WHERE Id=@id;";                     
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", o.Id);            
            cmd.Parameters.AddWithValue("@dd", o.DeliveryDate);
            cmd.Parameters.AddWithValue("@ppId", o.PickupPointId);                      
            cmd.Parameters.AddWithValue("@s", o.Status);
            cmd.ExecuteNonQuery();
        }

        public static List<OrderItem> GetOrderItems()
        {
            var list = new List<OrderItem>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT ProductId, OrderId, Quantity FROM OrderItems;", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new OrderItem
                {
                    ProductId = reader.GetInt32(0),
                    OrderId = reader.GetInt32(1),
                    Quantity = reader.GetInt32(2)
                });
            return list;
        }
        
        public static void DeleteOrder(int id)
        {
            using var conn = GetConn();          
            using var deleteOrderItems = new SqliteCommand("DELETE FROM OrderItems WHERE OrderId = @id", conn);
            using var deleteOrderId = new SqliteCommand("DELETE FROM Orders WHERE Id = @id", conn);
            deleteOrderItems.Parameters.AddWithValue("@id", id);
            deleteOrderItems.ExecuteNonQuery();
            deleteOrderId.Parameters.AddWithValue("@id", id);
            deleteOrderId.ExecuteNonQuery();
        }

        public static void DeleteProduct(int id)
        {
            using var conn = GetConn();
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM OrderItems WHERE ProductId=@id",conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            if ((long)checkCmd.ExecuteScalar() > 0)
                throw new InvalidOperationException("Нельзя удалить товар присутствующий в заказе");
            using var cmd = new SqliteCommand("DELETE FROM Products WHERE Id=@id",conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        
        public static void InsertProduct(Product p)
        {
            using var conn = GetConn();
            var insert = @"INSERT INTO Products(
                Article,
                BookName,
                Unit,
                Price,
                SupplierId,
                ManufacturerId,                
                CategoryId,
                Discount,
                StockQuantity,
                Description,
                ImagePath) 
                VALUES(@art, @b, @u, @p, @sId, @mId, @cId, @disc, @sq, @desc, @img);";
            using var cmd = new SqliteCommand(insert, conn);
            cmd.Parameters.AddWithValue("@art",p.Article);
            cmd.Parameters.AddWithValue("@b", p.BookName);
            cmd.Parameters.AddWithValue("@u", p.Unit);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@sId", p.SupplierId);
            cmd.Parameters.AddWithValue("@mId", p.ManufacturerId);
            cmd.Parameters.AddWithValue("@cId", p.CategoryId);
            cmd.Parameters.AddWithValue("@disc", p.Discount);
            cmd.Parameters.AddWithValue("@sq", p.StockQuantity);
            cmd.Parameters.AddWithValue("@desc", p.Description ??"");
            cmd.Parameters.AddWithValue("@img", (object?)p.ImagePath ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateProduct(Product p)
        {
            using var conn = GetConn();
            var insert = @"UPDATE Products SET
                Article = @art,
                BookName = @b,
                Unit = @u,
                Price = @p,
                SupplierId = @sId,
                ManufacturerId = @mId,                
                CategoryId = @cId,
                Discount = @disc,
                StockQuantity = @sq,
                Description = @desc,
                ImagePath = @img 
                WHERE Id=@id;";
            using var cmd = new SqliteCommand(insert, conn);
            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.Parameters.AddWithValue("@art", p.Article);
            cmd.Parameters.AddWithValue("@b", p.BookName);
            cmd.Parameters.AddWithValue("@u", p.Unit);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@sId", p.SupplierId);
            cmd.Parameters.AddWithValue("@mId", p.ManufacturerId);
            cmd.Parameters.AddWithValue("@cId", p.CategoryId);
            cmd.Parameters.AddWithValue("@disc", p.Discount);
            cmd.Parameters.AddWithValue("@sq", p.StockQuantity);
            cmd.Parameters.AddWithValue("@desc", p.Description ?? "");
            cmd.Parameters.AddWithValue("@img", (object?)p.ImagePath ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
}
