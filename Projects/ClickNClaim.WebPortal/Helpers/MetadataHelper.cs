using ClickNClaim.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ClickNClaim.WebPortal.Helpers
{
    public class MetadataHelper
    {
        public static string GetEnumDisplayNAme(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            EnumDisplayNameAttribute[] attributes = (EnumDisplayNameAttribute[])fi.GetCustomAttributes(typeof(EnumDisplayNameAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].DisplayName;
            }
            else
            {
                return value.ToString();
            }


        }

        public static string GetEnumDescription(Enum value)
        {
            // Get the Description attribute value for the enum value
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}