using System;

namespace ObjectSemantics.NET.Tests.MoqModels
{
    public class CustomerPayment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public double Amount { get; set; }
        public int LedgerAccountId { get; set; }
        public string ReferenceNo { get; set; }
        public string PaidBy { get; set; }
        public string PaidByMobile { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public string RegisteredBy { get; set; }
        public Customer Customer { get; set; }
        public string Narration { get; set; }
        public string CustomerName { get; set; }
        public string LedgerAccountName { get; set; }
        public LedgerAccount LedgerAccount { get; set; }
        public int CompanyBranchId { get; set; }
        public object CompanyBranch { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string PrimaryMobile { get; set; }
        public string SecondaryMobile { get; set; }
        public string PrimaryEmail { get; set; }
        public string SecondaryEmail { get; set; }
        public string Address { get; set; }
        public double OpeningBalance { get; set; }
        public DateTime OpeningDate { get; set; }
        public double CurrentBalance { get; set; }
        public double CreditLimit { get; set; }
        public string TaxRefNo { get; set; }
        public string BankAccount { get; set; }
        public string RoutingNo { get; set; }
        public string MoreDetails { get; set; }
        public DateTime LastModified { get; set; }
        public int LastModifiedAtBranchId { get; set; }
        public string ModifiedBy { get; set; }
        public string DefaultPriceScheme { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public string FullName { get; set; }
    }

    public class LedgerAccount
    {
        public int Id { get; set; }
        public int LedgerAccountTypeId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string Description { get; set; }
        public double OpeningBalance { get; set; }
        public DateTime OpeningDate { get; set; }
        public bool IsBankAccount { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string AcCode { get; set; }
        public object LedgerAccountType { get; set; }
    }
}
