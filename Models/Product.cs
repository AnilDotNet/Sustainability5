using System.ComponentModel.DataAnnotations;

namespace DevExtremeAspNetCoreApp2.Models
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class OfficeSurveyModel
    {
        public string OfficeID { get; set; }
        public int OfficeLocationID { get; set; }
        public string OHPhase { get; set; }
        public string OHPhase2 { get; set; }
        public string OHPhase3 { get; set; }
        public string OHPhase4 { get; set; }
        public string VisionCode { get; set; }
        public string ADPWorkCode { get; set; }
        public int VisionHeadCount { get; set; }
        public int ADPHeadCount { get; set; }
        public string Comments { get; set; }

        public string RecyclePaper { get; set; }        
        public string RecyclePlastic { get; set; }        
        public string RecycleGlass { get; set; }
        public string RecycleAluminumCans { get; set; }
        public string RecycleCardboard { get; set; }
        public string RecycleBatteries { get; set; }
        public string RecycleEWaste { get; set; }
        public string RecycleCompost { get; set; }        
        public string WaterData { get; set; }

        public bool RecyclePaperBool { get; set; }
        public bool RecyclePlasticBool { get; set; }
        public bool recycleGlassBool { get; set; }
        public bool RecycleAluminumCansBool { get; set; }
        public bool RecycleCardboardBool { get; set; }
        public bool RecycleBatteriesBool { get; set; }
        public bool RecycleEWasteBool { get; set; }
        public bool RecycleCompostBool { get; set; }
        public bool WaterDataBool { get; set; }

        public string EarthDay { get; set; }

        public string EnergyThermostats { get; set; }
        public string EnergyStarAppliances { get; set; }
        public string EnergyBanSpaceHeaters { get; set; }
        public string EnergyCopiersAutoOff { get; set; }
        public string EnergyLED_Lighting { get; set; }
        public string EnergyMotionLighting { get; set; }
        public string EnergyEV_Chargers { get; set; }
        public string EnergySolarOnsite { get; set; }

        public bool EnergyThermostatsBool { get; set; }
        public bool EnergyStarAppliancesBool { get; set; }
        public bool EnergyBanSpaceHeatersBool { get; set; }
        public bool EnergyCopiersAutoOffBool { get; set; }
        public bool EnergyLED_LightingBool { get; set; }
        public bool EnergyMotionLightingBool { get; set; }
        public bool EnergyEV_ChargersBool { get; set; }
        public bool EnergySolarOnsiteBool { get; set; }

        public string LastSurveyDate { get; set; } // Optional: use DateTime? if preferred

        [StringLength(60)]
        public string AddressLine1 { get; set; }

        [StringLength(60)]
        public string AddressLine2 { get; set; }

        [StringLength(60)]
        public string AddressLine3 { get; set; }

        [StringLength(30)]
        public string City { get; set; }

        [StringLength(15)]
        public string StateProvince { get; set; }

        [StringLength(15)]
        public string PostalCode { get; set; }

        [StringLength(30)]
        public string Country { get; set; }

        [StringLength(10)]
        public string Status { get; set; }

        [StringLength(30)]
        public string PrimFun { get; set; }

        [StringLength(30)]
        public string HTGFuel { get; set; }

        public int YearBuilt { get; set; }

        public int NetOfficeSF { get; set; }

        [StringLength(10)]
        public string ClimateZone { get; set; }

        [StringLength(10)]
        public string EGRIDSubregion { get; set; }

        public int WarehouseSF { get; set; }

        public int OfficeSF { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LeaseCommenceDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LeaseTermDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LeaseExpirationDate { get; set; }

        [StringLength(50)]
        public string Acquisition { get; set; }

        [StringLength(4000)]
        public string Comments1 { get; set; }

        [StringLength(10)]
        public string EstimateElectricUsage { get; set; }
        public bool EstimateElectricUsageBool { get; set; }

        [StringLength(10)]
        public string EstimateGasUsage { get; set; }
        public bool EstimateGasUsageBool { get; set; }

        [StringLength(30)]
        public string EnergyDataSource { get; set; }
    }

    public class Country
    {
        //public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OfficeInfo
    {
        public string FullAddress { get; set; }
        public string OfficeID { get; set; }
        // public string StateProvince { get; set; }
        // Add other properties if needed from *
    }
}
