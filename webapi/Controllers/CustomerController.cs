using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Controllers
{
    [EnableCors]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost("AddCustomer")]
        public async Task<IActionResult> AddCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!ValidationHelper.ValidateNRIC(customer.NRIC))
            {
                return BadRequest(new { message = "Invalid NRIC provided." });
            }
            if (ValidationHelper.CheckNRICExists(customer.NRIC))
            {
                return BadRequest(new { message = "NRIC already exists" });
            }

            int customerId = await _customerService.AddCustomerAsync(customer);
            return Ok(new { message = "Customer added successfully." });
        }
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchTerm = null)
        {
            var customers = await _customerService.GetCustomersAsync(pageNumber, pageSize, searchTerm);
            return Ok(customers);
        }

    }
}
