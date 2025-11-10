using System;
using System.Collections.Generic;
using System.Text;

namespace Flowcast.CodeGenerator;

internal static class NameUtilities
{
    private static readonly HashSet<string> CSharpKeywords = new HashSet<string>(StringComparer.Ordinal)
    {
        "abstract","as","base","bool","break","byte","case","catch","char","checked","class","const","continue",
        "decimal","default","delegate","do","double","else","enum","event","explicit","extern","false","finally",
        "fixed","float","for","foreach","goto","if","implicit","in","int","interface","internal","is","lock","long",
        "namespace","new","null","object","operator","out","override","params","private","protected","public","readonly",
        "ref","return","sbyte","sealed","short","sizeof","stackalloc","static","string","struct","switch","this","throw",
        "true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual","void","volatile","while"
    };

    public static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Identifier";
        }

        var builder = new StringBuilder(value.Length);
        var capitalize = true;
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(capitalize ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
                capitalize = false;
            }
            else
            {
                capitalize = true;
            }
        }

        if (builder.Length == 0)
        {
            return "Identifier";
        }

        return EnsureValidIdentifier(builder.ToString(), true);
    }

    public static string ToCamelCase(string value)
    {
        var pascal = ToPascalCase(value);
        if (pascal.Length == 0)
        {
            return pascal;
        }

        return char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
    }

    public static string EnsureValidIdentifier(string value, bool capitalizeFirst = false)
    {
        if (string.IsNullOrEmpty(value))
        {
            return capitalizeFirst ? "Identifier" : "identifier";
        }

        var builder = new StringBuilder(value.Length + 1);
        builder.Append(value);
        if (!IsValidFirstCharacter(builder[0]))
        {
            builder.Insert(0, '_');
        }

        for (var i = 0; i < builder.Length; i++)
        {
            if (!IsValidCharacter(builder[i]))
            {
                builder[i] = '_';
            }
        }

        var identifier = builder.ToString();
        if (CSharpKeywords.Contains(identifier))
        {
            identifier = "@" + identifier;
        }

        return identifier;
    }

    private static bool IsValidFirstCharacter(char c) => char.IsLetter(c) || c == '_';

    private static bool IsValidCharacter(char c) => char.IsLetterOrDigit(c) || c == '_';
}
