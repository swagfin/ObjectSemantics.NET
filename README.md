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

### Example 1: Mapping Object Properties

```csharp
Person person = new Person
{
    Name = "John Doe"
};

// Define template and map it using the object
string result = person.Map("I am {{ Name }}!");

Console.WriteLine(result);
```

**Output:**
```
I am John Doe!
```
---

### Example 2: Mapping Using String Extension

```csharp
Person person = new Person
{
    Name = "Jane Doe"
};

// You can also start with the string template
string result = "I am {{ Name }}!".Map(person);

Console.WriteLine(result);
```

**Output:**
```
I am Jane Doe!
```
---

### Example 3: Mapping Enumerable Collections (Looping)

```csharp
Person person = new Person
{
    MyCars = new List<Car>
    {
        new Car { Make = "BMW", Year = 2023 },
        new Car { Make = "Rolls-Royce", Year = 2020 }
    }
};

string template = @"
{{ #foreach(MyCars) }}
 - {{ Year }} {{ Make }}
{{ #endforeach }}";

string result = person.Map(template);

Console.WriteLine(result);
```

**Output:**
```
 - 2023 BMW
 - 2020 Rolls-Royce
```
---

### Example 4: Conditional Logic with `#if`, `#else`, and `#endif`

```csharp
Person person = new Person
{
    Age = 40
};

string template = @"
{{ #if(Age >= 18) }}
  Adult
{{ #else }}
  Minor
{{ #endif }}";

string result = person.Map(template);

Console.WriteLine(result);
```

**Output:**
```
Adult
```
---
### Example 5: Number Formatting Support

```csharp
Car car = new Car
{
    Price = 50000
};

string result = car.Map("{{ Price:#,##0 }} | {{ Price:N2 }}");

Console.WriteLine(result);
```

**Output:**
```
50,000 | 50,000.00
```
---

## 💡 More Examples & Documentation

Explore more usage examples and edge cases in the Wiki Page:

📁 [`Wiki Page`](https://github.com/swagfin/ObjectSemantics.NET/wiki/%F0%9F%9B%A0-Usage-Guide)

---

## 🤝 Contributing

Feel free to open issues or contribute improvements via pull requests!

---

## 📄 MIT License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_large)
