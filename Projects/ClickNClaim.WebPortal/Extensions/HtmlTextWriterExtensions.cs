using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace ClickNClaim.WebPortal.Extensions
{
    public static class HtmlTextWriterExtensions
    {
        public static void AddTag(this HtmlTextWriter writer, string tag, string text, HtmlTextWriterOptions options = null)
        {
            if (options == null)
                options = HtmlTextWriterDefaults.DefaultText;

            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, options.FontFamily);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, options.FontSize);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, options.FontWeight);

            if (!String.IsNullOrWhiteSpace(options.TextAlign))
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, options.TextAlign);
            if (!String.IsNullOrWhiteSpace(options.FontStyle))
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontStyle, options.FontStyle);
            if (!String.IsNullOrWhiteSpace(options.FontStyle))
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, options.PaddingLeft);
            if (!String.IsNullOrWhiteSpace(options.TextDecoration))
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextDecoration, options.TextDecoration);

            writer.RenderBeginTag(tag);

            writer.Write(text ?? "");
            writer.RenderEndTag();
        }

        public static void AddLink(this HtmlTextWriter writer, string text, string href)
        {
            writer.RenderBeginTag("p");
            writer.RenderBeginTag("a");
            writer.AddAttribute("href", href);
            writer.Write(text);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        public static void AddTODO(this HtmlTextWriter writer, string text)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "red");
            writer.RenderBeginTag("p");
            writer.Write(text);
            writer.RenderEndTag();
        }


        public static void BreakLine(this HtmlTextWriter writer)
        {
            writer.RenderBeginTag("br");
            writer.RenderEndTag();
        }

        public static void HLine(this HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "2px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "black");
            writer.RenderBeginTag("div");

            writer.RenderEndTag();
        }

        public static void TitleBox(this HtmlTextWriter writer, string content)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "1px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "black");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "solid");
            writer.RenderBeginTag("div");
            writer.Write(content);
            writer.RenderEndTag();
        }


    }



    public class HtmlTextWriterOptions
    {
        public string FontFamily;
        public string FontSize;
        public string FontWeight;
        public string TextAlign;
        public string FontStyle;
        public string PaddingLeft;
        public string TextDecoration;
    }

    public class HtmlTextWriterDefaults
    {

        public static HtmlTextWriterOptions CenteredTitle
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "22px", FontWeight = "bold", TextAlign = "Center" };
            }
        }

        public static HtmlTextWriterOptions CenteredSubTitleInfo
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "12px", FontWeight = "normal", TextAlign = "center", FontStyle = "italic" };
            }
        }

        public static HtmlTextWriterOptions DefaultText
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "12px", FontWeight = "normal" };
            }
        }

        public static HtmlTextWriterOptions DefaultText_WithLeftPadding
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "12px", FontWeight = "bold", PaddingLeft = "20px" };
            }
        }

        public static HtmlTextWriterOptions DefaulText_Bold
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "12px", FontWeight = "bold" };
            }
        }

        public static HtmlTextWriterOptions DefaultText_Italic
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "12px", FontWeight = "normal", FontStyle ="italic" };
            }
        }


        public static HtmlTextWriterOptions DefaultText_Underlined
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "12px", FontWeight = "normal", TextDecoration="underline" };
            }
        }

        public static HtmlTextWriterOptions H1
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "22px", FontWeight = "bold" };
            }
        }

        public static HtmlTextWriterOptions H2
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "18px", FontWeight = "bold" };
            }
        }

        public static HtmlTextWriterOptions H2_Centered
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "18px", FontWeight = "bold", TextAlign= "center" };
            }
        }

        public static HtmlTextWriterOptions H3
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "16px", FontWeight = "normal" };
            }
        }

        public static HtmlTextWriterOptions MissionActTitle
        {
            get
            {
                return new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "20px", FontWeight = "bold", TextDecoration = "underline" };
            }
        }

    }


}