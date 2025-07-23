using System;
using System.Collections.Generic;

namespace ObjectSemantics.NET.Tests.MoqModels
{
    internal class Student
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string StudentName { get; set; }
        public double Balance { get; set; }
        public int Age { get; set; }
        public DateTime RegDate { get; set; } = DateTime.Now;
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public string[] ArrayOfString { get; set; } = new string[] { };
        public double[] ArrayOfDouble { get; set; } = new double[] { };
        public bool IsActive { get; set; }
        public List<StudentClockInDetail> StudentClockInDetails { get; set; } = new List<StudentClockInDetail>();
    }
    class StudentClockInDetail
    {
        public DateTime? LastClockedInDate { get; set; } = null;
        public long? LastClockedInPoints { get; set; } = null;
    }
}
