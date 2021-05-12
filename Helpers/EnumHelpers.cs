using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace DSharpPlus.SlashCommands
{
    public static class EnumHelpers
    {
        /// <summary>
        /// Gets the name from the <see cref="ChoiceNameAttribute"/> for this enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns>The name.</returns>
        public static string GetName<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var nameAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(ChoiceNameAttribute), false)
                            .FirstOrDefault() as ChoiceNameAttribute;

                        return nameAttribute != null ? nameAttribute.Name : type.GetEnumName(val);
                    }
                }
            }
            return null;
        }
    }
}