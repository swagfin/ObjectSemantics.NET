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
                },
                Narration = null,
                CustomerName = "JOHN DOE ENTERPRISES",
                LedgerAccountName = "Cash A/C"
            };


            //additional headers
            var additionalParams = new Dictionary<string, object>
            {
                ["BranchId"] = "1",
                ["BranchName"] = "MAIN BRANCH",
                ["BranchMobile"] = "Default",
                ["BranchEmail"] = "Default",
                ["BranchAddress"] = "Default",
                ["customer_name"] = "JOHN DOE",
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
