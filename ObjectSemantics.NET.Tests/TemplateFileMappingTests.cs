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
                ["BranchName"] = "MAIN BRANCH",
                ["CompanyName"] = "TEST COMPANY",
                ["CompanyEmail"] = "test@gmail.com",
                ["CompanyAddress"] = "Test Address",
                ["CompanyMobile"] = "+2547000000001",
                ["customer_prevBalance"] = "19,395.00",
                ["customer_currentBalance"] = "19,095.00",
                ["CompanyLogo"] = "logo.jpg",
            };

            //map
            string result = payment.Map(template, additionalParams);

            Assert.Equal(result, expectedResult);
        }

    }
}
