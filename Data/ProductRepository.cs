using DevExtremeAspNetCoreApp2.Models;

namespace DevExtremeAspNetCoreApp2.Data
{
    public class ProductRepository
    {
        static List<Product> _products = new List<Product> {
            new Product { ID = 1, Name = "Apple", Price = 1.25m },
            new Product { ID = 2, Name = "Banana", Price = 0.99m },
            new Product { ID = 3, Name = "Orange", Price = 1.75m },
        };

        public static List<Product> GetAll() => _products;

        public static void Insert(Product product)
        {
            product.ID = _products.Any() ? _products.Max(p => p.ID) + 1 : 1;
            _products.Add(product);
        }

        public static void Update(Product product)
        {
            var existing = _products.FirstOrDefault(p => p.ID == product.ID);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
            }
        }

        public static void Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.ID == id);
            if (product != null)
                _products.Remove(product);
        }


        public static class OfficeSurveyRepository
        {
            private static List<OfficeSurveyModel> _data = new List<OfficeSurveyModel>
    {
        new OfficeSurveyModel {
            OfficeID = "OFF-001",
            OHPhase = "1", OHPhase2 = "2", OHPhase3 = "3", OHPhase4 = "4",
            VisionCode = "VC001", ADPWorkCode = "ADP001",
            VisionHeadCount = 15, ADPHeadCount = 12,
            Comments = "Main office", RecyclePaper = "Yes", RecyclePlastic = "Yes",
            RecycleGlass = "No", RecycleAluminumCans = "Yes", RecycleCardboard = "Yes",
            RecycleBatteries = "No", RecycleEWaste = "Yes", RecycleCompost = "No",
            WaterData = "Metered", EarthDay = "Participated",
            EnergyThermostats = "Smart", EnergyStarAppliances = "Yes",
            EnergyBanSpaceHeaters = "Yes", EnergyCopiersAutoOff = "Enabled",
            EnergyLED_Lighting = "Yes", EnergyMotionLighting = "Yes",
            EnergyEV_Chargers = "2", EnergySolarOnsite = "Installed",
            LastSurveyDate = "2024-10-15",
            AddressLine1 = "123 Main St", AddressLine2 = "Suite 500",
            City = "New York", StateProvince = "NY", PostalCode = "10001", Country = "USA",
            Status = "Active", PrimFun = "HQ", HTGFuel = "Gas",
            YearBuilt = 2008, NetOfficeSF = 30000, ClimateZone = "CZ1",
            EGRIDSubregion = "NYUP", WarehouseSF = 5000, OfficeSF = 25000,
            LeaseCommenceDate = new DateTime(2020, 6, 1),
            LeaseTermDate = new DateTime(2030, 6, 1),
            LeaseExpirationDate = new DateTime(2035, 6, 1),
            Acquisition = "Lease", Comments1 = "Main office location",
            EstimateElectricUsage = "Low", EstimateGasUsage = "Medium",
            EnergyDataSource = "Smart Meter"
        }
    };

            public static List<OfficeSurveyModel> GetAll() => _data;

            public static void Insert(OfficeSurveyModel model)
            {
                model.OfficeID = "OFF-" + (_data.Count + 1).ToString("D3");
                _data.Add(model);
            }

            public static void Update(OfficeSurveyModel model)
            {
                var existing = _data.FirstOrDefault(x => x.OfficeID == model.OfficeID);
                if (existing != null)
                {
                    _data.Remove(existing);
                    _data.Add(model);
                }
            }

            public static void Delete(string officeId)
            {
                var model = _data.FirstOrDefault(x => x.OfficeID == officeId);
                if (model != null)
                {
                    _data.Remove(model);
                }
            }
        }

    }
}
