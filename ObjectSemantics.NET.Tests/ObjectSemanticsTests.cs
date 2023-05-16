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
{{ #foreach(invoices)  }}
<tr>
    <td>{{ Id }}</td>
    <td>{{ RefNo }}</td>
    <td>{{ Narration }}</td>
    <td>{{ Amount:N0 }}</td>
    <td>{{ InvoiceDate:yyyy-MM-dd }}</td>
</tr>
{{ #endforeach }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = @"John Doe Invoices

<tr>
    <td>2</td>
    <td>INV_002</td>
    <td>Grade II Fees Invoice</td>
    <td>2,000</td>
    <td>2023-04-01</td>
</tr>

<tr>
    <td>1</td>
    <td>INV_001</td>
    <td>Grade I Fees Invoice</td>
    <td>320</td>
    <td>2022-08-01</td>
</tr>";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Map_Enumerable_Collection_SingleLine_Test()
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
                FileContents = @"{{ #foreach(invoices)  }} [{{RefNo}}] {{ #endforeach }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " [INV_002] [INV_001] ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Map_Multiple_Enumerable_Collection_On_Same_Template()
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
{{ StudentName }} Invoices
LOOP #1
{{ #foreach(invoices)  }}
    <h5>{{ Id }} On Loop #1</h5>
{{ #endforeach }}
LOOP #2
{{ #foreach(invoices)  }}
    <h5>{{ Id }} On Loop #2</h5>
{{ #endforeach }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = @"
John Doe Invoices
LOOP #1

    <h5>2 On Loop #1</h5>

    <h5>1 On Loop #1</h5>
LOOP #2

    <h5>2 On Loop #2</h5>

    <h5>1 On Loop #2</h5>";
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
            string expectedString = "Unknown Object example: {{ StudentIdentityCardXyx }}";
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
                FileContents = "IsEnabled: {{ #if(invoices!=null) }}YES{{ #endif }}"
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
                FileContents = "InvoicedPerson: {{ #if(invoices!=null) }}{{StudentName}}{{ #endif }}"
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
{{ #if(invoices!=null) }}
<h4>condition--passed</h4>
{{ #endif }}"
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
{{ #if(invoices!=null) }}
<h4>Hi, I have invoices for {{ StudentName }} </h4>
{{ #endif }}"
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
{{ #if (invoices != null) }}
{{ StudentName }} Invoices
{{ #foreach(invoices)  }}
<tr>
    <td>{{ Id }}</td>
    <td>{{ RefNo }}</td>
</tr>
{{ #endforeach }}
{{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = @"

John Doe Invoices

<tr>
    <td>2</td>
    <td>INV_002</td>
</tr>

<tr>
    <td>1</td>
    <td>INV_001</td>
</tr>
";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        //#Match =, !=, >, >=, <, and <=.

        [Theory]
        [InlineData("==", 5000)]
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
                FileContents = string.Format("{2} #if(balance{0}{1}) {3} {0} passed {2} #endif {3}", condition, amount, "{{", "}}")
            };

            string expectedResult = string.Format(" {0} passed ", condition);
            string generatedTemplate = TemplateMapper.Map(student, template);
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
                FileContents = "{{ #if(invoices==2) }} 2 records {{ #endif }}"
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
                FileContents = "{{ #if(invoices!=null) }} is not NULL {{ #endif }}"
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
                FileContents = "{{ #if(invoices==null) }} is NULL {{ #endif }}"
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
                FileContents = "{{ #if(invoices==0) }} no records {{ #endif }}"
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
                FileContents = "{{ #if(invoices==null) }} no records {{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " no records ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }



        [Theory]
        [InlineData(5000)]
        [InlineData(2000)]
        public void Should_Act_On_IfCondition_Having_ElseIf_Inline(double amount)
        {
            //Create Model
            Student student = new Student { Balance = amount };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "{{ #if(Balance==5000) }} --ok-passed-- {{ #else }} --error-failed-- {{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = (amount == 5000) ? " --ok-passed-- " : " --error-failed-- ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        [Theory]
        [InlineData(5000)]
        [InlineData(2000)]
        public void Should_Act_On_IfCondition_Having_ElseIf_MultiLine(double amount)
        {
            //Create Model
            Student student = new Student { Balance = amount };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"
{{ #if(Balance==5000) }}
--ok-passed--
{{ #else }}
--error-failed--
{{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = (amount == 5000) ? "\r\n\r\n--ok-passed--\r\n" : "\r\n\r\n--error-failed--\r\n";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("John Doe")]
        [InlineData("")]
        public void Should_Act_On_IfCondition_Having_ElseIf_MultiLine_String_EquallsNull(string studentName)
        {
            //Create Model
            Student student = new Student { StudentName = studentName };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"
{{ #if(StudentName==NULL) }}
--ok-passed--
{{ #else }}
--error-failed--
{{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = string.IsNullOrEmpty(studentName) ? "\r\n\r\n--ok-passed--\r\n" : "\r\n\r\n--error-failed--\r\n";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("John Doe")]
        [InlineData("")]
        public void Should_Act_On_IfCondition_Having_ElseIf_MultiLine_String_NotEquallsNull(string studentName)
        {
            //Create Model
            Student student = new Student { StudentName = studentName };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"
{{ #if(StudentName!=NULL) }}
--ok-passed--
{{ #else }}
--error-failed--
{{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = (!string.IsNullOrEmpty(studentName)) ? "\r\n\r\n--ok-passed--\r\n" : "\r\n\r\n--error-failed--\r\n";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("John Doe")]
        [InlineData("")]
        public void Should_Act_On_IfCondition_Having_ElseIf_MultiLine_String_Equalls(string studentName)
        {
            //Create Model
            Student student = new Student { StudentName = studentName };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"
{{ #if(StudentName==John Doe) }}
--ok-passed--
{{ #else }}
--error-failed--
{{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = (studentName == "John Doe") ? "\r\n\r\n--ok-passed--\r\n" : "\r\n\r\n--error-failed--\r\n";
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
                FileContents = "{{ #if(studentName==John Doe) }} YES, i am John Doe {{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " YES, i am John Doe ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Act_On_IfCondition_Having_ElseIf_Having_A_LoopBlock(bool populateInvoices)
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = (populateInvoices)
                           ? new List<Invoice>
                                {
                                    new Invoice { Id = 2, RefNo = "INV_002", Narration = "Grade II Fees Invoice", Amount = 2000, InvoiceDate = new DateTime(2023, 04, 01) },
                                    new Invoice { Id = 1, RefNo = "INV_001", Narration = "Grade I Fees Invoice", Amount = 320, InvoiceDate = new DateTime(2022, 08, 01) }
                                }
                           : null
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"
{{ #if(invoices==null) }}
-- no invoices found --
{{ #else }}
{{ #foreach(invoices)  }}
<tr>
    <td>{{ Id }}</td>
    <td>{{ RefNo }}</td>
</tr>
{{ #endforeach }}
{{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = (populateInvoices) ? @"


<tr>
    <td>2</td>
    <td>INV_002</td>
</tr>

<tr>
    <td>1</td>
    <td>INV_001</td>
</tr>
"

: @"

-- no invoices found --
";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }




        [Fact]
        public void Should_Act_On_IfCondition_Having_Multiple_IF_Condition_Blocks_SingleLine()
        {
            //Create Model
            Student student = new Student { StudentName = "John Doe", Balance = 2000 };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = "{{ #if(studentName==John Doe) }} YES, i am John Doe {{ #endif }} | {{ #if(Balance==2000) }} YES, my balance is 2000 {{ #endif }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = " YES, i am John Doe |  YES, my balance is 2000 ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("John Doe")]
        [InlineData("")]
        public void Should_Act_On_IfCondition_Having_Multiple_IF_Condition_Blocks_MultiLine(string studentName)
        {
            //Create Model
            Student student = new Student { StudentName = studentName };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"
#Test 1
{{ #if(StudentName!=NULL) }}
--ok-passed--
{{ #else }}
--error-failed--
{{ #endif }}
#Test 2
{{ #if(StudentName==John Doe) }}
--I am, John Doe--
{{ #else }}
--I am NOT--
{{ #endif }}"

            };
            string generatedTemplate = TemplateMapper.Map(student, template);
            string expectedResult = (studentName == "John Doe")
                ?
                "\r\n#Test 1\r\n\r\n--ok-passed--\r\n\r\n#Test 2\r\n\r\n--I am, John Doe--\r\n"
                : "\r\n#Test 1\r\n\r\n--error-failed--\r\n\r\n#Test 2\r\n\r\n--I am NOT--\r\n";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }
        #endregion


        #region Xml Char Escape Tests
        [Fact]
        public void Should_Escape_Xml_Char_If_Option_Is_Enabled()
        {
            //Create Model
            Student student = new Student { StudentName = "I've got \"special\" < & also >" };
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ StudentName }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template, null, new TemplateMapperOptions
            {
                XmlCharEscaping = true
            });
            string expectedString = "I&apos;ve got &quot;special&quot; &lt; &amp; also &gt;";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Escape_Xml_Char_In_LOOPS_If_Option_Is_Enabled()
        {
            //Create Model
            Student student = new Student
            {
                Invoices = new List<Invoice>
                {
                     new Invoice{  Narration="I've got \"special\""},
                     new Invoice{  Narration="I've got  < & also >"}
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ #foreach(invoices)  }} [{{ Narration }}] {{ #endforeach }}"
            };
            string generatedTemplate = TemplateMapper.Map(student, template, null, new TemplateMapperOptions
            {
                XmlCharEscaping = true
            });
            string expectedResult = @" [I&apos;ve got &quot;special&quot;]  [I&apos;ve got &lt; &amp; also &gt;] ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }
        #endregion


    }
}
