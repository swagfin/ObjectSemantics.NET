using BenchmarkDotNet.Attributes;
using ObjectSemantics.NET.Engine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ObjectSemantics.NET.Benchmarks
{
    [MemoryDiagnoser]
    public class RenderingBenchmarks
    {
        private readonly BenchmarkModel _model = new BenchmarkModel();
        private string _manyValues;
        private string[] _templates;
        private int _templateIndex;
        private CompiledTemplate _compiled;
        private readonly TemplateMapperOptions _optimized = new TemplateMapperOptions { LazyPropertyAccess = true, UseStreamingEvaluation = true };
        private const string ExpressionTemplate = "{{ #foreach(Items) }}{{ __calc(Quantity * Price):N2 }};{{ #endforeach }}";

        [GlobalSetup]
        public void Setup()
        {
            for (int i = 0; i < 100; i++)
                _model.Items.Add(new BenchmarkItem { Quantity = i + 1, Price = 12.5m });
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < 200; i++)
                text.Append("Hello {{ Name }}! ");
            _manyValues = text.ToString();
            _templates = new string[4096];
            for (int i = 0; i < _templates.Length; i++)
                _templates[i] = "Template " + i + " {{ Name }}";
            _model.Map(ExpressionTemplate);
            _model.Map(_manyValues);
            _compiled = TemplateMapper.Compile(_manyValues);
        }

        [Benchmark]
        public string WarmValues() { return _model.Map(_manyValues); }

        [Benchmark]
        public string ExpressionLoop() { return _model.Map(ExpressionTemplate); }

        [Benchmark]
        public string Aggregate() { return _model.Map("{{ __sum(Items.Price) }}|{{ __avg(Items.Price) }}"); }

        [Benchmark]
        public string CacheChurn() { return _model.Map(_templates[_templateIndex++ & 4095]); }

        [Benchmark]
        public object ColdParse() { return EngineTemplateParser.Parse(_manyValues); }

        [Benchmark]
        public string CompiledValues() { return _compiled.Render(_model); }

        [Benchmark]
        public void WriterValues() { _compiled.RenderTo(TextWriter.Null, _model); }

        [Benchmark]
        public string StreamingAggregate() { return _model.Map("{{ __sum(Items.Price) }}|{{ __avg(Items.Price) }}", options: _optimized); }

        [Benchmark(OperationsPerInvoke = 32)]
        public void ConcurrentRendering()
        {
            Parallel.For(0, 32, i => _model.Map(ExpressionTemplate));
        }
    }

    public class BenchmarkModel
    {
        public string Name { get; set; } = "Customer";
        public List<BenchmarkItem> Items { get; set; } = new List<BenchmarkItem>();
    }

    public class BenchmarkItem
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
