using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.MVVM.Models
{
    public enum FishCommonName
    {
        Walleye,
        Perch,
        Cobia
    }

    public record FishInfo(string CommonName, string ScientificName, string Habitat);


    public static class FishData
    {
        private static readonly Dictionary<FishCommonName, FishInfo> FishDictionary = new()
        {
            { FishCommonName.Walleye, new FishInfo(FishCommonName.Walleye.ToString(), "Sander vitreus", "Freshwater lakes") },
            { FishCommonName.Perch, new FishInfo(FishCommonName.Perch.ToString(), "Perca flavescens", "Freshwater") },
            { FishCommonName.Cobia, new FishInfo(FishCommonName.Cobia.ToString(), "Rachycentron canadum", "Saltwater") }
        };

        public static FishInfo GetInfo(this FishCommonName fish)
        {
            return FishDictionary.TryGetValue(fish, out var info) ? info :
                   new FishInfo(fish.ToString(), "Unknown", "Unknown");
        }
    }

}
