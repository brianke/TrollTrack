using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace TrollTrack.Features.Shared.Models.Entities
{
/*
    [Table("LureFrontColors")]
    public class LureFrontColorEntity
    {

        /// <summary>
        /// Override the default GetHashCode() so SelectedItem can find the matching item
        /// </summary>
        /// <returns>hashcode of <see cref="LureFrontColorEntity"/></returns>
        public override int GetHashCode()
        {
            return HelperClass.CalculateHashCode(this,
                    () => this.Id);    //,
                    //() => this.LureId,
                    //() => this.Color,
                    //() => this.DisplayOrder);
        }

        /// <summary>
        /// Override Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object? obj)
        {
            var item = obj as LureFrontColorEntity;

            if (item == null)
            {
                return false;
            }

            // ?? String.Empty will return empty string if LureDataEntity is null (avoids a object is null error)
            return (this.Id).Equals(item.Id);
                    //&& (this.LureId).Equals(item.LureId)
                    //&& (this.Color).Equals(item.Color)
                    //&& (this.DisplayOrder).Equals(item.DisplayOrder);
        }


        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(typeof(LureDataEntity))]
        public Guid LureId { get; set; }

        public LureColor Color { get; set; } = LureColor.NA;

        //public int DisplayOrder { get; set; } // For ordering colors (primary, secondary, etc.)
    }

    [Table("LureBackColors")]
    public class LureBackColorEntity
    {

        /// <summary>
        /// Override the default GetHashCode() so SelectedItem can find the matching item
        /// </summary>
        /// <returns>hashcode of <see cref="LureBackColorEntity"/></returns>
        public override int GetHashCode()
        {
            return HelperClass.CalculateHashCode(this,
                    () => this.Id);
            
                    //() => this.LureId,
                    //() => this.Color,
                    //() => this.DisplayOrder);
        }

        /// <summary>
        /// Override Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object? obj)
        {
            var item = obj as LureBackColorEntity;

            if (item == null)
            {
                return false;
            }

            // ?? String.Empty will return empty string if LureDataEntity is null (avoids a object is null error)
            return (this.Id).Equals(item.Id);

                    //&& (this.LureId).Equals(item.LureId)
                    //&& (this.Color).Equals(item.Color)
                    //&& (this.DisplayOrder).Equals(item.DisplayOrder);
        }

        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(typeof(LureDataEntity))]
        public Guid LureId { get; set; }

        public LureColor Color { get; set; } = LureColor.NA;

        //public int DisplayOrder { get; set; } // For ordering colors (primary, secondary, etc.)
    }
*/


    /// <summary>
    /// Represents all available Colors
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LureColor
    {
        NA = 0,

        // Basic Colors
        White = 1,
        Black = 2,
        Gray= 3,
        Silver = 4,

        // Primary Colors
        Red = 10,
        Blue = 11,
        Yellow = 12,
        Green = 13,

        // Secondary Colors
        Orange = 20,
        Purple = 21,
        Pink = 22,
        Brown = 23,

        // Extended Colors
        Cyan = 30,
        Magenta = 31,
        Lime = 32,
        Navy = 33,
        Teal = 34,
        Olive = 35,
        Maroon = 36,
        Aqua = 37,
        Fuchsia = 38,
        Gold = 39,
        Tan = 40,
        Beige = 41,
        Ivory = 42,
        Lavender = 43,
        Mint = 44,
        Peach = 45,
        Coral = 46,
        Salmon = 47,
        Crimson = 48,
        Indigo = 49,
        Violet = 50,
        Turquoise = 60,

        // Fluorescent Colors
        FluorescentPink = 100,
        FluorescentGreen = 101,
        FluorescentYellow = 102,
        FluorescentOrange = 103,
        FluorescentBlue = 104,
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
        public static readonly Dictionary<LureColor, Color> Colors = new()
        {
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
            { LureColor.Aqua, Color.FromArgb("#00FFFF") },
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
            return Colors.TryGetValue(LureColor, out var color) ? color : Colors[LureColor.White];
        }
    }
}