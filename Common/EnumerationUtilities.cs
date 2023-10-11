using System;
using System.Reflection;
using JeopardyKing.GameComponents;

namespace JeopardyKing.Common
{
    internal static class EnumerationUtilities
    {
        public static (T? attr, Exception? e) GetCustomAttributeFromEnum<T>(this Enum? value)
        {
            if (value == null)
                return (default, new ArgumentNullException(nameof(value)));

            var t = value.GetType();
            var name = Enum.GetName(t, value);
            if (string.IsNullOrEmpty(name))
                return (default, new InvalidOperationException($"Could not get name of enum {t}"));

            if (t.GetField(name) is not FieldInfo field)
                return (default, new InvalidOperationException($"Could not get field info for enum {name}"));

            if (Attribute.GetCustomAttribute(field, typeof(T)) is T attr)
                return (attr, default);
            else
                return (default, new ArgumentException($"{value} does not have attribute '{typeof(T)}'"));
        }

        public static void ActOnEnumMembersWithAttribute<TEnum, TAttr>(Action<TEnum, TAttr> action) where TEnum : struct, Enum
                                                                                             where TAttr : Attribute
        {
            foreach (var t in Enum.GetValues<TEnum>())
            {
                var (attr, _) = t.GetCustomAttributeFromEnum<TAttr>();
                if (attr != default)
                    action(t, attr);
            }
        }
    }
}
