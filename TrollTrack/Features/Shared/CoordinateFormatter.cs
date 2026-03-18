namespace TrollTrack.Features.Shared;

/// <summary>
/// Converts decimal degrees to degrees, minutes, seconds (DMS) format.
/// </summary>
public static class CoordinateFormatter
{
    /// <summary>
    /// Converts decimal degrees to degrees, minutes, seconds format.
    /// </summary>
    /// <param name="coordinate">The decimal degree coordinate</param>
    /// <param name="isLatitude">True for latitude (N/S), false for longitude (E/W)</param>
    /// <returns>Formatted coordinate string (e.g. "47° 36' 37.2\" N")</returns>
    public static string ToDegreesMinutesSeconds(double coordinate, bool isLatitude)
    {
        if (coordinate == 0) return isLatitude ? "0° 0' 0\" N" : "0° 0' 0\" W";

        var direction = isLatitude
            ? (coordinate >= 0 ? "N" : "S")
            : (coordinate >= 0 ? "E" : "W");

        coordinate = Math.Abs(coordinate);
        int degrees = (int)coordinate;
        double remainderAfterDegrees = coordinate - degrees;
        int minutes = (int)(remainderAfterDegrees * 60);
        double remainderAfterMinutes = (remainderAfterDegrees * 60) - minutes;
        double seconds = remainderAfterMinutes * 60;

        return $"{degrees}° {minutes}' {seconds:F1}\" {direction}";
    }
}
