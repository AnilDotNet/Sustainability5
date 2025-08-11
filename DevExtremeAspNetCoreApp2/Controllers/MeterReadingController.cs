using DevExtremeAspNetCoreApp2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace DevExtremeAspNetCoreApp2.Controllers
{
    public class MeterReadingController : Controller
    {
        private readonly IConfiguration _configuration;

        public MeterReadingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult OfficeAddress(string Country)
        {
            List<OfficeInfo> officeList = new List<OfficeInfo>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = @"SELECT OfficeID,CAST(OHPhase AS VARCHAR) +'-' + CAST(OfficeID AS VARCHAR) + '-' + AddressLine1 + ', ' + City AS FullAddress 
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
        public IActionResult GetMeterReadingInfo(string OfficeID)
        {
            var data = new List<Dictionary<string, object>>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM vw_utilitymeterreadinginfo where OfficeID=@OfficeID order by StartDate desc", conn))
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
    }
}
