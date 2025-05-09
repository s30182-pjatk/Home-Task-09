using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Task_09.Model;
using Task_09.Service;

namespace Task_09.Controller
{
    
    
    
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        
        private readonly IWarehouseService _service;

        public WarehousesController(IWarehouseService service)
        {
            _service = service;
        }
        
        [HttpPut("query")]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] ReceiveProductDTO receiveProduct)
        {
            var result = await _service.AddProductToWarehouse(receiveProduct);
            return result != -1 ? Ok(result) : BadRequest();
        }
        
        [HttpPut("procedure")]
        public async Task<IActionResult> AddProductToWarehouseProcedure([FromBody] ReceiveProductDTO receiveProduct)
        {
            var result = await _service.AddProductToWarehouse(receiveProduct);
            return result != -1 ? Ok(result) : BadRequest();
        }
        
    }
}
