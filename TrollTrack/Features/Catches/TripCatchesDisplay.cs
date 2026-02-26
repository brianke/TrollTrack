using System.Collections.ObjectModel;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

/// <summary>
/// Display model for trip catches popup - binds trip info and catches list
/// </summary>
public class TripCatchesDisplay
{
    public string TripName { get; }
    public DateTime TripDate { get; }
    public int CatchesCount => Catches.Count;
    public ObservableCollection<CatchDataEntity> Catches { get; }

    public TripCatchesDisplay(TripDataEntity trip, List<CatchDataEntity> catches)
    {
        TripName = trip.TripName;
        TripDate = trip.TripDate;
        Catches = new ObservableCollection<CatchDataEntity>(
            catches.OrderByDescending(c => c.Timestamp));
    }
}
