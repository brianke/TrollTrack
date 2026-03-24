using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public interface ITripService
    {
        TripDataEntity? ActiveTrip { get; }
        event EventHandler<TripDataEntity?> ActiveTripChanged;

        Task<bool> StartTripAsync(string tripName);
        Task EndTripAsync();
        Task<List<TripDataEntity>> GetAllTripsAsync();
        Task<TripDataEntity?> GetTripByIdAsync(Guid id);
    }
}