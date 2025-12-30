using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrollTrack.Features.Shared;

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
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            // If you want the enum's name, not the numeric value:
            return value.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
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
    /// Converts an Enum value to use the [Description] property for display
    /// </summary>

    public class EnumToDisplayStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return enumValue.ToDisplayString();
            }
            return value?.ToString() ?? string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Not needed for display binding
            throw new NotImplementedException();
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
                    string? stringValue = reader.GetString(); // Use nullable string
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


    /// <summary>
    /// Converts a string comparison to a boolean value
    /// </summary>
    public class StringComparisonToBooleanConverter: IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // if either value is null then return the false value
            if (value == null || parameter == null) return false;

            return ((String)value).Equals((String)parameter, StringComparison.CurrentCulture) ? true : false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a string comparison to a visibility value
    /// </summary>
    public class StringComparisonToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // if either value is null then return the false value
            if (value == null || parameter == null) return Visibility.Hidden;

            return ((String)value).Equals((String)parameter, StringComparison.CurrentCulture) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GuidEqualsMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Must have exactly 2 values: itemGuid, primaryGuid
            if (values == null || values.Length < 2)
                return false;

            if (!TryGetGuid(values[0], out var itemGuid))
                return false;

            if (!TryGetGuid(values[1], out var primaryGuid))
                return false;

            // Optional: treat empty as "no primary"
            if (itemGuid == Guid.Empty || primaryGuid == Guid.Empty)
                return false;

            return itemGuid == primaryGuid;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static bool TryGetGuid(object? input, out Guid guid)
        {
            guid = Guid.Empty;

            if (input is null)
                return false;

            if (input is Guid g)
            {
                guid = g;
                return true;
            }

            if (input is string s && Guid.TryParse(s, out var parsed))
            {
                guid = parsed;
                return true;
            }

            return false;
        }
    }


    public class LureImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
                return null;

            // Handle res:foo.png
            if (path.StartsWith("res:", StringComparison.OrdinalIgnoreCase))
                return ImageSource.FromFile(path.Substring(4));

            // Handle file:/some/path or file:relative/path
            if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                path = path.Substring(5);

            // If relative, assume it lives under AppDataDirectory
            if (!Path.IsPathRooted(path))
                path = Path.Combine(FileSystem.AppDataDirectory, path);

            return ImageSource.FromFile(path);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }


    public class BindingProxy : BindableObject
    {
        public static readonly BindableProperty DataProperty =
            BindableProperty.Create(nameof(Data), typeof(object), typeof(BindingProxy), default(object));

        public object? Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
    }
}