using System;

namespace ObjectSemantics.NET.Tests.MoqModels
{
    public class CustomerPayment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public double Amount { get; set; }
        public string ReferenceNo { get; set; }
        public string PaidBy { get; set; }
        public string PaidByMobile { get; set; }
        public string PaymentStatus { get; set; }
        public string RegisteredBy { get; set; }
        public string Narration { get; set; }
        public string CustomerName { get; set; }
        public string LedgerAccountName { get; set; }
        public int LedgerAccountId { get; set; }
        public DateTime PaymentDate { get; set; }
        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
    }
}
