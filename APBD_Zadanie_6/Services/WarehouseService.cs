﻿using APBD_Task_6.Models;
using System.Data.SqlClient;

namespace Zadanie5.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IConfiguration _configuration;
        private readonly IWarehouseService _warehouseService;
        private static List<ProductWarehouse> _productsInWarehouse = new List<ProductWarehouse>();


        public WarehouseService(IWarehouseService warehouseService, IConfiguration configuration)
        {
            _configuration = configuration;
            _warehouseService = warehouseService;
        }

        public void AddProduct(ProductWarehouse product)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand();
            command.Connection = connection; //mam połączenie UwU

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
                                  "INSERT INTO ProductWarehouse (IdProduct, IdWarehouse, Amount, CreatedAt) " +
                                  "VALUES (@IdProduct, @IdWarehouse, @Amount, @CreatedAt)";

            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            command.ExecuteNonQuery();

            _warehouseService.AddProduct(product);
            // _productsInWarehouse.Add(product);
        }

        private bool productExists(int productId, SqlConnection connection)
        {
            using var command = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Id = @ProductId", connection);
            command.Parameters.AddWithValue("@ProductId", productId);
            connection.Open();
            int count = (int)command.ExecuteScalar(); //to mi wykonuje select

            return count > 0;
        }

        private bool warehouseExists(int warehouseId, SqlConnection connection)
        {
            using var command = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Id = @WarehouseId", connection);
            command.Parameters.AddWithValue("@WarehouseId", warehouseId);
            connection.Open();
            int count = (int)command.ExecuteScalar(); //to mi wykonuje select

            return count > 0;
        }
    }
}
