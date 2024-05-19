using ObjectSemantics.NET.Tests.MoqModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class LoopFunctionTests
    {

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
            string generatedTemplate = template.Map(student);
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
            string generatedTemplate = template.Map(student);
            string expectedResult = " [INV_002] [INV_001] ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Map_Multiple_Same_Property_Enumerable_Collection_On_Same_Template()
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
            string generatedTemplate = template.Map(student);
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
        public void Should_Map_Multiple_Different_Property_Enumerable_Collection_On_Same_Template()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John Doe",
                Invoices = new List<Invoice>
                {
                     new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate= new DateTime(2023, 04, 01) },
                     new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate= new DateTime(2022, 08, 01)  }
                },
                StudentClockInDetails = new List<StudentClockInDetail>
                {
                    new StudentClockInDetail{ LastClockedInDate = new DateTime(2024, 04, 01), LastClockedInPoints = 10 },
                    new StudentClockInDetail{ LastClockedInDate = new DateTime(2024, 04, 02), LastClockedInPoints = 30 }
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
{{ #foreach(studentClockInDetails)  }}
    <h5>Got {{ LastClockedInPoints }} for {{ LastClockedInDate:yyyy-MM-dd }}</h5>
{{ #endforeach }}"
            };
            string generatedTemplate = template.Map(student);
            string expectedResult = @"
John Doe Invoices
LOOP #1

    <h5>2 On Loop #1</h5>

    <h5>1 On Loop #1</h5>
LOOP #2

    <h5>Got 10 for 2024-04-01</h5>

    <h5>Got 30 for 2024-04-02</h5>";

            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

    }
}
