using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class OptimizedEvaluationTests
    {
        [Fact]
        public void LazyAccessSkipsUnusedGettersAndReadsOncePerScope()
        {
            GetterModel model = new GetterModel();
            Assert.Throws<InvalidOperationException>(() => model.Map("{{ Name }}"));
            model.Reads = 0;
            Assert.Equal("Alice|Alice|Alice", model.Map("{{ Name }}|{{ Name }}|{{ #if(Name == Alice) }}{{ Name }}{{ #endif }}", options: new TemplateMapperOptions { LazyPropertyAccess = true }));
            Assert.Equal(1, model.Reads);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AggregatesPreserveNullEmptyAndInvalidBehavior(bool streaming)
        {
            Model model = new Model { Items = new List<Item> { new Item { Value = 2 }, new Item { Value = null }, new Item { Value = 4 } } };
            TemplateMapperOptions options = new TemplateMapperOptions { UseStreamingEvaluation = streaming };
            string template = "{{ __sum(Items.Value) }}|{{ __avg(Items.Value) }}|{{ __count(Items.Value) }}|{{ __min(Items.Value) }}|{{ __max(Items.Value) }}";
            Assert.Equal("6|3|2|2|4", model.Map(template, options: options));
            model.Items.Clear();
            Assert.Equal("||||", model.Map(template, options: options));
            model.Items = null;
            Assert.Equal("0|0|0|0|0", model.Map(template, options: options));
            Assert.Equal("", model.Map("{{ __sum(Unknown.Value) }}", options: options));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AggregateEnumerationHonorsLimits(bool streaming)
        {
            Model model = new Model { Items = new List<Item> { new Item(), new Item() } };
            Assert.Throws<TemplateLimitExceededException>(() => model.Map("{{ __sum(Items.Value) }}", options: new TemplateMapperOptions { UseStreamingEvaluation = streaming, MaximumIterations = 1 }));
        }

        [Fact]
        public void TypedFormattingPreservesDatePrecisionAndKind()
        {
            Model model = new Model { Date = new DateTime(2026, 9, 16, 12, 34, 56, DateTimeKind.Utc).AddTicks(1234567) };
            Assert.Equal(model.Date.ToString("O", CultureInfo.InvariantCulture), model.Map("{{ Date:O }}"));
        }

        [Fact]
        public void ConditionsHandleNullableValuesAndExactDecimals()
        {
            Model model = new Model { Flag = true, Number = 20, Amount = 9007199254740993m };
            Assert.Equal("Y", model.Map("{{ #if(Flag == true) }}Y{{ #else }}N{{ #endif }}"));
            Assert.Equal("Y", model.Map("{{ #if(Number >= 18) }}Y{{ #else }}N{{ #endif }}"));
            Assert.Equal("N", model.Map("{{ #if(Amount == 9007199254740992) }}Y{{ #else }}N{{ #endif }}"));
            model.Flag = null;
            Assert.Equal("Y", model.Map("{{ #if(Flag == null) }}Y{{ #else }}N{{ #endif }}"));
            Assert.Equal("Y", model.Map("{{ #if(Numbers > 0) }}Y{{ #else }}N{{ #endif }}"));
        }

        public class GetterModel
        {
            public int Reads { get; set; }
            public string Name { get { Reads++; return "Alice"; } }
            public string Unused { get { throw new InvalidOperationException("Unused getter"); } }
        }

        public class Model
        {
            public List<Item> Items { get; set; }
            public DateTime Date { get; set; }
            public bool? Flag { get; set; }
            public int? Number { get; set; }
            public decimal Amount { get; set; }
            public int[] Numbers { get; set; } = new[] { 1, 2 };
        }

        public class Item
        {
            public decimal? Value { get; set; }
        }
    }
}
