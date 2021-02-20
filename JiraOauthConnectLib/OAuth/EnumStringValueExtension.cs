using System;
using System.Reflection;

namespace JiraOAuthConnectLib
{
    /// <summary>

    /// 

    /// </summary>

    public static class EnumStringValueExtension

    {

        public static string GetStringValue(this Enum value)

        {

            string output = null;

            Type type = value.GetType();

            FieldInfo fieldInfo = type.GetField(value.ToString());

            EnumStringValueAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(EnumStringValueAttribute), false) as EnumStringValueAttribute[];

            if (attributes.Length > 0)

                output = attributes[0].Value;

            return output;

        }

    }


}
