using System.Data;
using Microsoft.Data.SqlClient;
using Task_09.Model;

namespace Task_09.Service;

public class WarehouseService : IWarehouseService
{
    private string _connectionString = "Server=localhost,1434;Database=APBD;User Id=sa;Password=myStrongPassword!;TrustServerCertificate=True;";
    //Task 01
    public async Task<int> AddProductToWarehouse(ReceiveProductDTO receiveProduct)
    {
        string prodctExistsQuery = "select count(*) from Product where IdProduct = @idProduct";
        string warehouseExistsQuery = "select count(*) from Warehouse where IdWarehouse = @idWarehouse";

        string DateAmountProductIdCheckQuery =
            "select top 1 IdOrder from  [Order] where Amount = @amount and IdProduct = @idProduct and CreatedAt < @date";

        string checkOrderFulfilmentQuery = "select count(*) from Product_Warehouse where IdOrder = @orderId";

        string updateOrderFulfilmentQuery = "update [Order] set FulfilledAt = GETDATE() where IdOrder = @orderId";

        string getPriceQuery = "select @amount * Price from Product where IdProduct = @idProduct";

        string insertQuery =
            "insert into Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) values (@idWarehouse, @idProduct, @idOrder, @amount, @price, GETDATE());SELECT SCOPE_IDENTITY();";



        using (SqlConnection conn = new SqlConnection(_connectionString)){
            
            await conn.OpenAsync();
            
            SqlTransaction transaction = conn.BeginTransaction();
            
            using (SqlCommand cmdProductExists = new SqlCommand(prodctExistsQuery, conn, transaction))
            using (SqlCommand cmdWarehouseExists = new SqlCommand(warehouseExistsQuery, conn, transaction))
            using (SqlCommand cmdDateAmountProductIdCheck = new SqlCommand(DateAmountProductIdCheckQuery, conn, transaction))
            using (SqlCommand cmdCheckOrderFulfilment = new SqlCommand(checkOrderFulfilmentQuery, conn, transaction))
            using (SqlCommand cmdUpdateOrder = new SqlCommand(updateOrderFulfilmentQuery, conn, transaction))
            using (SqlCommand cmdGetPrice = new SqlCommand(getPriceQuery, conn, transaction))
            using (SqlCommand cmdInsertProduct = new SqlCommand(insertQuery, conn, transaction))
            {

                

                // Check if Amount is greater than 0
                if (receiveProduct.Amount <= 0)
                {
                    return -1;
                }

                // Check if product and warehouse ids exist
                cmdProductExists.Parameters.AddWithValue("@idProduct", receiveProduct.IdProduct);
                cmdWarehouseExists.Parameters.AddWithValue("@idWarehouse", receiveProduct.IdWarehouse);


                var resultProductExists = await cmdProductExists.ExecuteScalarAsync();
                var resultWarehouseExists = await cmdWarehouseExists.ExecuteScalarAsync();

                if (Convert.ToInt32(resultProductExists) == 0 || Convert.ToInt32(resultWarehouseExists) == 0)
                {
                    transaction.Rollback();
                    return -1;
                }

                // Check if order date is earlier than request
                cmdDateAmountProductIdCheck.Parameters.AddWithValue("@amount", receiveProduct.Amount);
                cmdDateAmountProductIdCheck.Parameters.AddWithValue("@idProduct", receiveProduct.IdProduct);
                cmdDateAmountProductIdCheck.Parameters.AddWithValue("@date", receiveProduct.CreatedAt);

                // Returns Order id
                var resultOrderID = await cmdDateAmountProductIdCheck.ExecuteScalarAsync();
                int orderId = Convert.ToInt32(resultOrderID);

                if (orderId == 0)
                {
                    transaction.Rollback();
                    return -1;
                }

                // Check for idOrder in Order, if Order is already completed
                cmdCheckOrderFulfilment.Parameters.AddWithValue("@orderId", orderId);
                var checkFulfilment = await cmdCheckOrderFulfilment.ExecuteScalarAsync();

                if (Convert.ToInt32(checkFulfilment) != 0)
                {
                    transaction.Rollback();
                    return -1;
                }

                // Update fulfillment date
                cmdUpdateOrder.Parameters.AddWithValue("@orderId", orderId);
                await cmdUpdateOrder.ExecuteNonQueryAsync();

                // Calculate price
                cmdGetPrice.Parameters.AddWithValue("@amount", receiveProduct.Amount);
                cmdGetPrice.Parameters.AddWithValue("@idProduct", receiveProduct.IdProduct);
                var resultPrice = await cmdGetPrice.ExecuteScalarAsync();
                var price = Convert.ToDecimal(resultPrice);

                // Insert values to Product_Warehouse
                cmdInsertProduct.Parameters.AddWithValue("@idWarehouse", receiveProduct.IdWarehouse);
                cmdInsertProduct.Parameters.AddWithValue("@idProduct", receiveProduct.IdProduct);
                cmdInsertProduct.Parameters.AddWithValue("@idOrder", orderId);
                cmdInsertProduct.Parameters.AddWithValue("@amount", receiveProduct.Amount);
                cmdInsertProduct.Parameters.AddWithValue("@price", price);

                var resultProduct = await cmdInsertProduct.ExecuteScalarAsync();

                transaction.Commit();
                return Convert.ToInt32(resultProduct);
            }
        }
    }
    
    // Task 02
    public async Task<int> AddProductToWarehouseProcedure(ReceiveProductDTO receiveProduct)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using(SqlCommand sqlCommand = new SqlCommand("AddProductToWarehouse", conn))
        {
            sqlCommand.CommandType = CommandType.StoredProcedure;
            
            await conn.OpenAsync();
            
            var resultProduct = await sqlCommand.ExecuteScalarAsync();
            return Convert.ToInt32(resultProduct);
        }
    }
}