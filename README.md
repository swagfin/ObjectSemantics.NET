# ObjectSemantics.NET
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_shield)

Simple Object to File Mapper that supports string formatting

## Overview

* Maps properties from a source object to a template string and returns the result. This is useful for dynamically generating strings based on object properties.

## Install 

*NuGet Package*
```
Install-Package ObjectSemantics.NET
```
https://nuget.org/packages/ObjectSemantics.NET

**USAGE (Example 1)**
```cs
// Create Model
Student student = new Student
{
    StudentName = "George Waynne",
    Balance = 2510
};

// Define Template
var template = new ObjectSemanticsTemplate
{
    FileContents = @"My Name is: {{ StudentName }} and my balance is {{ Balance:N2 }}"
};

// Map Object to Template
string generatedTemplate = template.Map(student);

// Output the result
Console.WriteLine(generatedTemplate);
```
***Output***
```console
My Name is: George Waynne and my balance is 2,510.00
```

**USAGE (Example 2)**

```csharp
// Create Model
Student student = new Student
{
    StudentName = "John Doe",
    Invoices = new List<Invoice>
    {
         new Invoice{ Id = 2, RefNo = "INV_002", Narration = "Grade II Fees Invoice", Amount = 2000, InvoiceDate = new DateTime(2023, 04, 01) },
         new Invoice{ Id = 1, RefNo = "INV_001", Narration = "Grade I Fees Invoice", Amount = 320, InvoiceDate = new DateTime(2022, 08, 01) }
    }
};

// Define Template
var template = new ObjectSemanticsTemplate
{
    FileContents = @"{{ StudentName }} Invoices
{{ #foreach(Invoices)  }}
<tr>
    <td>{{ Id }}</td>
    <td>{{ RefNo }}</td>
    <td>{{ Narration }}</td>
    <td>{{ Amount:N0 }}</td>
    <td>{{ InvoiceDate:yyyy-MM-dd }}</td>
</tr>
{{ #endforeach }}"
};

// Map Object to Template
string generatedTemplate = template.Map(student);

// Output the result
Console.WriteLine(generatedTemplate);
```
***Output***
```console
John Doe Invoices

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
</tr>

```

## Check out more samples
[ObjectSemantics.NET.Tests](https://github.com/swagfin/ObjectSemantics.NET/tree/master/ObjectSemantics.NET.Tests)

## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_large)
