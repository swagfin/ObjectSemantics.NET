﻿using ObjectSemantics.NET.Logic;
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
                ReserveTemplatesInMemory = true,
                SupportedTemplateFileExtensions = new string[] { ".html" },
                TemplatesDirectory = Path.Combine(Environment.CurrentDirectory, "Samples")
            });


            var newStudent = new Student
            {
                StudentName = "Steve",
                Balance = 1200,
                RegDate = DateTime.Now,
                Invoices = null
            };

            string singleRecordHtml = objectSemantics.GenerateTemplate(newStudent, "record.html");

            //Proceed
            List<Student> students = new List<Student>
              {
                new Student
                {
                    StudentName="George",
                    Balance= 2320,
                    RegDate= DateTime.Now,
                    Invoices = new List<Invoice>
                     {
                          new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate=DateTime.Now.Date.AddDays(-1) },
                          new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate=DateTime.Now.Date.AddDays(-2) }
                     }
                },
                new Student
                {
                    StudentName="Steve",
                    Balance= 1200,
                    RegDate= DateTime.Now,
                    Invoices= null
                },
              };

            string htmlWithData = objectSemantics.GenerateTemplate(students, "list.html");
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
