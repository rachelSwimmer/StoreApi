# Plan: Add Payment Options to Orders (PayPal, CreditCard, GooglePay)

Extend the order system to support multiple payment methods by adding a payment enum, new model fields, updated DTOs, and optional payment service infrastructure for future gateway integration.

## Steps

1. **Create `PaymentMethod` enum** in a new Models/PaymentMethod.cs with values: `CreditCard`, `PayPal`, `GooglePay`

2. **Extend the Order model** in Models/Order.cs — add `PaymentMethod` (enum), `PaymentStatus` (string: Pending/Completed/Failed/Refunded), and `TransactionId` (string, nullable) properties

3. **Update Order DTOs** in DTOs/OrderDTOs.cs:
   - Add `PaymentMethod` (required) to `OrderCreateDto`
   - Add `PaymentStatus` and `TransactionId` to `OrderResponseDto`
   - Optionally add `PaymentStatus` to `OrderUpdateDto` for admin updates

4. **Update OrderService** in Services/OrderService.cs — set default `PaymentStatus` to "Pending" on creation, map new fields in response DTOs

5. **Update ApplicationDbContext** in Data/ApplicationDbContext.cs — configure `PaymentMethod` enum conversion and `TransactionId` max length

6. **Create EF Core migration** to add the new payment columns to the Orders table

## Further Considerations

1. **Payment Gateway Integration?** Should this plan include actual payment processing services (PayPal SDK, Stripe, Google Pay API), or just the data model for now? *Recommend: Model first, gateway integration as Phase 2*

2. **Payment Timestamps?** Do you want to track `PaidAt` timestamp separately from order status dates?

3. **Separate Payment Entity?** For complex scenarios (partial payments, refunds), consider a separate `Payment` model linked to orders instead of embedding in Order. *Recommend: Start embedded, refactor later if needed*
