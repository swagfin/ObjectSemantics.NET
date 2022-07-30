# ObjectSemantics.NET
Simple Object to File Mapper that supports string formatting

## Overview

* Mapping your object e.g. StudentDetails to a html Template that you define
* Mapping a collection to enumerable or List to a html/text/ (predefined format)

## Install 

*NuGet Package*
```
Install-Package ObjectSemantics.NET
```
https://nuget.org/packages/ObjectSemantics.NET

## Usage Example
** .NET Core API/Web Apps via Dependency Injection
```cs
    // DI injected service registrations
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddObjectSemantics(new ObjectSemanticsOptions
        {
            TemplatesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates"),
            ReserveTemplatesInMemory = true
            //other configs
        });
        services.AddControllers();
    }
```

**.NET and .NET Core Apps/Consoles**
```cs
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
```

** Report Template File - Raw Template **
> This is the raw html template defined in the templates folder
```html
<h3>Supporting Stylish Whitespaces of {{ RegDate:yyyy-MM-dd }} | {{    RegDate:yyyy-MM-dd hh:mm    }} |  {{RegDate:hh tt    }} |  {{   RegDate:yyyy-dd hh:mm}}</h3>
    <hr />
    <table>
        <thead>
            <tr>
                <th>NAME</th>
                <th>REG DATE</th>
                <th>BALANCE</th>
            </tr>
        </thead>
        <tbody>
            {{ for-each-start }}
            <tr>
                <td>{{ StudentName:uppercase }} and small letter is {{ StudentName:lowercase }} and Saved is {{ StudentName}}</td>
                <td>{{ RegDate:yyyy-MM-dd hh:mm tt }}</td>
                <td>{{ Balance:N2 }}</td>
            </tr>
            {{ for-each-end }}
        </tbody>
    </table>
```

> See how its not affected my excessive whitespaces
```html
<h3>Supporting Stylish Whitespaces of 2022-07-31 | 2022-07-31 01:13 |  01 AM |  2022-31 01:13</h3>
    <hr />
    <table>
        <thead>
            <tr>
                <th>NAME</th>
                <th>REG DATE</th>
                <th>BALANCE</th>
            </tr>
        </thead>
        <tbody>

            <tr>
                <td>GEORGE and small letter is george and Saved is George</td>
                <td>2022-07-31 01:13 AM</td>
                <td>2,320.00</td>
            </tr>

            <tr>
                <td>STEVE and small letter is steve and Saved is Steve</td>
                <td>2022-07-31 01:13 AM</td>
                <td>1,200.00</td>
            </tr>
        </tbody>
    </table>
```
