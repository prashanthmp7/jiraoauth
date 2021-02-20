using System;

namespace JiraOAuthConnectLib
{
    /// <summary>

    /// 

    /// </summary>

    [AttributeUsage(AttributeTargets.Field)]



    public class EnumStringValueAttribute : Attribute

    {
        public string Value { get; private set; }

        public EnumStringValueAttribute(string value)

        {

            Value = value;

        }

    }

}
