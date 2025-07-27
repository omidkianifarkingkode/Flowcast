using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class TypeDropdownAttribute : PropertyAttribute
{
    public Type BaseType { get; }

    public TypeDropdownAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}
