//using ObjectSemantics.NET.Tests.MoqModels;
//using System.Collections.Generic;
//using Xunit;

//namespace ObjectSemantics.NET.Tests
//{
//    public class BasicMappingTests
//    {
//        [Fact]
//        public void Should_Map_Object_To_Template_From_TemplateObject()
//        {
//            //Create Model
//            Student student = new Student
//            {
//                StudentName = "George Waynne",
//                Balance = 2510
//            };

//            string generatedTemplate = student.Map(@"My Name is: {{ StudentName }}");
//            string expectedString = "My Name is: George Waynne";
//            Assert.Equal(expectedString, generatedTemplate, false, true, true);
//        }


//        [Fact]
//        public void Should_Map_Additional_Parameters()
//        {
//            //Create Model
//            Student student = new Student
//            {
//                StudentName = "George Waynne"
//            };

//            //Additional Parameters
//            Dictionary<string, object> additionalParams = new Dictionary<string, object>
//            {
//                { "CompanyName","TEST INC."},
//                { "CompanyEmail","test.inc@test.com"},
//                { "Employees", 1289 }
//            };
//            string generatedTemplate = student.Map(@"My Name is: {{ StudentName }} | CompanyName: {{ CompanyName }} | CompanyEmail: {{ CompanyEmail }} | Employees: {{ Employees }}", additionalParams);
//            string expectedString = "My Name is: George Waynne | CompanyName: TEST INC. | CompanyEmail: test.inc@test.com | Employees: 1289";
//            Assert.Equal(expectedString, generatedTemplate, false, true, true);
//        }


//        [Fact]
//        public void Should_Return_Unknown_Properties_If_Not_Found_In_Object()
//        {
//            //Create Model
//            Student student = new Student
//            {
//                StudentName = "George Waynne"
//            };
//            //Template
//            var template = new ObjectSemanticsTemplate
//            {
//                FileContents = @"Unknown Object example: {{StudentIdentityCardXyx}}"
//            };
//            string generatedTemplate = template.Map(student);
//            string expectedString = "Unknown Object example: {{ StudentIdentityCardXyx }}";
//            Assert.Equal(expectedString, generatedTemplate, false, true, true);
//        }


//        [Fact]
//        public void Should_Ignore_Whitespaces_Inside_CurlyBrackets()
//        {
//            //Create Model
//            Student student = new Student
//            {
//                StudentName = "George Waynne"
//            };
//            //Template
//            var template = new ObjectSemanticsTemplate
//            {
//                FileContents = @"StudentName is: {{            StudentName       }}"
//            };
//            string generatedTemplate = template.Map(student);
//            string expectedString = "StudentName is: George Waynne";
//            Assert.Equal(expectedString, generatedTemplate, false, true, true);
//        }

//    }
//}
