using ObjectSemantics.NET.Logic;
using ObjectSemantics.NET.Tests.MoqModels;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class ObjectSemanticsTests
    {
        private readonly IObjectSemantics _logicService;
        public ObjectSemanticsTests()
        {
            //Create Single Instance Service
            this._logicService = new ObjectSemanticsLogic(new ObjectSemanticsOptions
            {
                CreateTemplatesDirectoryIfNotExist = true,
                TemplatesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MoqFiles")
            });

        }



        [Fact]
        public void Should_Map_Object_To_Template_From_TemplateObject()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "George Waynne",
                Balance = 2510
            };
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"My Name is: {{ StudentName }}"
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, template);
            string expectedString = "My Name is: George Waynne";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Map_Object_To_Template_From_TemplateFile()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "George Waynne",
                Balance = 2510
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, $"template-example.txt");
            string expectedString = "My Name is: George Waynne and my balance is: 2510";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Throw_Exception_If_Template_File_Is_Missing()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "George Waynne",
                Balance = 2510
            };
            Exception ex = Assert.Throws<Exception>(() => _logicService.GenerateTemplate(student, $"{Guid.NewGuid()}.txt"));
            Assert.Contains("Template doesn't seem to exist in directory:", ex.Message);
        }



        //Loop Object Tests
        [Fact]
        public void Should_Map_Enumerable_Collection_In_Object()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate=DateTime.Now.Date.AddDays(-1) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate=DateTime.Now.Date.AddDays(-2) }
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ StudentName }} Invoices
{{ for-each-start:invoices  }}
<tr>
    <td>{{ Id }}</td>
    <td>{{ RefNo }}</td>
    <td>{{ Narration }}</td>
    <td>{{ Amount:N0 }}</td>
    <td>{{ InvoiceDate:yyyy-MM-dd }}</td>
</tr>
{{ for-each-end:invoices }}"
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, template);
            string expectedResult = "John Doe Invoices" +
                "\r\n<tr>" +
                "\r\n    <td>2</td>" +
                "\r\n    <td>INV_002</td>" +
                "\r\n    <td>Grade II Fees Invoice</td>" +
                "\r\n    <td>2,000</td>" +
                "\r\n    <td>2022-11-26</td>" +
                "\r\n</tr>" +
                "\r\n<tr>" +
                "\r\n    <td>1</td>" +
                "\r\n    <td>INV_001</td>" +
                "\r\n    <td>Grade I Fees Invoice</td>" +
                "\r\n    <td>320</td>" +
                "\r\n    <td>2022-11-25</td>" +
                "\r\n</tr>" +
                "\r\n";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }



        [Fact]
        public void Should_Map_Additional_Parameters()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "George Waynne"
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"My Name is: {{ StudentName }} | CompanyName: {{ CompanyName }} | CompanyEmail: {{ CompanyEmail }} | Employees: {{ Employees }}"
            };
            //Additional Parameters
            List<ObjectSemanticsKeyValue> additionalParams = new List<ObjectSemanticsKeyValue>
            {
                 new ObjectSemanticsKeyValue{ Key ="CompanyName",  Value= "TEST INC." },
                 new ObjectSemanticsKeyValue{ Key ="CompanyEmail",  Value= "test.inc@test.com" },
                 new ObjectSemanticsKeyValue{ Key ="Employees",  Value= 1289 },
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, template, additionalParams);
            string expectedString = "My Name is: George Waynne | CompanyName: TEST INC. | CompanyEmail: test.inc@test.com | Employees: 1289";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Return_Unknown_Properties_If_Not_Found_In_Object()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "George Waynne"
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Unknown Object example: {{StudentIdentityCardXyx}}"
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, template);
            string expectedString = "Unknown Object example: {{StudentIdentityCardXyx}}";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }



        [Fact]
        public void Should_Ignore_Whitespaces_Inside_CurlyBrackets()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "George Waynne"
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"StudentName is: {{            StudentName       }}"
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, template);
            string expectedString = "StudentName is: George Waynne";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }



        [Fact]
        public void Should_Accept_String_To_String_Formatting()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "WILLiaM"
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original StudentName: {{ StudentName }}, Uppercase StudentName: {{ StudentName:uppercase }}, Lowercase StudentName: {{ StudentName:lowercase }}"
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, template);
            string expectedString = "Original StudentName: WILLiaM, Uppercase StudentName: WILLIAM, Lowercase StudentName: william";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Accept_Number_To_String_Formatting()
        {
            //Create Model
            Student student = new Student
            {
                Balance = 20000.5788
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original Balance: {{ Balance }}, #,##0 Balance: {{ Balance:#,##0 }}, N5 Balance: {{ Balance:N5 }}"
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, template);
            string expectedString = "Original Balance: 20000.5788, #,##0 Balance: 20,001, N5 Balance: 20,000.57880";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Accept_DateTime_To_String_Formatting()
        {
            //Lets see how it can handle multiple : {{ RegDate:yyyy-MM-dd HH:mm tt }}
            Student student = new Student
            {
                RegDate = new DateTime(2022, 11, 27, 18, 13, 59)
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original RegDate: {{ RegDate }}, yyyy RegDate: {{ RegDate:yyyy }}, yyyy-MM-dd HH:mm tt RegDate: {{ RegDate:yyyy-MM-dd HH:mm  tt }}"
            };
            string generatedTemplate = _logicService.GenerateTemplate(student, template);
            string expectedString = "Original RegDate: 11/27/2022 6:13:59 PM, yyyy RegDate: 2022, yyyy-MM-dd HH:mm tt RegDate: 2022-11-27 18:13 PM";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

    }
}
