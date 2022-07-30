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
                ReserveTemplatesInMemory = true,
                SupportedTemplateFileExtensions = new string[] { ".html" },
                TemplatesDirectory = Path.Combine(Environment.CurrentDirectory, "Samples")
            });

            List<Student> students = new List<Student>
            {
                new Student{ StudentName="George", Balance= 2320, RegDate= DateTime.Now },
                new Student{ StudentName="Steve", Balance= 1200, RegDate= DateTime.Now },
            };

            string htmlWithData = objectSemantics.GenerateTemplate(students, "report.html");
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
    }
}
