using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Graphics;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace TrollTrack.Features.Shared.Models.Entities
{
    /// <summary>
    /// Represents all available Colors
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LureColor
    {
        NA = 0,

        // Neutrals (8)
        Black,
        [Description("Dark Slate Gray")]
        DarkSlateGray,
        [Description("Dim Gray")]
        DimGray,
        [Description("Slate Gray")]
        SlateGray,
        Gray,
        [Description("Dark Gray")]
        DarkGray,
        Silver,
        White,

        // Reds (8)
        [Description("Dark Red")]
        DarkRed,
        Maroon,
        Firebrick,
        Crimson,
        Red,
        [Description("Indian Red")]
        IndianRed,
        [Description("Light Coral")]
        LightCoral,
        Salmon,

        // Oranges (8)
        [Description("Dark Orange")]
        DarkOrange,
        [Description("Orange Red")]
        OrangeRed,
        Orange,
        Tomato,
        Coral,
        [Description("Light Salmon")]
        LightSalmon,
        [Description("Peach Puff")]
        PeachPuff,
        [Description("Navajo White")]
        NavajoWhite,

        // Yellows (8)
        Gold,
        Yellow,
        [Description("Light Yellow")]
        LightYellow,
        [Description("Lemon Chiffon")]
        LemonChiffon,
        Khaki,
        [Description("Dark Khaki")]
        DarkKhaki,
        Goldenrod,
        [Description("Pale Goldenrod")]
        PaleGoldenrod,

        // Greens (8)
        [Description("Dark Green")]
        DarkGreen,
        Green,
        [Description("Forest Green")]
        ForestGreen,
        [Description("Sea Green")]
        SeaGreen,
        [Description("Medium Sea Green")]
        MediumSeaGreen,
        [Description("Lime Green")]
        LimeGreen,
        [Description("Lawn Green")]
        LawnGreen,
        [Description("Yellow Green")]
        YellowGreen,

        // Teals/Cyans (8)
        Teal,
        [Description("Dark Cyan")]
        DarkCyan,
        [Description("Cadet Blue")]
        CadetBlue,
        [Description("Dark Turquoise")]
        DarkTurquoise,
        [Description("Medium Turquoise")]
        MediumTurquoise,
        Turquoise,
        Aquamarine,
        Cyan,

        // Blues (8)
        Navy,
        [Description("Midnight Blue")]
        MidnightBlue,
        [Description("Dark Blue")]
        DarkBlue,
        [Description("Medium Blue")]
        MediumBlue,
        Blue,
        [Description("Royal Blue")]
        RoyalBlue,
        [Description("Dodger Blue")]
        DodgerBlue,
        [Description("Deep Sky Blue")]
        DeepSkyBlue,

        // Purples/Pinks (8)
        Indigo,
        Purple,
        [Description("Dark Magenta")]
        DarkMagenta,
        [Description("Blue Violet")]
        BlueViolet,
        [Description("Medium Purple")]
        MediumPurple,
        Violet,
        Fuchsia,
        [Description("Deep Pink")]
        DeepPink,

        // Fluorescent Colors
        [Description("Fluorescent Pink")]
        FluorescentPink = 100,
        [Description("Fluorescent Green")]
        FluorescentGreen = 101,
        [Description("Fluorescent Yellow")]
        FluorescentYellow = 102,
        [Description("Fluorescent Orange")]
        FluorescentOrange = 103,
        [Description("Fluorescent Blue")]
        FluorescentBlue = 104,
        [Description("Fluorescent Red")]
        FluorescentRed = 105,

        // Metallic
        Chrome = 150,
        Copper = 151,
        Bronze = 152,

        // Special
        Clear = 200,
        Glow = 201,
        Holographic = 202
    }

    public static class ColorPalette
    {
        public static readonly Dictionary<LureColor, Color> LureColors = new()
        {
            // Neutrals (8)
            { LureColor.Black, Colors.Black },
            { LureColor.DarkSlateGray, Colors.DarkSlateGray },
            { LureColor.DimGray, Colors.DimGray },
            { LureColor.SlateGray, Colors.SlateGray },
            { LureColor.Gray, Colors.Gray },
            { LureColor.DarkGray, Colors.DarkGray },
            { LureColor.Silver, Colors.Silver },
            { LureColor.White, Colors.White },

            // Reds (8)
            { LureColor.DarkRed, Colors.DarkRed },
            { LureColor.Maroon, Colors.Maroon },
            { LureColor.Firebrick, Colors.Firebrick },
            { LureColor.Crimson, Colors.Crimson },
            { LureColor.Red, Colors.Red },
            { LureColor.IndianRed, Colors.IndianRed },
            { LureColor.LightCoral, Colors.LightCoral },
            { LureColor.Salmon, Colors.Salmon },

            // Oranges (8)
            { LureColor.DarkOrange, Colors.DarkOrange },
            { LureColor.OrangeRed, Colors.OrangeRed },
            { LureColor.Orange, Colors.Orange },
            { LureColor.Tomato, Colors.Tomato },
            { LureColor.Coral, Colors.Coral },
            { LureColor.LightSalmon, Colors.LightSalmon },
            { LureColor.PeachPuff, Colors.PeachPuff },
            { LureColor.NavajoWhite, Colors.NavajoWhite },

            // Yellows (8)
            { LureColor.Gold, Colors.Gold },
            { LureColor.Yellow, Colors.Yellow },
            { LureColor.LightYellow, Colors.LightYellow },
            { LureColor.LemonChiffon, Colors.LemonChiffon },
            { LureColor.Khaki, Colors.Khaki },
            { LureColor.DarkKhaki, Colors.DarkKhaki },
            { LureColor.Goldenrod, Colors.Goldenrod },
            { LureColor.PaleGoldenrod, Colors.PaleGoldenrod },

            // Greens (8)
            { LureColor.DarkGreen, Colors.DarkGreen },
            { LureColor.Green, Colors.Green },
            { LureColor.ForestGreen, Colors.ForestGreen },
            { LureColor.SeaGreen, Colors.SeaGreen },
            { LureColor.MediumSeaGreen, Colors.MediumSeaGreen },
            { LureColor.LimeGreen, Colors.LimeGreen },
            { LureColor.LawnGreen, Colors.LawnGreen },
            { LureColor.YellowGreen, Colors.YellowGreen },

            // Teals/Cyans (8)
            { LureColor.Teal, Colors.Teal },
            { LureColor.DarkCyan, Colors.DarkCyan },
            { LureColor.CadetBlue, Colors.CadetBlue },
            { LureColor.DarkTurquoise, Colors.DarkTurquoise },
            { LureColor.MediumTurquoise, Colors.MediumTurquoise },
            { LureColor.Turquoise, Colors.Turquoise },
            { LureColor.Aquamarine, Colors.Aquamarine },
            { LureColor.Cyan, Colors.Cyan },

            // Blues (8)
            { LureColor.Navy, Colors.Navy },
            { LureColor.MidnightBlue, Colors.MidnightBlue },
            { LureColor.DarkBlue, Colors.DarkBlue },
            { LureColor.MediumBlue, Colors.MediumBlue },
            { LureColor.Blue, Colors.Blue },
            { LureColor.RoyalBlue, Colors.RoyalBlue },
            { LureColor.DodgerBlue, Colors.DodgerBlue },
            { LureColor.DeepSkyBlue, Colors.DeepSkyBlue },

            // Purples/Pinks (8)
            { LureColor.Indigo, Colors.Indigo },
            { LureColor.Purple, Colors.Purple },
            { LureColor.DarkMagenta, Colors.DarkMagenta },
            { LureColor.BlueViolet, Colors.BlueViolet },
            { LureColor.MediumPurple, Colors.MediumPurple },
            { LureColor.Violet, Colors.Violet },
            { LureColor.Fuchsia, Colors.Fuchsia },
            { LureColor.DeepPink, Colors.DeepPink },
/*
            // Basic Colors
            { LureColor.White, Color.FromArgb("#FFFFFF") },
            { LureColor.Black, Color.FromArgb("#000000") },
            { LureColor.Gray, Color.FromArgb("#808080") },
            { LureColor.Silver, Color.FromArgb("#C0C0C0") },
        
            // Primary Colors
            { LureColor.Red, Color.FromArgb("#FF0000") },
            { LureColor.Blue, Color.FromArgb("#0000FF") },
            { LureColor.Yellow, Color.FromArgb("#FFFF00") },
            { LureColor.Green, Color.FromArgb("#008000") },
        
            // Secondary Colors
            { LureColor.Orange, Color.FromArgb("#FFA500") },
            { LureColor.Purple, Color.FromArgb("#800080") },
            { LureColor.Pink, Color.FromArgb("#FFC0CB") },
            { LureColor.Brown, Color.FromArgb("#A52A2A") },
        
            // Extended Colors
            { LureColor.Cyan, Color.FromArgb("#00FFFF") },
            { LureColor.Magenta, Color.FromArgb("#FF00FF") },
            { LureColor.Lime, Color.FromArgb("#00FF00") },
            { LureColor.Navy, Color.FromArgb("#000080") },
            { LureColor.Teal, Color.FromArgb("#008080") },
            { LureColor.Olive, Color.FromArgb("#808000") },
            { LureColor.Maroon, Color.FromArgb("#800000") },
            { LureColor.Fuchsia, Color.FromArgb("#FF00FF") },
            { LureColor.Gold, Color.FromArgb("#FFD700") },
            { LureColor.Tan, Color.FromArgb("#D2B48C") },
            { LureColor.Beige, Color.FromArgb("#F5F5DC") },
            { LureColor.Ivory, Color.FromArgb("#FFFFF0") },
            { LureColor.Lavender, Color.FromArgb("#E6E6FA") },
            { LureColor.Mint, Color.FromArgb("#98FF98") },
            { LureColor.Peach, Color.FromArgb("#FFDAB9") },
            { LureColor.Coral, Color.FromArgb("#FF7F50") },
            { LureColor.Salmon, Color.FromArgb("#FA8072") },
            { LureColor.Crimson, Color.FromArgb("#DC143C") },
            { LureColor.Indigo, Color.FromArgb("#4B0082") },
            { LureColor.Violet, Color.FromArgb("#EE82EE") },
            { LureColor.Turquoise, Color.FromArgb("#40E0D0") },
*/        
            // Fluorescent Colors (bright, saturated versions)
            { LureColor.FluorescentPink, Color.FromArgb("#FF10F0") },
            { LureColor.FluorescentGreen, Color.FromArgb("#39FF14") },
            { LureColor.FluorescentYellow, Color.FromArgb("#FFFF00") },
            { LureColor.FluorescentOrange, Color.FromArgb("#FF5F00") },
            { LureColor.FluorescentBlue, Color.FromArgb("#0DFFFF") },
            { LureColor.FluorescentRed, Color.FromArgb("#FF073A") },
        
            // Metallic (approximations)
            { LureColor.Chrome, Color.FromArgb("#E5E4E2") },
            { LureColor.Copper, Color.FromArgb("#B87333") },
            { LureColor.Bronze, Color.FromArgb("#CD7F32") },
        
            // Special (using reasonable defaults)
            { LureColor.Glow, Color.FromArgb("#D7FFD7") }, // Light green glow
            { LureColor.Holographic, Color.FromArgb("#E0E0E0") }, // Silver-ish
            { LureColor.Clear, Color.FromArgb("#F0FFFFFF") }, // Very light transparent white
        };


        public static Color GetColor(LureColor LureColor)
        {
            return LureColors.TryGetValue(LureColor, out var color) ? color : LureColors[LureColor.White];
        }

        public static double GetColorHue(LureColor LureColor)
        {
            return LureColors.TryGetValue(LureColor, out var color) ? color.GetDegreeHue() : LureColors[LureColor.White].GetDegreeHue();
        }

        public static float GetColorSaturation(LureColor LureColor)
        {
            return LureColors.TryGetValue(LureColor, out var color) ? color.GetSaturation() : LureColors[LureColor.White].GetSaturation();
        }

        public static string GetCategory(LureColor lureColor)
        {
            var value = (int)lureColor;
            if (value >= 200) return "Special";
            if (value >= 150) return "Metallic";
            if (value >= 100) return "Fluorescent";
            return "Regular";
        }
    }
}