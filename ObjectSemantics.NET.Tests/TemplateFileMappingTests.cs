using ObjectSemantics.NET.Tests.MoqModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class TemplateFileMappingTests
    {
        [Fact]
        public void Should_Map_Large_File_Template()
        {
            string template = File.ReadAllText("MoqFiles/PaymentTemplate.xml", Encoding.UTF8);
            string expectedResult = File.ReadAllText("MoqFiles/PaymentTemplate.result.xml", Encoding.UTF8);


            var payment = new CustomerPayment
            {
                Id = 12719,
                CustomerId = 54,
                Amount = 300.0,
                LedgerAccountId = 1,
                ReferenceNo = "CP-20251029-14QH",
                PaidBy = "JOHN DOE",
                PaidByMobile = "N/A",
                PaymentDate = DateTime.Parse("2025-10-29T14:03:19.4147588"),
                PaymentStatus = null,
                RegisteredBy = "George Waynne",
                Customer = new Customer
                {
                    Id = 54,
                    FirstName = "JOHN DOE",
                    LastName = "ENTERPRISES",
                    CompanyName = "John Doe Enterprises",
                    PrimaryMobile = "N/A",
                    SecondaryMobile = null,
                    PrimaryEmail = "",
                    SecondaryEmail = null,
                    Address = "",
                    OpeningBalance = 0.0,
                    OpeningDate = DateTime.Parse("2023-11-13T00:00:00"),
                    CurrentBalance = 19095.0,
                    CreditLimit = 0.0,
                    TaxRefNo = "",
                    BankAccount = null,
                    RoutingNo = null,
                    MoreDetails = "",
                    LastModified = DateTime.Parse("2024-06-18T18:11:41.7819616"),
                    LastModifiedAtBranchId = 1,
                    ModifiedBy = "Joe Maina",
                    DefaultPriceScheme = "SELLING PRICE",
                    IsActive = true,
                    Status = "ACTIVE",
                    FullName = "Afrique"
                },
                Narration = null,
                CustomerName = "JOHN DOE ENTERPRISES",
                LedgerAccountName = "Cash A/C",
                LedgerAccount = new LedgerAccount
                {
                    Id = 1,
                    LedgerAccountTypeId = 4,
                    AccountCode = "82187",
                    AccountName = "Cash A/C",
                    Description = "Current Cash",
                    OpeningBalance = 0.0,
                    OpeningDate = DateTime.Parse("2020-03-20T10:39:12"),
                    IsBankAccount = false,
                    BankAccountName = null,
                    BankAccountNumber = null,
                    AcCode = "82187",
                    LedgerAccountType = null
                },
                CompanyBranchId = 1,
                CompanyBranch = null
            };


            //additional headers
            var additionalParams = new Dictionary<string, object>
            {
                ["BranchId"] = "1",
                ["BranchName"] = "MAIN BRANCH",
                ["BranchMobile"] = "Default",
                ["BranchEmail"] = "Default",
                ["BranchAddress"] = "Default",
                ["customer_name"] = "Afrique",
                ["customer_email"] = "",
                ["customer_mobile"] = "N/A",
                ["customer_address"] = "",
                ["customer_taxrefno"] = "",
                ["customer_prevBalance"] = "19,395.00",
                ["customer_currentBalance"] = "19,095.00",
                ["CompanyAddress"] = "Test Address",
                ["CompanyBusinessType"] = "RETAIL & WHOLESALE STORE",
                ["CompanyEmail"] = "test@gmail.com",
                ["CompanyMobile"] = "+2547000000001",
                ["CompanyName"] = "TEST COMPANY",
                ["CompanyTaxNo"] = "P000000001",
                ["ReportsDeliveryEmailAddresses"] = "",
                ["CompanyLogo"] = "logo.jpg",
                ["footer"] = "!Thank you and Come Again!"
            };


            //map
            var result = payment.Map(template, additionalParams);

            Assert.Equal(result, expectedResult);
        }

    }
}
