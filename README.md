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
          ReserveTemplatesInMemory = false,
          SupportedTemplateFileExtensions = new string[] { ".html" },
          TemplatesDirectory = Path.Combine(Environment.CurrentDirectory, "Samples")
      });
	  //Add Custom Headers as well
      List<ObjectSemanticsKeyValue> headers = new List<ObjectSemanticsKeyValue>
      {
           new ObjectSemanticsKeyValue{ Key ="CompanyName",  Value= "CRUDSOFT TECHNOLOGIES" },
           new ObjectSemanticsKeyValue{ Key ="CompanyEmail",  Value= "georgewainaina18@gmail.com" },
           new ObjectSemanticsKeyValue{ Key ="CompanyEmployees",  Value= 1289 },
      };

	  //Example Object
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

      string exampleOne = objectSemantics.GenerateTemplate(student, "record.html", headers);
	  //Or
      string htmlWithData = objectSemantics.GenerateTemplate(student, "recordWithChildren.html", headers);

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
<hr />
<h6>Also Supports Additional Parameters</h6>
<h4>COMPANY NAME: {{ CompanyName }}</h4>
<h4>COMPANY EMAIL: {{ CompanyEmail }}</h4>
<h4>COMPANY EMPLOYEES: {{ CompanyEmployees}}</h4>

<h3>DATE OF REGISTRATION :  {{ RegDate:yyyy-MM-dd }} | {{    RegDate:yyyy-MM-dd hh:mm    }} |  {{RegDate:hh tt    }} |  {{   RegDate:yyyy-dd hh:mm}}</h3>
<br />
<h6>This data is protected by {{ CompanyName }} and licened by {{NoIdeaPeople}}</h6>
<hr />

<h4>{{ StudentName:uppercase }} DETAILS</h4>

<ol>
    <li><strong>STUDENT NAME</strong>: {{ StudentName:uppercase }}</li>
    <li><strong>STUDENT REG. DATE</strong>: {{ RegDate:yyyy-MM-dd hh:mm tt }}</li>
    <li><strong>CURRENT BALANCE</strong>: Ksh. <span style="color:red;font-weight:bold;font-size:17px">{{ Balance:N2 }}</span> </li>
</ol>

<h4>{{ StudentName:uppercase }} INVOICES</h4>

<table>
    <thead>
        <tr>
            <th>NO</th>
            <th>INVOICE REF</th>
            <th colspan="2">NARRATION</th>
            <th>AMOUNT</th>
            <th>INVOICE DATE</th>
        </tr>
    </thead>
    <tbody>
        {{ for-each-start:invoices  }}
        <tr>
            <td>{{ Id }}</td>
            <td>{{ RefNo }}</td>
            <td colspan="2">{{ Narration }}</td>
            <td>{{ Amount:N0 }}</td>
            <td>{{ InvoiceDate:yyyy-MM-dd }}</td>
        </tr>
        {{ for-each-end:invoices }}
    </tbody>
</table>

<h6>Recent Invoices</h6>
<ol>
    {{ for-each-start:invoices   }}
    <li>Invoice No: {{ RefNo }}  of {{ Narration }} amount {{ Amount:N0 }} </li>
    {{ for-each-end:invoices }}
</ol>
```

> See how its not affected my excessive whitespaces
# ![Result](https://github.com/swagfin/ObjectSemantics.NET/blob/592e6404783b21dfab60dcc8087b0c23a5ce2b71/Screenshots/results-example.png)