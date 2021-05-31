using System;
using System.ComponentModel;

namespace DapperExtensions.Extensions
{
    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var memInfo = value.GetType().GetMember(value.ToString());

            if (memInfo?.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs?.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return value.ToString();
        }
    }
}
