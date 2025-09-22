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

    public record FishInfo(Guid Id, string CommonName, string ScientificName, string Habitat);


    public static class FishData
    {
        public static readonly Dictionary<FishCommonName, FishInfo> FishDictionary = new()
        {
            { FishCommonName.Walleye, new FishInfo(Guid.Parse("C3CB4B07-EFDA-4FF7-8A15-42C4A7E3E5F2"), FishCommonName.Walleye.ToString(), "Sander vitreus", "Freshwater lakes") },
            { FishCommonName.Perch, new FishInfo(Guid.Parse("C6349469-152F-4DCA-B87C-464E5F64C2C7"), FishCommonName.Perch.ToString(), "Perca flavescens", "Freshwater") },
            { FishCommonName.Cobia, new FishInfo(Guid.Parse("2DF05EC7-F99D-483A-9660-E2743C322EFF"), FishCommonName.Cobia.ToString(), "Rachycentron canadum", "Saltwater") }
        };

        public static FishInfo GetInfo(this FishCommonName fish)
        {
            return FishDictionary.TryGetValue(fish, out var info) ? info :
                   new FishInfo(Guid.Parse("8409180E-B84A-4980-A0B0-5F23FC4EF89B"), fish.ToString(), "Unknown", "Unknown");
        }
    }

}
