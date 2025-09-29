using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Shared.Models
{
    public static class FishData
    {
        public static readonly List<FishInfoEntity> FishList = new()
        {
            new FishInfoEntity
            {
                Id = Guid.Parse("E1E25250-D8A4-4A4E-A443-EFE5411A456E"),
                CommonName = "Unknown",
                ScientificName = "Unknown",
                Habitat = "Unknown"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("C3CB4B07-EFDA-4FF7-8A15-42C4A7E3E5F2"),
                CommonName = "Walleye",
                ScientificName = "Sander vitreus",
                Habitat = "Freshwater lakes"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("C6349469-152F-4DCA-B87C-464E5F64C2C7"),
                CommonName = "Perch",
                ScientificName = "Perca flavescens",
                Habitat = "Freshwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("2DF05EC7-F99D-483A-9660-E2743C322EFF"),
                CommonName = "Cobia",
                ScientificName = "Rachycentron canadum",
                Habitat = "Saltwater"
            }
        };

        public static FishInfoEntity GetInfo(string fishCommonName = "Unknown")
        {
            return FishList.FirstOrDefault(x => x.CommonName == fishCommonName);
        }
    }

}
