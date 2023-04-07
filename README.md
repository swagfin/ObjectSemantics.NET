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

**USAGE**
```cs
using ObjectSemantics.NET;
class Program
{
  static void Main(string[] args)
  {
	  
    Student student = new Student
    {
        StudentName = "George",
        Invoices = new List<Invoice>
                  {
                    new Invoice{  Id=2, RefNo="INV_002",Narration="Grade II Fees Invoice", Amount=2000, InvoiceDate=DateTime.Now.Date.AddDays(-1) },
                    new Invoice{  Id=1, RefNo="INV_001",Narration="Grade I Fees Invoice", Amount=320, InvoiceDate=DateTime.Now.Date.AddDays(-2) }
                  }
    };


    ObjectSemanticsTemplate template = new ObjectSemanticsTemplate
    {
        FileContents = @"<h6>{{ StudentName:uppercase }}  Invoices</h6>
                        <ol>
                            {{ for-each-start:invoices   }}
                                <li>Invoice No: {{ RefNo }}  of {{ Narration }} amount {{ Amount:N0 }} </li>
                            {{ for-each-end:invoices }}
                        </ol>"
    };

    string htmlWithData = TemplateMapper.MapFromTemplate(student, template);

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
> This is the raw html template file contents defined in the templates folder (HTML)
# ![Result](https://github.com/swagfin/ObjectSemantics.NET/blob/5f0814c6513baffee7f78c99112d8777abaf4737/Screenshots/recordWithChildren.png)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_shield)

> See how its not affected my excessive whitespaces
# ![Result](https://github.com/swagfin/ObjectSemantics.NET/blob/592e6404783b21dfab60dcc8087b0c23a5ce2b71/Screenshots/results-example.png)

## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_large)