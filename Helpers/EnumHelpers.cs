using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace DSharpPlus.SlashCommands.Helpers
{
    public static class EnumHelpers
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
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
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        return descriptionAttribute != null ? descriptionAttribute.Description : type.GetEnumName(val);
                    }
                }
            }
            return null;
        }
    }
}