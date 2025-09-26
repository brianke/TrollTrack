using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrollTrack.Converters
{
    /// <summary>
    /// Converts boolean auto-refresh state to button text
    /// </summary>
    public class AutoRefreshTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isEnabled)
            {
                return isEnabled ? "⏸️" : "▶️";
            }
            return "▶️";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean auto-refresh state to button color
    /// </summary>
    public class AutoRefreshColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isEnabled)
            {
                return isEnabled ? Colors.OrangeRed : Colors.Green;
            }
            return Colors.Green;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Inverts a boolean value for binding
    /// </summary>
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts an Enum value to a string representation
    /// </summary>
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            // If you want the enum's name, not the numeric value:
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, stringValue);
                }
                catch
                {
                    // Optionally handle parse failures or return a default value
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Convert string value to double
    /// </summary>
    public class StringToDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    string stringValue = reader.GetString();
                    if (string.IsNullOrWhiteSpace(stringValue))
                        return 0.0; // or throw new JsonException("Empty string cannot be converted to double");

                    if (double.TryParse(stringValue, out double result))
                        return result;

                    throw new JsonException($"Unable to convert '{stringValue}' to double");

                case JsonTokenType.Number:
                    return reader.GetDouble();

                case JsonTokenType.Null:
                    return 0.0; // or throw new JsonException("Null cannot be converted to double");

                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}