# ObjectSemantics.NET
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_shield)

**Simple and flexible object-to-template string mapper with formatting support**

## 🧠 Overview

**ObjectSemantics.NET** is a lightweight C# library that lets you inject object property values directly into string templates much like [Handlebars](https://handlebarsjs.com/) or Helm templates, but focused on .NET.

This is especially useful when you want to dynamically generate content such as:
- Email templates
- HTML fragments
- Reports or invoices
- Config files
- Logging output
---

## 📦 Installation

Install from [NuGet](https://www.nuget.org/packages/ObjectSemantics.NET):

```bash
Install-Package ObjectSemantics.NET
```

---

## 🚀 Quick Start

### Example 1: Basic Object Property Mapping

```csharp
// Create model
Student student = new Student
{
    StudentName = "George Waynne",
    Balance = 2510
};

// Define template
var template = new ObjectSemanticsTemplate
{
    FileContents = @"My Name is: {{ StudentName }} and my balance is {{ Balance:N2 }}"
};

// Map object to template
string result = template.Map(student);

Console.WriteLine(result);
```

**Output:**
```
My Name is: George Waynne and my balance is 2,510.00
```

---

### Example 2: Mapping Enumerable Collections

```csharp
Student student = new Student
{
    StudentName = "John Doe",
    Invoices = new List<Invoice>
    {
        new Invoice { Id = 2, RefNo = "INV_002", Narration = "Grade II Fees", Amount = 2000, InvoiceDate = new DateTime(2023, 04, 01) },
        new Invoice { Id = 1, RefNo = "INV_001", Narration = "Grade I Fees", Amount = 320, InvoiceDate = new DateTime(2022, 08, 01) }
    }
};

var template = new ObjectSemanticsTemplate
{
    FileContents = @"{{ StudentName }} Invoices
{{ #foreach(Invoices) }}
<tr>
    <td>{{ Id }}</td>
    <td>{{ RefNo }}</td>
    <td>{{ Narration }}</td>
    <td>{{ Amount:N0 }}</td>
    <td>{{ InvoiceDate:yyyy-MM-dd }}</td>
</tr>
{{ #endforeach }}"
};

string result = template.Map(student);

Console.WriteLine(result);
```

**Output:**
```
John Doe Invoices

<tr>
    <td>2</td>
    <td>INV_002</td>
    <td>Grade II Fees</td>
    <td>2,000</td>
    <td>2023-04-01</td>
</tr>

<tr>
    <td>1</td>
    <td>INV_001</td>
    <td>Grade I Fees</td>
    <td>320</td>
    <td>2022-08-01</td>
</tr>
```

---

### Example 3: String Formatters

Use format like `uppercase`, `lowercase`, `titlecase`, `length`.

```csharp
FileContents = "Uppercase: {{ StudentName:uppercase }}, Length: {{ StudentName:length }}"
```
Outputs:

```
Uppercase: ALICE, Length: 5
```

---

### Example 4: If Conditionals (`if`)

```csharp
FileContents = @"
{{ if Age >= 18 }}
Adult
{{ else }}
Minor
{{ end }}"
```
Outputs:

```
Adult
```

---
Explore more usage examples and edge cases in the test project:

📁 [`ObjectSemantics.NET.Tests`](./ObjectSemantics.NET.Tests)

---

## 🤝 Contributing

Feel free to open issues or contribute improvements via pull requests!

---

## 📄 MIT License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_large)
