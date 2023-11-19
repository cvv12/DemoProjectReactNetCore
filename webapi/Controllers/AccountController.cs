using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Models;
using webapi.RequestModel;
using webapi.Services;

namespace webapi.Controllers
{
    [EnableCors]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("GetAccounts")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAccounts(int pageNumber = 1, int pageSize = 3)
        {
            var (accounts, totalCount) = await _accountService.GetAccountsAsync(pageNumber, pageSize);
            var result = new
            {
                Accounts = accounts,
                totalCount = totalCount
            };

            return Ok(result);
        }


        [HttpPost("AddAccount")]
        public async Task<IActionResult> AddAccount([FromBody] AccountRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            bool accountExists = await _accountService.CustomerHasAccountTypeAsync(model.CustomerId, model.Account.AccountType);
            if (accountExists)
            {
                return BadRequest($"A person can only have one {model.Account.AccountType} account.");
            }
            model.Account.AccountNumber = GeneratorHelper.GenerateAccountNumber(model.Account.AccountType);
            int accountId = await _accountService.AddAccountAsync(model);
            if (accountId > 0)
            {
                var account = await _accountService.GetAccountByIdAsync(accountId);
                return CreatedAtAction(nameof(GetAccountById), new { id = accountId }, account);
            }

            return BadRequest("Unable to add account");
        }

        [HttpPut("UpdateAccount")]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
        {
            var existingAccount = await _accountService.GetAccountByIdAsync(request.AccountId);

            if (existingAccount == null)
            {
                return NotFound($"Account with ID {request.AccountId} not found");
            }

            existingAccount.Balance = request.Balance;

            bool updated = await _accountService.UpdateAccountBalanceAsync(request.AccountId, request.Balance);

            if (updated)
            {
                return Ok(existingAccount); 
            }

            return BadRequest("Unable to update account balance");
        }

        [HttpGet("GetAccountById/{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account != null)
            {
                return Ok(account);
            }

            return NotFound();
        }

        [HttpDelete("DeleteAccount/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            await _accountService.DeleteAccountAsync(id);
            return Ok($"Account with ID {id} has been deleted.");
        }

    }
}
