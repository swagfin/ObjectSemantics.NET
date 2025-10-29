namespace ObjectSemantics.NET.Tests.MoqModels
{
    internal class Payment
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public double Amount { get; set; } = 0;
        public string PayMethod { get; set; }
        public int? PayMethodId { get; set; }
        public string ReferenceNo { get; set; }
    }
}
