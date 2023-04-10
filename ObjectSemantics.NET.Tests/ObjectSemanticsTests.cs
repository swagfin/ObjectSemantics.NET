using ObjectSemantics.NET.Tests.MoqModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class ObjectSemanticsTests
    {

        #region Basic Functions Tests


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
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedString = "My Name is: George Waynne";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
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
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
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
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = "John Doe Invoices" +
                "\r\n<tr>" +
                "\r\n    <td>2</td>" +
                "\r\n    <td>INV_002</td>" +
                "\r\n    <td>Grade II Fees Invoice</td>" +
                "\r\n    <td>2,000</td>" +
                "\r\n    <td>2023-04-01</td>" +
                "\r\n</tr>" +
                "\r\n<tr>" +
                "\r\n    <td>1</td>" +
                "\r\n    <td>INV_001</td>" +
                "\r\n    <td>Grade I Fees Invoice</td>" +
                "\r\n    <td>320</td>" +
                "\r\n    <td>2022-08-01</td>" +
                "\r\n</tr>";
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
            string generatedTemplate = TemplateMapper.Map(student, template, additionalParams);
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
            string generatedTemplate = TemplateMapper.Map(student, template);
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
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedString = "StudentName is: George Waynne";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        #endregion


        #region Text and Data Formatting Tests
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
            string generatedTemplate = TemplateMapper.Map(student, template);
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
            string generatedTemplate = TemplateMapper.Map(student, template);
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
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedString = "Original RegDate: 11/27/2022 6:13:59 PM, yyyy RegDate: 2022, yyyy-MM-dd HH:mm tt RegDate: 2022-11-27 18:13 PM";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }


        #endregion


        #region IF Conditional Tests

        [Fact]
        public void Should_Act_On_IfCondition_SingleLine()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "IsEnabled: {{ if-start:invoices(!=null) }}YES{{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = "IsEnabled: YES";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Act_On_IfCondition_SingleLine_With_Attribute()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "InvoicedPerson: {{ if-start:invoices(!=null) }}{{StudentName}}{{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = "InvoicedPerson: John Doe";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Act_On_IfCondition_MultiLine()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"status
{{ if-start:invoices(!=null) }}
<h4>condition--passed</h4>
{{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = "status\r\n\r\n<h4>condition--passed</h4>\r\n";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Act_On_IfCondition_MultiLine_With_Attribute()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"status
{{ if-start:invoices(!=null) }}
<h4>Hi, I have invoices for {{ StudentName }} </h4>
{{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = "status\r\n\r\n<h4>Hi, I have invoices for John Doe </h4>\r\n";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Act_On_IfCondition_Having_Loop_As_Child()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"
{{ if-start:invoices(!=null) }}
{{ StudentName }} Invoices
{{ for-each-start:invoices  }}
<tr>
    <td>{{ Id }}</td>
    <td>{{ RefNo }}</td>
</tr>
{{ for-each-end:invoices }}
{{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = "\r\n\r\nJohn Doe Invoices" +
                "\r\n<tr>" +
                "\r\n    <td>2</td>" +
                "\r\n    <td>INV_002</td>" +
                "\r\n</tr>" +
                "\r\n<tr>" +
                "\r\n    <td>1</td>" +
                "\r\n    <td>INV_001</td>" +
                "\r\n</tr>\r\n"; ;
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        //#Match =, !=, >, >=, <, and <=.

        [Theory]
        [InlineData("=", 5000)]
        [InlineData("!=", 0)]
        [InlineData(">", 2000)]
        [InlineData("<=", 5001)]
        [InlineData("<", 5001)]
        public void Should_Act_On_IfCondition_Equality_Checks(string condition, double amount)
        {
            //Create Model
            Student student = new Student { Balance = 5000 };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = string.Format("{2} if-start:balance({0}{1}) {3} {0} passed {2} if-end:balance {3}", condition, amount, "{{", "}}")
            };

            string expectedResult = string.Format(" {0} passed ", condition);
            string generatedTemplate = TemplateMapper.Map(student, template);
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Act_On_IfCondition_Simple_Property_String_Equality()
        {
            //Create Model
            Student student = new Student { StudentName = "John Doe" };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "{{ if-start:studentName(=John Doe) }} YES, i am John Doe {{ if-end:studentName }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " YES, i am John Doe ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Act_On_IfCondition_IEnumerable_Tests_Equall()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "{{ if-start:invoices(=2) }} 2 records {{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " 2 records ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Act_On_IfCondition_IEnumerable_Tests_NotEqualNull()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "{{ if-start:invoices(!=null) }} is not NULL {{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " is not NULL ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Act_On_IfCondition_IEnumerable_Tests_NULL_Object_Behaviour()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = null
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "{{ if-start:invoices(=null) }} is NULL {{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " is NULL ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }
        [Fact]
        public void Should_Act_On_IfCondition_IEnumerable_Tests_Count_NULL_Object_Behaviour()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = null
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "{{ if-start:invoices(=0) }} no records {{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " no records ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Act_On_IfCondition_IEnumerable_Tests_Count_Object_Behaviour()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>()
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "{{ if-start:invoices(=null) }} no records {{ if-end:invoices }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " no records ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        #endregion
    }
}
