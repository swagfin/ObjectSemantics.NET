using ObjectSemantics.NET.Tests.MoqModels;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class PromotionAndMessagingTemplateTests
    {
        [Theory]
        [InlineData("A", "Special offer for Jane Doe")]
        [InlineData("B", "Do not miss out Jane Doe")]
        public void Should_Render_Ab_Test_Subject_Line(string campaignVariant, string expected)
        {
            OrderEmailModel order = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    FullName = "jane doe"
                }
            };

            var extra = new Dictionary<string, object>
            {
                ["CampaignVariant"] = campaignVariant
            };

            string template = "{{ #if(CampaignVariant == A) }}Special offer for {{ Customer.FullName:titlecase }}{{ #else }}Do not miss out {{ Customer.FullName:titlecase }}{{ #endif }}";
            string result = order.Map(template, extra);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_Use_Generic_Greeting_When_Personalized_Name_Is_Null()
        {
            OrderEmailModel order = new OrderEmailModel
            {
                Customer = new OrderCustomer
                {
                    FullName = null
                }
            };

            string template = "{{ #if(Customer.FullName == null) }}Hello Customer{{ #else }}Hello {{ Customer.FullName:titlecase }}{{ #endif }}";
            string result = order.Map(template);

            Assert.Equal("Hello Customer", result);
        }

        [Theory]
        [InlineData("EMAIL", "Compliance: Unsubscribe via https://example.com/unsub/u-100")]
        [InlineData("SMS", "Compliance: Reply STOP to unsubscribe")]
        public void Should_Render_Channel_Specific_Compliance_Block(string channel, string expected)
        {
            OrderEmailModel order = new OrderEmailModel();
            var extra = new Dictionary<string, object>
            {
                ["Meta.Channel"] = channel,
                ["Meta.UnsubscribeUrl"] = "https://example.com/unsub/u-100"
            };

            string template = "Compliance: {{ #if(Meta.Channel == EMAIL) }}Unsubscribe via {{ Meta.UnsubscribeUrl }}{{ #else }}Reply STOP to unsubscribe{{ #endif }}";
            string result = order.Map(template, extra);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_Map_Bulk_Message_Batch_Deterministically()
        {
            string template = "{{ #if(CampaignVariant == A) }}[A]{{ #else }}[B]{{ #endif }} {{ #if(Customer.FullName == null) }}Hello Customer{{ #else }}Hello {{ Customer.FullName:titlecase }}{{ #endif }} - {{ OrderNo }}";

            for (int i = 1; i <= 250; i++)
            {
                OrderEmailModel order = new OrderEmailModel
                {
                    OrderNo = "ORD-" + i.ToString("000", CultureInfo.InvariantCulture),
                    Customer = new OrderCustomer
                    {
                        FullName = i % 5 == 0 ? null : "customer " + i.ToString(CultureInfo.InvariantCulture)
                    }
                };

                var extra = new Dictionary<string, object>
                {
                    ["CampaignVariant"] = i % 2 == 0 ? "A" : "B"
                };

                string result = order.Map(template, extra);
                string expectedPrefix = i % 2 == 0 ? "[A]" : "[B]";
                string expectedName = i % 5 == 0 ? "Hello Customer" : "Hello Customer " + i.ToString(CultureInfo.InvariantCulture);
                string expected = expectedPrefix + " " + expectedName + " - ORD-" + i.ToString("000", CultureInfo.InvariantCulture);

                Assert.Equal(expected, result);
                Assert.DoesNotContain("{{", result);
            }
        }
    }
}
