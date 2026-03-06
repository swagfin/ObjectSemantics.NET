using System;
using System.Collections.Generic;

namespace ObjectSemantics.NET.Tests.MoqModels
{
    public class OrderEmailModel
    {
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsPaid { get; set; }
        public OrderCustomer Customer { get; set; }
        public List<OrderLineItem> Items { get; set; } = new List<OrderLineItem>();
    }

    public class OrderCustomer
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal CreditLimit { get; set; }
        public List<OrderPayment> Payments { get; set; } = new List<OrderPayment>();
        public OrderAddress BillingAddress { get; set; }
    }

    public class OrderAddress
    {
        public string Line1 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class OrderLineItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class OrderPayment
    {
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
    }
}
