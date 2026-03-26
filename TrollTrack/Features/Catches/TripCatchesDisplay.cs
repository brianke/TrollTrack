using System.Collections.ObjectModel;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

/// <summary>
/// Display model for trip catches popup - binds trip info and catches list
/// </summary>
public class TripCatchesDisplay
{
    public Guid TripId { get; }
    public string TripName { get; }
    public DateTime TripDate { get; }
    /// <summary>True while the trip is still active (not ended).</summary>
    public bool IsActiveTrip { get; }
    /// <summary>Past (ended) trips can be deleted from this popup.</summary>
    public bool CanDeleteTrip => !IsActiveTrip;
    public int CatchesCount => Catches.Count;
    /// <summary>
    /// Summary of catches by species, e.g. "13 Walleye, 3 Perch, 1 Northern Pike"
    /// </summary>
    public string CatchesBySpeciesSummary => Catches == null || Catches.Count == 0
            ? "0 catches"
            : string.Join(", ", Catches
                .GroupBy(c => c.FishName)
                .OrderByDescending(g => g.Count())
                .Select(g => $"{g.Count()} {g.Key}"));

    public ObservableCollection<CatchDataEntity> Catches { get; }

    public TripCatchesDisplay(TripDataEntity trip, List<CatchDataEntity> catches)
    {
        TripId = trip.Id;
        TripName = trip.TripName;
        TripDate = trip.TripDate;
        IsActiveTrip = trip.IsActive;
        Catches = new ObservableCollection<CatchDataEntity>(
            catches.OrderByDescending(c => c.Timestamp));
    }
}
