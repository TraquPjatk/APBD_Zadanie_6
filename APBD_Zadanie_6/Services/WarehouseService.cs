using System.Data;
using APBD_Task_6.Models;
using System.Data.SqlClient;

namespace Zadanie5.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IConfiguration _configuration;
        private readonly IWarehouseService _warehouseService;
        private static List<ProductWarehouse> _productsInWarehouse = new List<ProductWarehouse>();


        public WarehouseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void AddProduct(ProductWarehouse product)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand();
                command.Connection = connection; // mam połączenie UwU

                try
                {
                
                    if (productExists(product.IdProduct, connection))
                    {
                        throw new AggregateException("Product with id: " + product.IdProduct + " exists.");
                    }

                    if (warehouseExists(product.IdWarehouse, connection))
                    {
                        throw new AggregateException("Warehouse with id: " + product.IdWarehouse + " exists.");
                    }

                    if (product.Amount <= 0)
                    {
                        throw new AggregateException("Product amount should be grater than 0!");
                    }

                    _productsInWarehouse.Add(product);
                    command.CommandText = "" +
                                          "INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, Amount, CreatedAt) " +
                                          "VALUES (@IdProduct, @IdWarehouse, @Amount, @CreatedAt)";

                    command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
                    command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
                    command.Parameters.AddWithValue("@Amount", product.Amount);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    command.ExecuteNonQuery();

                    _warehouseService.AddProduct(product);
                    
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
                // _productsInWarehouse.Add(product);
            }
        }

        private bool productExists(int productId, SqlConnection connection)
        {
            using var command = new SqlCommand("SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct", connection);
            command.Parameters.AddWithValue("@IdProduct", productId);
            connection.Open();
            int count = (int)command.ExecuteScalar(); //to mi wykonuje select
            
            return count > 0;
        }

        private bool warehouseExists(int warehouseId, SqlConnection connection)
        {
            using var command = new SqlCommand("SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse",
                connection);
            command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
            connection.Open();
            int count = (int)command.ExecuteScalar(); //to mi wykonuje select

            return count > 0;
        }
    }
}