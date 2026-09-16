using ObjectSemantics.NET.Engine;
using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class RenderingCompatibilityTests
    {
        [Theory]
        [InlineData("{{ missing }}", "{{ missing }}")]
        [InlineData("{{ NAME }}", "Alice")]
        [InlineData("{{ Items. .Count }}", "0")]
        [InlineData("{{ __calc(10 - Missing) }}", "")]
        [InlineData("{{ __calc(10 - Optional) }}", "0")]
        [InlineData("{{ __calc(-2 * (3 + 4)) }}", "-14")]
        [InlineData("{{ __calc(--2 - -3) }}", "5")]
        [InlineData("{{ __calc(3 / -2) }}", "-1.5")]
        [InlineData("{{ __calc(1 / 0) }}", "")]
        [InlineData("{{ __sum(Items.Value) }}", "")]
        [InlineData("{{ #foreach(Items) }}{{ Missing }}{{ #endforeach }}", "")]
        public void PreservesExistingOutputRules(string template, string expected)
        {
            Assert.Equal(expected, new Model().Map(template));
        }

        [Fact]
        public void PreservesMissingLoopPropertyAndDuplicateParameterRules()
        {
            Model model = new Model { Items = new List<Item> { new Item() } };
            Assert.Equal("Missing", model.Map("{{ #foreach(Items) }}{{ Missing }}{{ #endforeach }}"));
            Assert.Throws<ArgumentException>(() => model.Map("{{ Name }}", new Dictionary<string, object> { ["name"] = "Override" }));
        }

        [Fact]
        public void ConcurrentRendersDoNotShareValues()
        {
            Parallel.For(0, 500, i =>
            {
                Model model = new Model { Name = i.ToString(), Optional = i };
                Assert.Equal(i + "|" + (i * 2), model.Map("{{ Name }}|{{ __calc(Optional * 2) }}"));
            });
        }

        [Fact]
        public void CacheInitializesOnceAndEvictsIncrementally()
        {
            EngineTemplateCache.CacheState cache = new EngineTemplateCache.CacheState(2, 100);
            int calls = 0;
            Func<string, EngineRunnerTemplate> factory = text =>
            {
                Interlocked.Increment(ref calls);
                return new EngineRunnerTemplate { Template = text };
            };
            Parallel.For(0, 100, i => cache.GetOrAdd("first", factory));
            Assert.Equal(1, calls);
            EngineRunnerTemplate second = cache.GetOrAdd("second", factory);
            cache.GetOrAdd("third", factory);
            Assert.Same(second, cache.GetOrAdd("second", factory));
            cache.GetOrAdd("first", factory);
            Assert.Equal(4, calls);
        }

        [Fact]
        public void OversizedTemplatesBypassCache()
        {
            EngineTemplateCache.CacheState cache = new EngineTemplateCache.CacheState(2, 3);
            EngineRunnerTemplate first = cache.GetOrAdd("large", EngineTemplateParser.Parse);
            Assert.NotSame(first, cache.GetOrAdd("large", EngineTemplateParser.Parse));
        }

        [Fact]
        public void SourceBudgetEvictsOnlyAsMuchAsNeeded()
        {
            EngineTemplateCache.CacheState cache = new EngineTemplateCache.CacheState(10, 6);
            EngineRunnerTemplate first = cache.GetOrAdd("aaa", EngineTemplateParser.Parse);
            EngineRunnerTemplate second = cache.GetOrAdd("bb", EngineTemplateParser.Parse);
            cache.GetOrAdd("cc", EngineTemplateParser.Parse);
            Assert.Same(second, cache.GetOrAdd("bb", EngineTemplateParser.Parse));
            Assert.NotSame(first, cache.GetOrAdd("aaa", EngineTemplateParser.Parse));
        }

        public class Model
        {
            public string Name { get; set; } = "Alice";
            public int? Optional { get; set; }
            public List<Item> Items { get; set; } = new List<Item>();
        }

        public class Item
        {
            public decimal Value { get; set; }
        }
    }
}
