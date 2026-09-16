using System;
using System.Threading;

namespace ObjectSemantics.NET
{
    public class TemplateMapperOptions
    {
        /// <summary>Read only requested top-level properties, once per scope. Opt in for models with pure getters.</summary>
        public bool LazyPropertyAccess { get; set; }
        /// <summary>Traverse aggregate paths without intermediate lists. Getter traversal becomes depth-first.</summary>
        public bool UseStreamingEvaluation { get; set; }
        /// <summary>Throw for syntax diagnostics, missing values, and invalid expressions.</summary>
        public bool StrictMode { get; set; }
        /// <summary>Maximum block or recursive collection traversal depth. Zero disables the limit.</summary>
        public int MaximumNestingDepth { get; set; } = 128;
        /// <summary>Maximum source length in UTF-16 characters. Zero disables the limit.</summary>
        public int MaximumTemplateCharacters { get; set; }
        /// <summary>Total enumerated items across loops, conditions, and expressions. Zero disables the limit.</summary>
        public long MaximumIterations { get; set; }
        /// <summary>Maximum output length in UTF-16 characters. Zero disables the limit.</summary>
        public long MaximumOutputCharacters { get; set; }
        public CancellationToken CancellationToken { get; set; }
        /// <summary>Escape XML special characters in formatted property values.</summary>
        public bool XmlCharEscaping { get; set; }

        internal TemplateMapperOptions Snapshot()
        {
            if (MaximumNestingDepth < 0) throw new ArgumentOutOfRangeException(nameof(MaximumNestingDepth));
            if (MaximumTemplateCharacters < 0) throw new ArgumentOutOfRangeException(nameof(MaximumTemplateCharacters));
            if (MaximumIterations < 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
            if (MaximumOutputCharacters < 0) throw new ArgumentOutOfRangeException(nameof(MaximumOutputCharacters));
            return (TemplateMapperOptions)MemberwiseClone();
        }
    }
}
