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
                Id = Guid.Parse("A1B2C3D4-1111-4A00-B000-000000000001"),
                CommonName = "King Salmon (Chinook)",
                ScientificName = "Oncorhynchus tshawytscha",
                Habitat = "Freshwater / Saltwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-2222-4A00-B000-000000000002"),
                CommonName = "Coho Salmon (Silver)",
                ScientificName = "Oncorhynchus kisutch",
                Habitat = "Freshwater / Saltwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-3333-4A00-B000-000000000003"),
                CommonName = "Atlantic Salmon",
                ScientificName = "Salmo salar",
                Habitat = "Freshwater / Saltwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-4444-4A00-B000-000000000004"),
                CommonName = "Pink Salmon",
                ScientificName = "Oncorhynchus gorbuscha",
                Habitat = "Freshwater / Saltwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-5555-4A00-B000-000000000005"),
                CommonName = "Sockeye Salmon",
                ScientificName = "Oncorhynchus nerka",
                Habitat = "Freshwater / Saltwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-6666-4A00-B000-000000000006"),
                CommonName = "Rainbow Trout",
                ScientificName = "Oncorhynchus mykiss",
                Habitat = "Freshwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-7777-4A00-B000-000000000007"),
                CommonName = "Brown Trout",
                ScientificName = "Salmo trutta",
                Habitat = "Freshwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-8888-4A00-B000-000000000008"),
                CommonName = "Lake Trout",
                ScientificName = "Salvelinus namaycush",
                Habitat = "Freshwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-9999-4A00-B000-000000000009"),
                CommonName = "Brook Trout",
                ScientificName = "Salvelinus fontinalis",
                Habitat = "Freshwater"
            },
            new FishInfoEntity
            {
                Id = Guid.Parse("A1B2C3D4-AAAA-4A00-B000-00000000000A"),
                CommonName = "Steelhead",
                ScientificName = "Oncorhynchus mykiss",
                Habitat = "Freshwater / Saltwater"
            }
        };

        public static FishInfoEntity GetInfoFromName(string fishCommonName)
        {
            return FishList.FirstOrDefault(x => x.CommonName == fishCommonName) ?? FishList.First(x => x.CommonName == "Unknown");
        }

        public static FishInfoEntity GetInfoFromId(Guid fishGuid)
        {
            return FishList.FirstOrDefault(x => x.Id == fishGuid) ?? FishList.First(x => x.CommonName == "Unknown");
        }

        public static List<string> GetAllFishNames()
        {
            return FishList.Where(f => f.CommonName != "Unknown").Select(f => f.CommonName).ToList();
        }

        public static string GetFishNameById(Guid fishInfoId)
        {
            return FishList.FirstOrDefault(f => f.Id == fishInfoId)?.CommonName ?? "Unknown";
        }
    }

}
