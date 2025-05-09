using Task_09.Model;

namespace Task_09.Service;

public interface IWarehouseService
{
    Task<int> AddProductToWarehouse(ReceiveProductDTO receiveProduct);
    Task<int> AddProductToWarehouseProcedure(ReceiveProductDTO receiveProduct);
}