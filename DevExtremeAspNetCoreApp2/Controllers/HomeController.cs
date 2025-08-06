using Microsoft.AspNetCore.Mvc;
using System.Data;
//using Microsoft.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Text.Json;
using DevExtremeAspNetCoreApp2.Models;
using static DevExtremeAspNetCoreApp2.Data.ProductRepository;
using System.Data.SqlClient;

namespace DevExtremeAspNetCoreApp2.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Index() => View();

        //[HttpGet]
        //public IActionResult GetProducts()
        //{
        //    var data = ProductRepository.GetAll();
        //    return Json(data);
        //}

        //[HttpPost]
        //public IActionResult Insert(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        ProductRepository.Insert(product);
        //        return Json(product);
        //    }
        //    return BadRequest(ModelState);
        //}

        //[HttpPut]
        //public IActionResult Update(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        ProductRepository.Update(product);
        //        return Json(product);
        //    }
        //    return BadRequest(ModelState);
        //}

        //[HttpDelete]
        //public IActionResult Delete(int key)
        //{
        //    ProductRepository.Delete(key);
        //    return Ok();
        //}

        [HttpGet("GetData")]
        public IActionResult GetData()
        {
            return Json(OfficeSurveyRepository.GetAll());
        }

        [HttpPost("Insert")]
        public IActionResult Insert([FromBody] OfficeSurveyModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            OfficeSurveyRepository.Insert(model);
            return Json(model);
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] OfficeSurveyModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            OfficeSurveyRepository.Update(model);
            return Json(model);
        }

        [HttpDelete("Delete")]
        public IActionResult Delete(string key)
        {
            OfficeSurveyRepository.Delete(key);
            return Ok();
        }

        [HttpGet]
        public IActionResult GetCountries()
        {
            List<Country> countries = new List<Country>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "select distinct(StateProvince) from dbo.uv_OfficeInfo";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countries.Add(new Country
                        {
                            //Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["StateProvince"].ToString()
                        });
                    }
                }
            }

            return Ok(countries);
        }

        [HttpPost]
        public JsonResult OfficeAddress(string Country)
        {
            List<OfficeInfo> officeList = new List<OfficeInfo>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = @"SELECT OfficeID,CAST(OfficeID AS VARCHAR) + '-' + AddressLine1 + ', ' + City AS FullAddress 
                             FROM uv_OfficeInfo 
                             WHERE StateProvince = @Country";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Country", Country);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        officeList.Add(new OfficeInfo
                        {
                            OfficeID = reader["OfficeID"].ToString(),
                            FullAddress = reader["FullAddress"].ToString()
                        });
                    }
                }
            }

            // return View(officeList);
            // return Ok(officeList);
            return Json(officeList);
            //return Json(officeList, JsonRequestBehavior.AllowGet); // For MVC

        }

        public ActionResult OfficeDetails()
        {
            List<OfficeSurveyModel> officeList = new List<OfficeSurveyModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = @"SELECT *
                             FROM uv_OfficeInfo 
                             WHERE StateProvince = 'India'";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        officeList.Add(new OfficeSurveyModel
                        {
                            OfficeID = reader["OfficeID"].ToString()
                        });
                    }
                }
            }

            return View(officeList);
        }
        //[HttpPost("/GetOfficeDetails")]
        [HttpPost]
        public IActionResult GetOfficeDetails(string OfficeID)
        {
            var data = new List<Dictionary<string, object>>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM uv_OfficeInfo where OfficeID=@OfficeID", conn))
            {
                cmd.Parameters.AddWithValue("@OfficeID", OfficeID);
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = Enumerable.Range(0, reader.FieldCount)
                        .ToDictionary(reader.GetName, reader.GetValue);
                    data.Add(row);
                }
            }

            return Ok(data);
        }


        private object GetValue(Dictionary<string, object> dict, string key)
        {
            if (!dict.ContainsKey(key)) return DBNull.Value;

            var val = dict[key];
            if (val == null) return DBNull.Value;

            if (val is JsonElement je)
            {
                switch (je.ValueKind)
                {
                    case JsonValueKind.String:
                        return je.GetString();
                    case JsonValueKind.Number:
                        if (je.TryGetInt32(out int i)) return i;
                        if (je.TryGetInt64(out long l)) return l;
                        if (je.TryGetDouble(out double d)) return d;
                        return DBNull.Value;
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.Null:
                        return DBNull.Value;
                    default:
                        return je.ToString();
                }
            }
            else
            {
                return val;
            }
        }

        private object GetIntValue(Dictionary<string, object> dict, string key)
        {
            var val = GetValue(dict, key);
            if (val == DBNull.Value) return DBNull.Value;

            if (val is int || val is long) return val;

            if (int.TryParse(val.ToString(), out int i)) return i;

            return DBNull.Value;
        }

        [HttpPost]
        public IActionResult InsertOfficeDetails([FromBody] JsonElement values)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(values.ToString(), options);

            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Insert into Offices table
                using (var cmd = new SqlCommand("sp_OFFICES_INSERT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@OfficeID", GetSafeValue(data, "officeID"));
                    // Office Details
                    cmd.Parameters.AddWithValue("@OHPhase", GetSafeValue(data, "ohPhase"));
                    cmd.Parameters.AddWithValue("@OHPhase2", GetSafeValue(data, "ohPhase2"));
                    cmd.Parameters.AddWithValue("@OHPhase3", GetSafeValue(data, "ohPhase3"));
                    cmd.Parameters.AddWithValue("@OHPhase4", GetSafeValue(data, "ohPhase4"));
                    cmd.Parameters.AddWithValue("@VisionCode", GetSafeValue(data, "visionCode"));
                    cmd.Parameters.AddWithValue("@ADPWorkCode", GetSafeValue(data, "adpWorkCode"));
                    cmd.Parameters.AddWithValue("@VisionHeadCount", GetSafeInt(data, "visionHeadCount"));
                    cmd.Parameters.AddWithValue("@ADPHeadCount", GetSafeInt(data, "adpHeadCount"));
                    cmd.Parameters.AddWithValue("@Comments", GetSafeValue(data, "comments"));

                    // Office Profile Recycling Fields
                    cmd.Parameters.AddWithValue("@RecyclePaper", GetSafeValue(data, "recyclePaper"));
                    cmd.Parameters.AddWithValue("@RecyclePlastic", GetSafeValue(data, "RecyclePlastic"));
                    cmd.Parameters.AddWithValue("@RecycleGlass", GetSafeValue(data, "RecycleGlass"));
                    cmd.Parameters.AddWithValue("@RecycleAluminumCans", GetSafeValue(data, "RecycleAluminumCans"));
                    cmd.Parameters.AddWithValue("@RecycleCardboard", GetSafeValue(data, "RecycleCardboard"));
                    cmd.Parameters.AddWithValue("@RecycleBatteries", GetSafeValue(data, "RecycleBatteries"));
                    cmd.Parameters.AddWithValue("@RecycleEWaste", GetSafeValue(data, "RecycleEWaste"));
                    cmd.Parameters.AddWithValue("@RecycleCompost", GetSafeValue(data, "RecycleCompost"));
                    cmd.Parameters.AddWithValue("@WaterData", GetSafeValue(data, "WaterData"));
                    cmd.Parameters.AddWithValue("@EarthDay", GetSafeValue(data, "earthDay"));

                    // Office Profile Energy Fields
                    cmd.Parameters.AddWithValue("@EnergyThermostats", GetSafeValue(data, "energyThermostats"));
                    cmd.Parameters.AddWithValue("@EnergyStarAppliances", GetSafeValue(data, "energyStarAppliances"));
                    cmd.Parameters.AddWithValue("@EnergyBanSpaceHeaters", GetSafeValue(data, "energyBanSpaceHeaters"));
                    cmd.Parameters.AddWithValue("@EnergyCopiersAutoOff", GetSafeValue(data, "energyCopiersAutoOff"));
                    cmd.Parameters.AddWithValue("@EnergyLED_Lighting", GetSafeValue(data, "energyLED_Lighting"));
                    cmd.Parameters.AddWithValue("@EnergyMotionLighting", GetSafeValue(data, "energyMotionLighting"));
                    cmd.Parameters.AddWithValue("@EnergyEV_Chargers", GetSafeValue(data, "energyEV_Chargers"));
                    cmd.Parameters.AddWithValue("@EnergySolarOnsite", GetSafeValue(data, "energySolarOnsite"));

                    // Survey Date
                    cmd.Parameters.AddWithValue("@LastSurveyDate", GetSafeDate(data, "lastSurveyDate"));


                    cmd.ExecuteNonQuery();
                }

                // Insert into Office Profiles or Locations
                using (var cmd = new SqlCommand("sp_OFFICELOCATIONS_INSERT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Address Information
                    cmd.Parameters.AddWithValue("@AddressLine1", GetSafeValue(data, "addressLine1"));
                    cmd.Parameters.AddWithValue("@AddressLine2", GetSafeValue(data, "addressLine2"));
                    cmd.Parameters.AddWithValue("@AddressLine3", "");
                    cmd.Parameters.AddWithValue("@City", GetSafeValue(data, "city"));
                    cmd.Parameters.AddWithValue("@StateProvince", GetSafeValue(data, "stateProvince"));
                    cmd.Parameters.AddWithValue("@PostalCode", GetSafeValue(data, "postalCode"));
                    cmd.Parameters.AddWithValue("@Country", GetSafeValue(data, "country"));

                    cmd.Parameters.AddWithValue("@OfficeID", GetSafeValue(data, "officeID"));
                    cmd.Parameters.AddWithValue("@STATUS", GetSafeValue(data, "Status"));
                    cmd.Parameters.AddWithValue("@PrimFun", GetSafeValue(data, "PrimFun"));
                    cmd.Parameters.AddWithValue("@HTGFuel", GetSafeValue(data, "htgfuel"));
                    cmd.Parameters.AddWithValue("@YearBuilt", GetSafeValue(data, "yearBuilt"));
                    cmd.Parameters.AddWithValue("@NetOfficeSF", GetSafeValue(data, "netOfficeSF"));

                    cmd.Parameters.AddWithValue("@ClimateZone", GetSafeValue(data, "ClimateZone"));
                    cmd.Parameters.AddWithValue("@eGRIDSubregion", GetSafeValue(data, "eGRIDSubregion"));
                    cmd.Parameters.AddWithValue("@WarehouseSF", 1);
                    cmd.Parameters.AddWithValue("@OfficeSF",GetSafeValue(data, "officeSF"));

                    cmd.Parameters.AddWithValue("@LeaseCommenceDate", GetSafeDate(data, "leaseCommenceDate"));
                    cmd.Parameters.AddWithValue("@LeaseTermDate", GetSafeDate(data, "leaseTermDate"));
                    cmd.Parameters.AddWithValue("@LeaseExpirationDate", GetSafeDate(data, "leaseExpirationDate"));

                    cmd.Parameters.AddWithValue("@Acquisition", GetSafeValue(data, "acquisition"));
                    cmd.Parameters.AddWithValue("@Comments", GetSafeValue(data, "comments1"));
                    cmd.Parameters.AddWithValue("@EstimateElectricUsage", GetSafeValue(data, "EstimateElectricUsage"));
                    cmd.Parameters.AddWithValue("@EstimateGasUsage", GetSafeValue(data, "EstimateGasUsage"));
                    cmd.Parameters.AddWithValue("@EnergyDataSource", GetSafeValue(data, "energyDataSource"));



                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Office inserted successfully.");
        }

        private object GetVal(Dictionary<string, object> data, string key)
        {
            if (!data.TryGetValue(key, out var val) || val == null) return DBNull.Value;
            if (val is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Null || je.ValueKind == JsonValueKind.Undefined)
                    return DBNull.Value;
                return je.ToString();
            }
            return val;
        }

        private object GetIntVal(Dictionary<string, object> data, string key)
        {
            if (!data.TryGetValue(key, out var val) || val == null) return DBNull.Value;
            if (val is JsonElement je)
            {
                if (je.TryGetInt32(out var result)) return result;
                if (int.TryParse(je.ToString(), out var parsed)) return parsed;
            }
            if (int.TryParse(val.ToString(), out var finalVal)) return finalVal;
            return DBNull.Value;
        }


        [HttpPost]
        public IActionResult UpdateOfficeDetails(string key, string values)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var updatedFields = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(values, options);

            //  Fetch current row from DB
            var currentData = new Dictionary<string, object>();
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SELECT * FROM uv_OfficeInfo WHERE OfficeID = @OfficeID", conn))
            {
                cmd.Parameters.AddWithValue("@OfficeID", key);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            currentData[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                    }
                }
            }

            //  Merge updated fields into existing DB row
            foreach (var field in updatedFields)
            {
                currentData[field.Key] = ExtractValue(field.Value);
            }

            //  Pass everything (including OHPhase2) to stored procedure
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("sp_OFFICES_UPDATE", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                cmd.Parameters.AddWithValue("@OfficeID", key);
                cmd.Parameters.AddWithValue("@OHPhase", currentData.GetValueOrDefault("OHPhase") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OHPhase2", currentData.GetValueOrDefault("ohPhase2") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OHPhase3", currentData.GetValueOrDefault("OHPhase3") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OHPhase4", currentData.GetValueOrDefault("OHPhase4") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@VisionCode", currentData.GetValueOrDefault("VisionCode") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ADPWorkCode", currentData.GetValueOrDefault("ADPWorkCode") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@VisionHeadCount", GetIntValue(currentData, "VisionHeadCount"));
                cmd.Parameters.AddWithValue("@ADPHeadCount", GetIntValue(currentData, "ADPHeadCount"));
                cmd.Parameters.AddWithValue("@Comments", currentData.GetValueOrDefault("Comments") ?? DBNull.Value);

                // Office Profiles
                cmd.Parameters.AddWithValue("@RecyclePaper", currentData.GetValueOrDefault("RecyclePaper") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecyclePlastic", currentData.GetValueOrDefault("RecyclePlastic") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecycleGlass", currentData.GetValueOrDefault("RecycleGlass") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecycleAluminumCans", currentData.GetValueOrDefault("RecycleAluminumCans") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecycleCardboard", currentData.GetValueOrDefault("RecycleCardboard") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecycleBatteries", currentData.GetValueOrDefault("RecycleBatteries") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecycleEWaste", currentData.GetValueOrDefault("RecycleEWaste") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecycleCompost", currentData.GetValueOrDefault("RecycleCompost") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WaterData", currentData.GetValueOrDefault("WaterData") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EarthDay", currentData.GetValueOrDefault("EarthDay") ?? DBNull.Value);

                // Energy
                cmd.Parameters.AddWithValue("@EnergyThermostats", currentData.GetValueOrDefault("EnergyThermostats") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EnergyStarAppliances", currentData.GetValueOrDefault("EnergyStarAppliances") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EnergyBanSpaceHeaters", currentData.GetValueOrDefault("EnergyBanSpaceHeaters") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EnergyCopiersAutoOff", currentData.GetValueOrDefault("EnergyCopiersAutoOff") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EnergyLED_Lighting", currentData.GetValueOrDefault("EnergyLED_Lighting") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EnergyMotionLighting", currentData.GetValueOrDefault("EnergyMotionLighting") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EnergyEV_Chargers", currentData.GetValueOrDefault("EnergyEV_Chargers") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EnergySolarOnsite", currentData.GetValueOrDefault("EnergySolarOnsite") ?? DBNull.Value);


                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        //  Helper to convert JsonElement to real value
        private object ExtractValue(object val)
        {
            if (val == null) return DBNull.Value;
            if (val is JsonElement je)
            {
                return je.ValueKind switch
                {
                    JsonValueKind.String => je.GetString(),
                    JsonValueKind.Number when je.TryGetInt32(out var i) => i,
                    JsonValueKind.Number when je.TryGetDouble(out var d) => d,
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => DBNull.Value,
                    _ => je.ToString()
                };
            }
            return val;
        }

        private object GetSafeValue(Dictionary<string, object> data, string key)
        {
            // If dictionary itself is null or key not found
            if (data == null || !data.ContainsKey(key) || data[key] == null)
                return DBNull.Value;

            // Handle JsonElement values
            if (data[key] is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Null ||
                    je.ValueKind == JsonValueKind.Undefined ||
                    je.ValueKind == JsonValueKind.Object && je.GetRawText() == "null")
                    return DBNull.Value;

                var value = je.ToString();
                return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
            }

            // Handle regular .NET object
            var stringValue = data[key]?.ToString();
            return string.IsNullOrWhiteSpace(stringValue) ? DBNull.Value : data[key];
        }


        private object GetSafeInt(Dictionary<string, object> data, string key)
        {
            if (!data.ContainsKey(key)) return DBNull.Value;

            if (data[key] is JsonElement je && je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out var num))
                return num;

            if (int.TryParse(data[key]?.ToString(), out var parsed))
                return parsed;

            return DBNull.Value;
        }

        private object GetSafeDate(Dictionary<string, object> data, string key)
        {
            if (!data.ContainsKey(key)) return DBNull.Value;

            if (data[key] is JsonElement je && je.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(je.GetString(), out var date))
                return date;

            if (DateTime.TryParse(data[key]?.ToString(), out var parsed))
                return parsed;

            return DBNull.Value;
        }

        public OfficeSurveyModel GetOfficeDetails2(string officeId)
        {
            OfficeSurveyModel model = null;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT * FROM uv_OfficeInfo WHERE OfficeID = @OfficeID", conn))
                {
                    cmd.Parameters.AddWithValue("@OfficeID", officeId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var officeLocationId = Convert.ToInt32(reader["OfficeLocationID"]);

                            // Save to Session so you can use it later in other controllers
                            //Session["OfficeLocationID"] = officeLocationId;
                            TempData["OfficeLocationID"] = officeLocationId;
                            model = new OfficeSurveyModel
                            {
                                OfficeID = reader["OfficeID"].ToString(),
                                OfficeLocationID = Convert.ToInt32(reader["OfficeLocationID"]),
                                OHPhase = reader["OHPhase"].ToString(),
                                OHPhase2 = reader["OHPhase2"].ToString(),
                                OHPhase3 = reader["OHPhase3"].ToString(),
                                OHPhase4 = reader["OHPhase4"].ToString(),
                                VisionCode = reader["VisionCode"].ToString(),
                                ADPWorkCode = reader["ADPWorkCode"].ToString(),
                                VisionHeadCount = reader["VisionHeadCount"] != DBNull.Value ? Convert.ToInt32(reader["VisionHeadCount"]) : 0,
                                ADPHeadCount = reader["ADPHeadCount"] != DBNull.Value ? Convert.ToInt32(reader["ADPHeadCount"]) : 0,
                                Comments = reader["Comments"].ToString(),

                                RecyclePaper = reader["RecyclePaper"].ToString(),                                
                                RecyclePlastic = reader["RecyclePlastic"].ToString(),
                                RecycleGlass = reader["RecycleGlass"].ToString(),
                                RecycleAluminumCans = reader["RecycleAluminumCans"].ToString(),
                                RecycleCardboard = reader["RecycleCardboard"].ToString(),
                                RecycleBatteries = reader["RecycleBatteries"].ToString(),
                                RecycleEWaste = reader["RecycleEWaste"].ToString(),
                                RecycleCompost = reader["RecycleCompost"].ToString(),
                                WaterData = reader["WaterData"].ToString(),

                                RecyclePaperBool = string.Equals(reader["RecyclePaper"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                RecyclePlasticBool = string.Equals(reader["RecyclePlastic"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                recycleGlassBool = string.Equals(reader["RecycleGlass"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                RecycleAluminumCansBool = string.Equals(reader["RecycleAluminumCans"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                RecycleCardboardBool = string.Equals(reader["RecycleCardboard"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                RecycleBatteriesBool = string.Equals(reader["RecycleBatteries"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                RecycleEWasteBool = string.Equals(reader["RecycleEWaste"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                RecycleCompostBool = string.Equals(reader["RecycleCompost"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                WaterDataBool = string.Equals(reader["WaterData"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),

                                EarthDay = reader["EarthDay"].ToString(),

                                EnergyThermostats = reader["EnergyThermostats"].ToString(),
                                EnergyStarAppliances = reader["EnergyStarAppliances"].ToString(),
                                EnergyBanSpaceHeaters = reader["EnergyBanSpaceHeaters"].ToString(),
                                EnergyCopiersAutoOff = reader["EnergyCopiersAutoOff"].ToString(),
                                EnergyLED_Lighting = reader["EnergyLED_Lighting"].ToString(),
                                EnergyMotionLighting = reader["EnergyMotionLighting"].ToString(),
                                EnergyEV_Chargers = reader["EnergyEV_Chargers"].ToString(),
                                EnergySolarOnsite = reader["EnergySolarOnsite"].ToString(),

                                EnergyThermostatsBool = string.Equals(reader["EnergyThermostats"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EnergyStarAppliancesBool = string.Equals(reader["EnergyStarAppliances"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EnergyBanSpaceHeatersBool = string.Equals(reader["EnergyBanSpaceHeaters"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EnergyCopiersAutoOffBool = string.Equals(reader["EnergyCopiersAutoOff"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EnergyLED_LightingBool = string.Equals(reader["EnergyLED_Lighting"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EnergyMotionLightingBool = string.Equals(reader["EnergyMotionLighting"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EnergyEV_ChargersBool = string.Equals(reader["EnergyEV_Chargers"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EnergySolarOnsiteBool = string.Equals(reader["EnergySolarOnsite"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase), 

                                LastSurveyDate = reader["LastSurveyDate"].ToString(),

                                AddressLine1 = reader["AddressLine1"].ToString(),
                                AddressLine2 = reader["AddressLine2"].ToString(),
                                AddressLine3 = reader["AddressLine3"].ToString(),
                                City = reader["City"].ToString(),
                                StateProvince = reader["StateProvince"].ToString(),
                                PostalCode = reader["PostalCode"].ToString(),
                                Country = reader["Country"].ToString(),
                                Status = reader["Status"].ToString(),
                                PrimFun = reader["PrimFun"].ToString(),
                                HTGFuel = reader["HTGFuel"].ToString(),

                                YearBuilt = reader["YearBuilt"] != DBNull.Value ? Convert.ToInt32(reader["YearBuilt"]) : 0,
                                NetOfficeSF = reader["NetOfficeSF"] != DBNull.Value ? Convert.ToInt32(reader["NetOfficeSF"]) : 0,

                                ClimateZone = reader["ClimateZone"].ToString(),
                                EGRIDSubregion = reader["EGRIDSubregion"].ToString(),

                                WarehouseSF = reader["WarehouseSF"] != DBNull.Value ? Convert.ToInt32(reader["WarehouseSF"]) : 0,
                                OfficeSF = reader["OfficeSF"] != DBNull.Value ? Convert.ToInt32(reader["OfficeSF"]) : 0,

                                LeaseCommenceDate = reader["LeaseCommenceDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LeaseCommenceDate"]) : null,
                                LeaseTermDate = reader["LeaseTermDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LeaseTermDate"]) : null,
                                LeaseExpirationDate = reader["LeaseExpirationDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LeaseExpirationDate"]) : null,

                                Acquisition = reader["Acquisition"].ToString(),
                                //Comments1 = reader["Comments1"].ToString(),

                                EstimateElectricUsage = reader["EstimateElectricUsage"].ToString(),
                                EstimateElectricUsageBool = string.Equals(reader["EstimateElectricUsage"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EstimateGasUsage = reader["EstimateGasUsage"].ToString(),
                                EstimateGasUsageBool = string.Equals(reader["EstimateGasUsage"]?.ToString().Trim(), "True", StringComparison.OrdinalIgnoreCase),
                                EnergyDataSource = reader["EnergyDataSource"].ToString()
                            };
                        }
                    }
                }
            }

            return model;
        }

        public IActionResult Edit(string officeId)
        {
            var data = GetOfficeDetails2(officeId); // fetch data from DB
            return View(data); // pass model to view
        }
        public IActionResult New()
        {
            ViewData["Title"] = "NEW OFFICE INFORMATION";
            return View(); // pass model to view
        }

        [HttpPost]
        public IActionResult UpdateOffice([FromBody] OfficeSurveyModel model)
        {
            if (ModelState.IsValid)
            {
                SaveOfficeDetails(model); // your logic to save in DB
                return Ok(); // success
            }

            return BadRequest("Validation failed.");
        }

        public void SaveOfficeDetails(OfficeSurveyModel model)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                using (SqlCommand cmd = new SqlCommand("sp_OFFICES_UPDATE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parameters (add all based on your SP)
                    cmd.Parameters.AddWithValue("@OfficeID", model.OfficeID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OHPhase", model.OHPhase ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OHPhase2", model.OHPhase2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OHPhase3", model.OHPhase3 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OHPhase4", model.OHPhase4 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VisionCode", model.VisionCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ADPWorkCode", model.ADPWorkCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VisionHeadCount", model.VisionHeadCount);
                    cmd.Parameters.AddWithValue("@ADPHeadCount", model.ADPHeadCount);
                    cmd.Parameters.AddWithValue("@Comments", model.Comments ?? (object)DBNull.Value);

                    // Office Profiles

                    //cmd.Parameters.AddWithValue("@RecyclePaper", model.RecyclePaper ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RecyclePaper", model.RecyclePaperBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@RecyclePlastic", model.RecyclePlasticBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@RecycleGlass", model.recycleGlassBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@RecycleAluminumCans", model.RecycleAluminumCansBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@RecycleCardboard", model.RecycleCardboardBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@RecycleBatteries", model.RecycleBatteriesBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@RecycleEWaste", model.RecycleEWasteBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@RecycleCompost", model.RecycleCompostBool ? "True" : "False");

                    cmd.Parameters.AddWithValue("@WaterData", model.WaterDataBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EarthDay", model.EarthDay ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@EnergyThermostats", model.EnergyThermostatsBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EnergyStarAppliances", model.EnergyStarAppliancesBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EnergyBanSpaceHeaters", model.EnergyBanSpaceHeatersBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EnergyCopiersAutoOff", model.EnergyCopiersAutoOffBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EnergyLED_Lighting", model.EnergyLED_LightingBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EnergyMotionLighting", model.EnergyMotionLightingBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EnergyEV_Chargers", model.EnergyEV_ChargersBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EnergySolarOnsite", model.EnergySolarOnsiteBool ? "True" : "False");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand("sp_OFFICELOCATIONS_UPDATE", conn))
                {
                    int officeLocationId = TempData["OfficeLocationID"] != null
        ? (int)TempData["OfficeLocationID"]
        : 0;
                    cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.AddWithValue("@OfficeLocationID", model.OfficeLocationID);
                    cmd.Parameters.AddWithValue("@OfficeLocationID", officeLocationId);
                    cmd.Parameters.AddWithValue("@Status", model.Status ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PrimFun", model.PrimFun ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@HTGFuel", model.HTGFuel ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@YearBuilt", 1);
                    cmd.Parameters.AddWithValue("@NetOfficeSF", 1);
                    cmd.Parameters.AddWithValue("@ClimateZone", model.ClimateZone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@eGRIDSubregion", model.EGRIDSubregion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WarehouseSF", 1);
                    cmd.Parameters.AddWithValue("@OfficeSF", 1);
                    cmd.Parameters.AddWithValue("@LeaseCommenceDate", model.LeaseCommenceDate);
                    cmd.Parameters.AddWithValue("@LeaseExpirationDate", model.LeaseExpirationDate);
                    cmd.Parameters.AddWithValue("@LeaseTermDate", model.LeaseTermDate);
                    cmd.Parameters.AddWithValue("@Acquisition", model.Acquisition ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Comments", model.Comments ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine1", model.AddressLine1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine2", model.AddressLine2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine3", model.AddressLine3 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", model.City ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@StateProvince", model.StateProvince ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostalCode", model.PostalCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", model.Country ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EstimateElectricUsage", model.EstimateElectricUsageBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EstimateGasUsage", model.EstimateGasUsageBool ? "True" : "False");
                    cmd.Parameters.AddWithValue("@EnergyDataSource", model.EnergyDataSource ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastSurveyDate", model.LastSurveyDate);

                    //conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        //public int GetNextOfficeLocationId()
        //{
        //    string connectionString = _configuration.GetConnectionString("DefaultConnection");
        //    int nextId = 1; // default if table is empty

        //    using (SqlConnection con = new SqlConnection(connectionString))
        //    using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(OfficeLocationID), 0) + 1 FROM OfficeLocations", con))
        //    {
        //        con.Open();
        //        object result = cmd.ExecuteScalar();
        //        if (result != null)
        //        {
        //            nextId = Convert.ToInt32(result);
        //        }
        //    }

        //    return nextId;
        //}

        public List<string> GetDistinctPrimFun()
        {
            List<string> primFunList = new List<string>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = "SELECT DISTINCT PrimFun FROM uv_OfficeInfo";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        primFunList.Add(reader["PrimFun"].ToString());
                    }
                }
            }

            return primFunList;
        }

        [HttpGet]
        public JsonResult GetPrimFunList()
        {
            var primFunList = GetDistinctPrimFun();
            return Json(primFunList);
        }

        public List<string> GetDistinctClimateZone()
        {
            List<string> climateZoneList = new List<string>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = "SELECT DISTINCT ClimateZone FROM uv_OfficeInfo WHERE ClimateZone IS NOT NULL";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    climateZoneList.Add(reader["ClimateZone"].ToString());
                }
            }

            return climateZoneList;
        }

        [HttpGet]
        public JsonResult GetClimateZones()
        {
            var climateZones = GetDistinctClimateZone(); // method we wrote earlier
            return Json(climateZones);
        }
        public List<string> GetDistinctEgridSubregion()
        {
            var subregionList = new List<string>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = "SELECT DISTINCT eGRIDSubregion FROM uv_OfficeInfo WHERE ClimateZone IS NOT NULL";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        subregionList.Add(reader["eGRIDSubregion"].ToString());
                }
            }

            return subregionList;
        }

        [HttpGet]
        public JsonResult GetEgridSubregions()
        {
            var subregions = GetDistinctEgridSubregion();
            return Json(subregions);
        }

        public List<string> GetDistinctHtgFuel()
        {
            var fuelList = new List<string>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = "SELECT DISTINCT HTGFuel FROM uv_OfficeInfo WHERE ClimateZone IS NOT NULL";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        fuelList.Add(reader["HTGFuel"].ToString());
                }
            }

            return fuelList;
        }
        [HttpGet]
        public JsonResult GetHtgFuels()
        {
            var fuels = GetDistinctHtgFuel();
            return Json(fuels);
        }
        public List<string> GetDistinctEnergyDataSource()
        {
            var sourceList = new List<string>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = "SELECT DISTINCT EnergyDataSource FROM uv_OfficeInfo WHERE ClimateZone IS NOT NULL";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        sourceList.Add(reader["EnergyDataSource"].ToString());
                }
            }

            return sourceList;
        }

        [HttpGet]
        public JsonResult GetEnergyDataSources()
        {
            var sources = GetDistinctEnergyDataSource();
            return Json(sources);
        }



    }
}
