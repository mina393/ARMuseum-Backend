using ARMuseum.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using X.Paymob.CashIn.Models.Callback;
using X.Paymob.CashIn;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymobPaymentService _paymentService;
    private readonly OurDbContext _context; // Entity Framework DbContext

    public PaymentController(PaymobPaymentService paymentService, OurDbContext context)
    {
        _paymentService = paymentService;
        _context = context;
    }

    // POST: api/Payment/buy
    // Initiates the payment process by creating an order and a payment key.
    [HttpPost("buy")]
    public async Task<IActionResult> BuyTicket([FromBody] BuyTicketRequestDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find the user in TbUsers by their AspNetUserId to get the internal numeric UId.
        var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == model.UId);
        if (tbUser == null)
        {
            return Unauthorized("User not found or not associated with a valid TbUser record.");
        }

        var payUrl = await _paymentService.InitiatePaymentAsync(tbUser.UId, model.TicketId, model.Amount, model.Currency, model.MId);
        return Ok(new { payment_url = payUrl });
    }

    // This is required to handle Paymob's use of numbers as strings in callbacks.
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    // POST: api/Payment/paymob-callback
    // Endpoint for receiving transaction status callbacks from Paymob.
    [HttpPost("paymob-callback")]
    public async Task<IActionResult> PaymobCallback([FromQuery] string hmac,
                                                     [FromBody] CashInCallback callback,
                                                     [FromServices] IPaymobCashInBroker broker)
    {
        if (callback.Type is null || callback.Obj is null)
            return BadRequest("Callback missing data");

        var content = ((JsonElement)callback.Obj).GetRawText();

        switch (callback.Type.ToUpperInvariant())
        {
            case CashInCallbackTypes.Transaction:
                {
                    var transaction = JsonSerializer.Deserialize<CashInCallbackTransaction>(content, SerializerOptions)!;
                    var valid = broker.Validate(transaction, hmac);
                    if (!valid) return BadRequest("HMAC validation failed");

                    // Find the order in the database by its OrderId.
                    var item = await _context.TbBuyAtickets.FindAsync(transaction.Order.Id);
                    if (item == null) return NotFound();

                    // Update the payment status.
                    item.TSucces = transaction.Success ? "Yes" : "No";
                    await _context.SaveChangesAsync();

                    return Ok();
                }
            default:
                return BadRequest("Unhandled callback type");
        }
    }
}