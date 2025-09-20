// File: Services/PaymobPaymentService.cs
using X.Paymob.CashIn;
using ARMuseum.Data.Models;
using X.Paymob.CashIn.Models.Orders;
using X.Paymob.CashIn.Models.Payment;
using System;
using System.Threading.Tasks;

// Service responsible for handling payment logic with the Paymob gateway.
public class PaymobPaymentService
{
    private readonly IPaymobCashInBroker _paymob;
    private readonly OurDbContext _context;

    public PaymobPaymentService(IPaymobCashInBroker paymob, OurDbContext context)
    {
        _paymob = paymob;
        _context = context;
    }

    /// <summary>
    /// Initiates a payment process with Paymob and returns a payment URL for the iframe.
    /// </summary>
    public async Task<string> InitiatePaymentAsync(int userId, int ticketId, decimal amount, string currency, int mId)
    {
        // Step 1: Create an order with Paymob to get an order ID.
        int amountCents = (int)(amount * 100);
        var order = CashInCreateOrderRequest.CreateOrder(amountCents);
        var orderResp = await _paymob.CreateOrderAsync(order);

        // TODO: Replace this placeholder with actual user billing data from your database or the request.
        var billing = new CashInBillingData(
            firstName: "First",
            lastName: "Last",
            phoneNumber: "01xxxxxxx",
            email: "test@test.com"
        );

        // Step 2: Request a payment key from Paymob, which is a short-lived token to authorize the payment.
        var paymentKeyReq = new CashInPaymentKeyRequest(
            integrationId: 0, // TODO: Replace with your Integration ID from a secure configuration.
            orderId: orderResp.Id,
            billingData: billing,
            amountCents: amountCents
        );
        var paymentKeyResp = await _paymob.RequestPaymentKeyAsync(paymentKeyReq);

        // Step 3: Save a record of the transaction to the database with a "Pending" status.
        var entity = new TbBuyAticket
        {
            TicketId = ticketId,
            UId = userId,
            MId = mId,
            TOrderId = orderResp.Id,
            TCreatedAt = DateTime.UtcNow,
            TCurrency = currency,
            TIsRefund = "No",
            TSucces = "Pending",
            TAmountCents = amountCents
        };
        _context.TbBuyAtickets.Add(entity);
        await _context.SaveChangesAsync();

        // Step 4: Generate the final iframe URL to be used by the client.
        return _paymob.CreateIframeSrc(
            iframeId: "", // TODO: Replace with your Iframe ID from a secure configuration.
            token: paymentKeyResp.PaymentKey
        );
    }
}