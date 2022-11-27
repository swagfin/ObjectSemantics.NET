using System;

namespace ObjectSemantics.NET.Tests.MoqModels
{
    internal class Invoice
    {
        public int Id { get; set; }
        public string RefNo { get; set; }
        public string Narration { get; set; }
        public double Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
}
