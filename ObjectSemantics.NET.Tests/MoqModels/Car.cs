using System;

namespace ObjectSemantics.NET.Tests.MoqModels
{
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public int Year { get; set; }
        public double EngineSize { get; set; }
        public float FuelEfficiency { get; set; }
        public decimal Price { get; set; }
        public bool IsElectric { get; set; }
        public bool? IsLiked { get; set; } = null;
        public DateTime ManufactureDate { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public Guid UniqueId { get; set; }
        public char Rating { get; set; }
        public byte SafetyScore { get; set; }
        public long Mileage { get; set; }
        public short NumberOfDoors { get; set; }
    }
}
