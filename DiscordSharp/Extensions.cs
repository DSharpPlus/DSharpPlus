using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DiscordSharp.Objects;

#if !V45
using System.IO;
public static class Net35Extensions
{
    /// <summary>
    /// Check to see if a flags enumeration has a specific flag set.
    /// </summary>
    /// <param name="variable">Flags enumeration to check</param>
    /// <param name="value">Flag to check for</param>
    /// <returns></returns>
    public static bool HasFlag(this Enum variable, Enum value)
    {
        if (variable == null)
            return false;

        if (value == null)
            throw new ArgumentNullException("value");

        // Not as good as the .NET 4 version of this function, but should be good enough
        if (!Enum.IsDefined(variable.GetType(), value))
        {
            throw new ArgumentException(string.Format(
                "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                value.GetType(), variable.GetType()));
        }

        ulong num = Convert.ToUInt64(value);
        return ((Convert.ToUInt64(variable) & num) == num);

    }
    
    public static void CopyTo(this System.IO.Stream input, System.IO.Stream output)
    {
        byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
        int bytesRead;

        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, bytesRead);
        }
    }
}
#endif

public static class JsonExtensions
{
    public static bool IsNullOrEmpty(this JToken token)
    {
        return (token == null) ||
               (token.Type == JTokenType.Array && !token.HasValues) ||
               (token.Type == JTokenType.Object && !token.HasValues) ||
               (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
               (token.Type == JTokenType.Null);
    }
}

public static class EnumExtensions
{
    
    private static void CheckIsEnum<T>(bool withFlags)
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));
        if (withFlags && !Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
            throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", typeof(T).FullName));
    }

    public static bool IsFlagSet<T>(this T value, T flag) where T : struct
    {
        CheckIsEnum<T>(true);
        long lValue = Convert.ToInt64(value);
        long lFlag = Convert.ToInt64(flag);
        return (lValue & lFlag) != 0;
    }

    public static IEnumerable<T> GetFlags<T>(this T value) where T : struct
    {
        CheckIsEnum<T>(true);
        foreach (T flag in Enum.GetValues(typeof(T)).Cast<T>())
        {
            if (value.IsFlagSet(flag))
                yield return flag;
        }
    }

    public static T SetFlags<T>(this T value, T flags, bool on) where T : struct
    {
        CheckIsEnum<T>(true);
        long lValue = Convert.ToInt64(value);
        long lFlag = Convert.ToInt64(flags);
        if (on)
        {
            lValue |= lFlag;
        }
        else
        {
            lValue &= (~lFlag);
        }
        return (T)Enum.ToObject(typeof(T), lValue);
    }

    public static T SetFlags<T>(this T value, T flags) where T : struct
    {
        return value.SetFlags(flags, true);
    }

    public static T ClearFlags<T>(this T value, T flags) where T : struct
    {
        return value.SetFlags(flags, false);
    }

    public static T CombineFlags<T>(this IEnumerable<T> flags) where T : struct
    {
        CheckIsEnum<T>(true);
        long lValue = 0;
        foreach (T flag in flags)
        {
            long lFlag = Convert.ToInt64(flag);
            lValue |= lFlag;
        }
        return (T)Enum.ToObject(typeof(T), lValue);
    }

    public static string GetDescription<T>(this T value) where T : struct
    {
        CheckIsEnum<T>(false);
        string name = Enum.GetName(typeof(T), value);
        if (name != null)
        {
            FieldInfo field = typeof(T).GetField(name);
            if (field != null)
            {
                DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return null;
    }
}