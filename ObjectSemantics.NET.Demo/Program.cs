using ObjectSemantics.NET.Logic;
using System;
using System.Collections.Generic;
using System.IO;

namespace ObjectSemantics.NET.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            IObjectSemantics objectSemantics = new ObjectSemanticsLogic(new ObjectSemanticsOptions
            {
                CreateTemplatesDirectoryIfNotExist = true,
                SupportedTemplateFileExtensions = new string[] { ".html" },
                TemplatesDirectory = Path.Combine(Environment.CurrentDirectory, "Samples")
            });

            List<ObjectSemanticsKeyValue> headers = new List<ObjectSemanticsKeyValue>
            {
                 new ObjectSemanticsKeyValue{ Key ="CompanyName",  Value= "CRUDSOFT TECHNOLOGIES" },
                 new ObjectSemanticsKeyValue{ Key ="CompanyEmail",  Value= "georgewainaina18@gmail.com" },
                 new ObjectSemanticsKeyValue{ Key ="CompanyEmployees",  Value= 1289 },
            };


            Student student = new Student
            {
                StudentName = "George",
                Balance = 2320,
                RegDate = DateTime.Now,
                Invoices = new List<Invoice>
                     {
                          new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate=DateTime.Now.Date.AddDays(-1) },
                          new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate=DateTime.Now.Date.AddDays(-2) }
                     }
            };


            //TESTING 
            //string htmlWithData = objectSemantics.GenerateTemplate(student, "record.html", headers);

            string htmlWithData = objectSemantics.GenerateTemplate(student, "recordWithChildren.html", headers);

            student.StudentName = "Gladwel Wambui";
            student.Balance = 0;
            student.Invoices = new List<Invoice>();
            htmlWithData = objectSemantics.GenerateTemplate(student, "recordWithChildren.html", headers);

            Console.WriteLine(htmlWithData);


            Console.ReadLine();
            Environment.Exit(0);
        }
    }
    class Student
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string StudentName { get; set; }
        public double Balance { get; set; }
        public DateTime RegDate { get; set; } = DateTime.Now;
        public List<Invoice> Invoices { get; set; }
    }

    class Invoice
    {
        public int Id { get; set; }
        public string RefNo { get; set; }
        public string Narration { get; set; }
        public double Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
}
