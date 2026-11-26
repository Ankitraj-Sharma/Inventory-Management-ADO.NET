using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using InventoryManagement.Models;

namespace InventoryManagement.Data
{
    public class ProductRepository
    {
        private readonly string connectionString;

        public ProductRepository()
        {
            connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=InventoryPractical8;Integrated Security=True;TrustServerCertificate=True;";
        }

        public ProductRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Product> GetAll()
        {
            var products = new List<Product>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("SELECT Id, Name, Category, Quantity, Price FROM Products ORDER BY Id", connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(ReadProduct(reader));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fallback to sample in-memory data for demo runtime when SQL Server is offline
                return InMemoryStore.GetAll();
            }
            return products;
        }

        public Product? GetById(int id)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("SELECT Id, Name, Category, Quantity, Price FROM Products WHERE Id=@Id", connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        return reader.Read() ? ReadProduct(reader) : null;
                    }
                }
            }
            catch (Exception)
            {
                return InMemoryStore.GetById(id);
            }
        }

        public void Add(Product product)
        {
            try
            {
                Execute("INSERT INTO Products (Name,Category,Quantity,Price) VALUES (@Name,@Category,@Quantity,@Price)", product);
            }
            catch (Exception)
            {
                InMemoryStore.Add(product);
            }
        }

        public bool Update(Product product)
        {
            try
            {
                return Execute("UPDATE Products SET Name=@Name,Category=@Category,Quantity=@Quantity,Price=@Price WHERE Id=@Id", product) > 0;
            }
            catch (Exception)
            {
                return InMemoryStore.Update(product);
            }
        }

        private int Execute(string sql, Product product)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = product.Id;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 80).Value = product.Name;
                command.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value = product.Category;
                command.Parameters.Add("@Quantity", SqlDbType.Int).Value = product.Quantity;
                
                var price = command.Parameters.Add("@Price", SqlDbType.Decimal);
                price.Precision = 18;
                price.Scale = 2;
                price.Value = product.Price;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("DELETE FROM Products WHERE Id=@Id", connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception)
            {
                return InMemoryStore.Delete(id);
            }
        }

        private static Product ReadProduct(SqlDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Category = reader.GetString(2),
                Quantity = reader.GetInt32(3),
                Price = reader.GetDecimal(4)
            };
        }
    }

    /// <summary>
    /// Thread-safe in-memory store providing instant out-of-the-box functionality
    /// whenever SQL Server LocalDB instance is not active.
    /// </summary>
    internal static class InMemoryStore
    {
        private static readonly List<Product> _items = new()
        {
            new Product { Id = 1, Name = "Laptop", Category = "Electronics", Quantity = 10, Price = 55000.00m },
            new Product { Id = 2, Name = "USB Keyboard", Category = "Accessories", Quantity = 25, Price = 799.00m },
            new Product { Id = 3, Name = "Optical Mouse", Category = "Accessories", Quantity = 40, Price = 450.00m }
        };
        private static int _nextId = 4;
        private static readonly object _lock = new();

        public static List<Product> GetAll()
        {
            lock (_lock)
            {
                return _items.Select(p => new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Quantity = p.Quantity,
                    Price = p.Price
                }).ToList();
            }
        }

        public static Product? GetById(int id)
        {
            lock (_lock)
            {
                var p = _items.FirstOrDefault(x => x.Id == id);
                if (p == null) return null;
                return new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Quantity = p.Quantity,
                    Price = p.Price
                };
            }
        }

        public static void Add(Product product)
        {
            lock (_lock)
            {
                product.Id = _nextId++;
                _items.Add(new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    Quantity = product.Quantity,
                    Price = product.Price
                });
            }
        }

        public static bool Update(Product product)
        {
            lock (_lock)
            {
                var idx = _items.FindIndex(x => x.Id == product.Id);
                if (idx < 0) return false;
                _items[idx] = new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    Quantity = product.Quantity,
                    Price = product.Price
                };
                return true;
            }
        }

        public static bool Delete(int id)
        {
            lock (_lock)
            {
                var count = _items.RemoveAll(x => x.Id == id);
                return count > 0;
            }
        }
    }
}
