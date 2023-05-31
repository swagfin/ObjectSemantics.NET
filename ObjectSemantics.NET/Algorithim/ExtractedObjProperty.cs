﻿using System;
using System.Collections;

public class ExtractedObjProperty
{
    public Type Type { get; set; }
    public string Name { get; set; }
    public object OriginalValue { get; set; }
    public string StringFormatted { get { return string.Format("{0}", OriginalValue); } }
    public bool IsEnumerableObject
    {
        get { return typeof(IEnumerable).IsAssignableFrom(Type) && Type != typeof(string); }
    }
}