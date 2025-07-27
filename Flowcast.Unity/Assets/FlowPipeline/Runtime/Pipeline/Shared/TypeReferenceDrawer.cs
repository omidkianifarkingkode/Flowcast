#if UNITY_EDITOR
using FlowPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
        var baseType = typeAttr?.BaseType ?? typeof(object);

        // Cache all matching types
        if (_cachedTypes == null)
        {
            _cachedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    baseType.IsAssignableFrom(t) &&
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
}
#endif