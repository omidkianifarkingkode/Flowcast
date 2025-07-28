#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using FlowPipeline;

[CustomPropertyDrawer(typeof(TypeReference))]
public class TypeReferenceDrawer : PropertyDrawer
{
    private List<Type> _cachedTypes;
    private string[] _displayNames;
    private int _selectedIndex;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var typeNameProp = property.FindPropertyRelative("_typeName");
        var typeAttr = fieldInfo.GetCustomAttributes(typeof(TypeDropdownAttribute), false).FirstOrDefault() as TypeDropdownAttribute;
        var baseType = typeAttr?.BaseType;

        if (_cachedTypes == null)
        {
            _cachedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => SafeGetTypes(a))
                .Where(t =>
                    IsValidStep(t, baseType) &&
                    t.IsClass &&
                    !t.IsAbstract &&
                    !t.IsGenericTypeDefinition)
                .OrderBy(t => t.FullName)
                .ToList();

            _displayNames = _cachedTypes.Select(t => t.Name).ToArray();
        }

        var currentType = Type.GetType(typeNameProp.stringValue);
        _selectedIndex = Mathf.Max(0, _cachedTypes.FindIndex(t => t == currentType));

        EditorGUI.BeginProperty(position, label, property);
        _selectedIndex = EditorGUI.Popup(position, label.text, _selectedIndex, _displayNames);
        typeNameProp.stringValue = _cachedTypes[_selectedIndex].AssemblyQualifiedName;
        EditorGUI.EndProperty();
    }

    private IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly a)
    {
        try { return a.GetTypes(); }
        catch { return Array.Empty<Type>(); }
    }

    private bool IsValidStep(Type candidate, Type baseType)
    {
        if (baseType == null)
            return true;

        if (baseType.IsAssignableFrom(candidate))
            return true;

        if (baseType.IsGenericTypeDefinition)
        {
            return candidate.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == baseType);
        }

        // strict matching against a fully closed type
        return baseType.IsAssignableFrom(candidate);
    }
}
#endif
