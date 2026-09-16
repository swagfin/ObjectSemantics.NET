namespace ObjectSemantics.NET
{
    public class TemplateDiagnostic
    {
        public string Message { get; set; }
        public int Position { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}
