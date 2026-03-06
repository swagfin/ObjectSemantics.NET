using ObjectSemantics.NET.Tests.MoqModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class RealLifeTemplateScenarioTests
    {
        [Fact]
        public void Should_Render_Order_Confirmation_Email_With_Mixed_Features()
        {
            OrderEmailModel order = new OrderEmailModel
            {
                OrderNo = "ORD-1001",
                OrderDate = new DateTime(2026, 03, 06, 9, 30, 0),
                Subtotal = 2500,
                Tax = 400,
                Total = 2900,
                IsPaid = true,
                Customer = new OrderCustomer
                {
                    FullName = "jane doe",
                    BillingAddress = new OrderAddress
                    {
                        Line1 = "12 River Road",
                        City = "Nairobi",
                        Country = "Kenya"
                    }
                },
                Items = new List<OrderLineItem>
                {
                    new OrderLineItem { Name = "Keyboard", Quantity = 1, UnitPrice = 1200, LineTotal = 1200 },
                    new OrderLineItem { Name = "Mouse", Quantity = 2, UnitPrice = 650, LineTotal = 1300 }
                }
            };

            var extra = new Dictionary<string, object>
            {
                ["SupportEmail"] = "support@crudsoft.com"
            };

            string template = "Order {{ OrderNo }}|Date {{ OrderDate:yyyy-MM-dd }}|Customer {{ Customer.FullName:titlecase }}|{{ #if(IsPaid == true) }}PAID{{ #else }}UNPAID{{ #endif }}|Items {{ #foreach(Items) }}[{{ Quantity }}x{{ Name }}={{ LineTotal:N2 }}]{{ #endforeach }}|Ship {{ Customer.BillingAddress.City }},{{ Customer.BillingAddress.Country }}|Total {{ Total:N2 }}|Support {{ SupportEmail }}";

            string result = order.Map(template, extra);
            string expected = "Order ORD-1001|Date 2026-03-06|Customer Jane Doe|PAID|Items [1xKeyboard=1,200.00][2xMouse=1,300.00]|Ship Nairobi,Kenya|Total 2,900.00|Support support@crudsoft.com";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_Evaluate_Nested_If_Condition_For_Message_Routing()
        {
            OrderEmailModel order = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    BillingAddress = new OrderAddress
                    {
                        Country = "Kenya"
                    }
                }
            };

            string template = "{{ #if(Customer.BillingAddress.Country == Kenya) }}LOCAL{{ #else }}INTERNATIONAL{{ #endif }}";
            string result = order.Map(template);

            Assert.Equal("LOCAL", result);
        }

        [Fact]
        public void Should_Fallback_To_Else_When_Intermediate_Nested_Object_Is_Null()
        {
            OrderEmailModel order = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    BillingAddress = null
                }
            };

            string template = "{{ #if(Customer.BillingAddress.Country == Kenya) }}LOCAL{{ #else }}INTERNATIONAL{{ #endif }}";
            string result = order.Map(template);

            Assert.Equal("INTERNATIONAL", result);
        }

        [Fact]
        public void Should_Map_Additional_Parameters_With_Dot_Notation()
        {
            OrderEmailModel order = new OrderEmailModel
            {
                OrderNo = "ORD-9999"
            };

            var extra = new Dictionary<string, object>
            {
                ["Meta.UnsubscribeUrl"] = "https://example.com/unsub/abc123",
                ["Meta.Channel"] = "EMAIL"
            };

            string template = "Order {{ OrderNo }} via {{ Meta.Channel }}; Unsubscribe: {{ Meta.UnsubscribeUrl }}";
            string result = order.Map(template, extra);

            Assert.Equal("Order ORD-9999 via EMAIL; Unsubscribe: https://example.com/unsub/abc123", result);
        }

        [Fact]
        public void Should_Escape_Xml_In_Nested_And_Additional_Values_For_Email()
        {
            OrderEmailModel order = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    FullName = "Tom & Jerry <Ltd>"
                }
            };

            var extra = new Dictionary<string, object>
            {
                ["SupportLink"] = "<a href='https://example.com/help'>Help</a>"
            };

            string template = "{{ Customer.FullName }}|{{ SupportLink }}";
            string result = order.Map(template, extra, new TemplateMapperOptions
            {
                XmlCharEscaping = true
            });

            Assert.Equal("Tom &amp; Jerry &lt;Ltd&gt;|&lt;a href=&apos;https://example.com/help&apos;&gt;Help&lt;/a&gt;", result);
        }

        [Fact]
        public void Should_Remain_Deterministic_Across_Repeated_Message_Mapping()
        {
            string template = "Hello {{ Customer.FullName }} - {{ OrderNo }}";

            for (int i = 1; i <= 200; i++)
            {
                OrderEmailModel order = new OrderEmailModel
                {
                    OrderNo = "ORD-" + i.ToString("000", System.Globalization.CultureInfo.InvariantCulture),
                    Customer = new OrderCustomer
                    {
                        FullName = "Customer " + i.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    }
                };

                string result = order.Map(template);
                string expected = "Hello Customer " + i.ToString(System.Globalization.CultureInfo.InvariantCulture) + " - ORD-" + i.ToString("000", System.Globalization.CultureInfo.InvariantCulture);

                Assert.Equal(expected, result);
            }
        }
    }
}
