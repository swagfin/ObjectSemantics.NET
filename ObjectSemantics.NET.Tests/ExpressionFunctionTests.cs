using ObjectSemantics.NET.Tests.MoqModels;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class ExpressionFunctionTests
    {
        [Fact]
        public void Should_Sum_Collection_Path()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    Payments = new List<OrderPayment>
                    {
                        new OrderPayment { Amount = 1000 },
                        new OrderPayment { Amount = 2000.75m },
                        new OrderPayment { Amount = 999.25m }
                    }
                }
            };

            string result = model.Map("{{ __sum(Customer.Payments.Amount):N2 }}");
            Assert.Equal("4,000.00", result);
        }

        [Fact]
        public void Should_Average_Collection_Path_With_Single_Underscore()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    Payments = new List<OrderPayment>
                    {
                        new OrderPayment { PaidAmount = 300 },
                        new OrderPayment { PaidAmount = 900 },
                        new OrderPayment { PaidAmount = 1200 }
                    }
                }
            };

            string result = model.Map("{{ _avg(Customer.Payments.PaidAmount):N2 }}");
            Assert.Equal("800.00", result);
        }

        [Fact]
        public void Should_Calculate_Property_Expression()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                PaidAmount = 10000,
                Customer = new OrderCustomer
                {
                    CreditLimit = 4500
                }
            };

            string result = model.Map("{{ __calc(PaidAmount - Customer.CreditLimit):N2 }}");
            Assert.Equal("5,500.00", result);
        }

        [Fact]
        public void Should_Calculate_Expression_Including_Parentheses_And_Decimals()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                Subtotal = 1000,
                Tax = 160
            };

            string result = model.Map("{{ __calc((Subtotal + Tax) * 0.5):N2 }}");
            Assert.Equal("580.00", result);
        }

        [Fact]
        public void Should_Calculate_Count_Min_And_Max_Aggregates()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    Payments = new List<OrderPayment>
                    {
                        new OrderPayment { Amount = 700 },
                        new OrderPayment { Amount = 200 },
                        new OrderPayment { Amount = 1100 }
                    }
                }
            };

            string template = "{{ __count(Customer.Payments.Amount) }}|{{ __min(Customer.Payments.Amount):N2 }}|{{ __max(Customer.Payments.Amount):N2 }}";
            string result = model.Map(template);
            Assert.Equal("3|200.00|1,100.00", result);
        }

        [Fact]
        public void Should_Calculate_Per_Row_Expression_Inside_Loop()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                Items = new List<OrderLineItem>
                {
                    new OrderLineItem { Quantity = 2, UnitPrice = 350 },
                    new OrderLineItem { Quantity = 3, UnitPrice = 100.5m }
                }
            };

            string template = "{{ #foreach(Items) }}[{{ __calc(Quantity * UnitPrice):N2 }}]{{ #endforeach }}";
            string result = model.Map(template);

            Assert.Equal("[700.00][301.50]", result);
        }

        [Fact]
        public void Should_Render_Empty_When_Calc_Expression_Is_Invalid()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                PaidAmount = 5000,
                Customer = new OrderCustomer
                {
                    CreditLimit = 1000
                }
            };

            string result = model.Map("A{{ __calc(PaidAmount - UnknownValue):N2 }}B");
            Assert.Equal("AB", result);
        }

        [Fact]
        public void Should_Render_Empty_For_Unknown_Aggregate_Path()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    Payments = new List<OrderPayment>
                    {
                        new OrderPayment { Amount = 100 }
                    }
                }
            };

            string result = model.Map("{{ __sum(Customer.UnknownPayments.Amount) }}|{{ __avg(Customer.UnknownPayments.Amount) }}|{{ __count(Customer.UnknownPayments.Amount) }}|{{ __min(Customer.UnknownPayments.Amount) }}|{{ __max(Customer.UnknownPayments.Amount) }}");
            Assert.Equal("||||", result);
        }

        [Fact]
        public void Should_Return_Zero_For_Aggregates_When_Source_Is_Null()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                Customer = null
            };

            string result = model.Map("{{ __sum(Customer.Payments.Amount) }}|{{ __avg(Customer.Payments.Amount) }}|{{ __count(Customer.Payments.Amount) }}|{{ __min(Customer.Payments.Amount) }}|{{ __max(Customer.Payments.Amount) }}");
            Assert.Equal("0|0|0|0|0", result);
        }

        [Fact]
        public void Should_Return_Zero_When_Calc_Uses_Null_Operand_Path()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                PaidAmount = 5000,
                Customer = null
            };

            string result = model.Map("A{{ __calc(PaidAmount - Customer.CreditLimit) }}B");
            Assert.Equal("A0B", result);
        }

        [Fact]
        public void Should_Support_Expressions_Without_Number_Format()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                PaidAmount = 10000,
                Customer = new OrderCustomer
                {
                    CreditLimit = 4500,
                    Payments = new List<OrderPayment>
                    {
                        new OrderPayment { Amount = 1000 },
                        new OrderPayment { Amount = 2000 },
                        new OrderPayment { Amount = 1500 }
                    }
                }
            };

            string result = model.Map("{{ __sum(Customer.Payments.Amount) }}|{{ __calc(PaidAmount - Customer.CreditLimit) }}");
            Assert.Equal("4500|5500", result);
        }

        [Fact]
        public void Should_Render_Empty_When_Sum_Path_Is_String_Based()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                Items = new List<OrderLineItem>
                {
                    new OrderLineItem { Name = "Keyboard" },
                    new OrderLineItem { Name = "Mouse" }
                }
            };

            string result = model.Map("X{{ __sum(Items.Name) }}Y");
            Assert.Equal("XY", result);
        }

        [Fact]
        public void Should_Render_Empty_When_Sum_Path_Is_Date_Based()
        {
            OrderEmailModel model = new OrderEmailModel
            {
                OrderDate = new System.DateTime(2026, 03, 07)
            };

            string result = model.Map("X{{ __sum(OrderDate) }}Y");
            Assert.Equal("XY", result);
        }
    }
}
