// @nuget: HtmlAgilityPack

using System;
using System.Xml;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;

namespace ScrapingTest2
{
    abstract public class LocalPropertySiteScraper
    {
        protected String listingsUrl;

        protected List<String> months = new List<String>() { "JANUARY", "FEBRUARY", "MARCH", "APRIL", "MAY", "JUNE", "JULY", "AUGUST", "SEPTEMBER", "OCTOBER", "NOVEMBER", "DECEMBER" };

        protected Dictionary<String, String> nightsUnavailable = new Dictionary<String, String>() {
            {"JAN", null}, {"FEB", null}, {"MAR", null}, {"APR", null}, {"MAY", null}, {"JUN", null}, 
            {"JUL", null}, {"AUG", null}, {"SEP", null}, {"OCT", null}, {"NOV", null}, {"DEC", null},
        };


        protected Dictionary<String, String> fieldXPath = new Dictionary<String, String>() {
            {"city", "//garbagePath123"},
            {"state", "//garbagePath123"},
            {"address", "//garbagePath123"},
            {"bedrooms", "//garbagePath123"},
            {"bathrooms", "//garbagePath123"},
            {"rentalType", "//garbagePath123"},
            {"dailyRate", "//garbagePath123"},
            {"weeklyRate", "//garbagePath123"},
            {"lat", "//garbagePath123"},
            {"lon", "//garbagePath123"},
            {"images", "//garbagePath123"},
        };

        //database fields
        protected String site = "";
        protected String url = "";
        protected String city = "";
        protected String state = "";
        protected String address = "";
        protected String bedrooms = "";
        protected String bathrooms = "";
        protected String rentalType = "";
        protected String dailyRate = "";
        protected String weeklyRate = "";
        protected String lat = "";
        protected String lon = "";
        protected String images = "";

        public virtual String ParseCity(String cityText) { return city; }
        public virtual String ParseState(String stateText) { return state; }
        public virtual String ParseAddress(String addressText) { return address; }
        public virtual String ParseBedrooms(String bedroomsText) { return bedrooms; }
        public virtual String ParseBathrooms(String bathroomsText) { return bathrooms; }
        public virtual String ParseRentalType(String rentalTypeText) { return rentalType; }
        public virtual String ParseDailyRates(HtmlNodeCollection dailyRateNodes) { return ""; }
        public virtual String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes) { return ""; }
        public virtual String ParseLat(String latText) { return lat; }
        public virtual String ParseLong(String longText) { return lon; }
        public virtual String ParseImages(HtmlNodeCollection imageNodes) { return ""; }


        //Begin Methods
        public virtual void Scrape() //may need to change for certain sites
        {
            List<String> listing_urls = FetchListingUrls(listingsUrl); //get url listings
            List<Dictionary<String, String>> results = new List<Dictionary<String, String>>(); //
            foreach (String listing_url in listing_urls)
            {
                Dictionary<String, String> result = ParseListing(listing_url);
                if (result != null)
                    results.Add(result);
            }
            InsertIntoDB(results);
            Console.WriteLine("Inserted " + results.Count + " entries.");
        }

        abstract public List<String> FetchListingUrls(String url);

        virtual public void GetNumberOfNightsUnavailable(String url) {  }

        public virtual Dictionary<String, String> ParseListing(String url)
        {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);

            Console.WriteLine("-----------------------------------------------------------------------------------------------");
            Console.WriteLine("PARSING: " + url);

            var cityNode = listingPage.DocumentNode.SelectSingleNode(fieldXPath["city"]);
            if (cityNode != null) { city = ParseCity(cityNode.InnerHtml); }

            var stateNode = listingPage.DocumentNode.SelectSingleNode(fieldXPath["state"]);
            if (stateNode != null) { state = ParseState(stateNode.InnerHtml); }

            var addressNode = listingPage.DocumentNode.SelectSingleNode(fieldXPath["address"]);
            if (addressNode != null) { address = ParseAddress(addressNode.InnerHtml); }
            //else Console.WriteLine("\t address NULL");

            var bedroomsNode = listingPage.DocumentNode.SelectSingleNode(fieldXPath["bedrooms"]);
            if (bedroomsNode != null) { bedrooms = ParseBedrooms(bedroomsNode.InnerHtml); }
            //else Console.WriteLine("\t bedrooms NULL");

            var bathroomsNode = listingPage.DocumentNode.SelectSingleNode(fieldXPath["bathrooms"]);
            if (bathroomsNode != null) { bathrooms = ParseBathrooms(bathroomsNode.InnerHtml); }
            //else Console.WriteLine("\t bathrooms NULL");

            var rentalTypeNode = listingPage.DocumentNode.SelectSingleNode(fieldXPath["rentalType"]);
            if (rentalTypeNode != null) { rentalType = ParseRentalType(rentalTypeNode.InnerHtml); }
            //else Console.WriteLine("\t rentalType NULL");

            HtmlNodeCollection dailyRateNodes = listingPage.DocumentNode.SelectNodes(fieldXPath["dailyRate"]);
            if (dailyRateNodes != null) { dailyRate = ParseDailyRates(dailyRateNodes); }
            //else Console.WriteLine("\t dailyRate NULL");

            HtmlNodeCollection weeklyRateNodes = listingPage.DocumentNode.SelectNodes(fieldXPath["weeklyRate"]);
            if (weeklyRateNodes != null) { weeklyRate = ParseWeeklyRates(weeklyRateNodes); }
            //else Console.WriteLine("\t weeklyRate NULL");

            var latNode = listingPage.DocumentNode.SelectSingleNode(fieldXPath["lat"]);
            if (latNode != null) { lat = ParseLat(latNode.InnerHtml); }

            var longNode = listingPage.DocumentNode.SelectSingleNode(fieldXPath["lon"]);
            if (longNode != null) { lon = ParseLong(longNode.InnerHtml); }

            HtmlNodeCollection imageNodes = listingPage.DocumentNode.SelectNodes(fieldXPath["images"]);
            if (imageNodes != null) { images = ParseImages(imageNodes); }
            else Console.WriteLine("\t images NULL");


            //insert fields into dictionary and return
            Dictionary<String, String> dict = new Dictionary<String, String>();
            dict.Add("site", site);
            dict.Add("url", url);
            dict.Add("city", city);
            dict.Add("state", state);

            dict.Add("address", address);
            dict.Add("bedrooms", bedrooms);
            dict.Add("bathrooms", bathrooms);
            dict.Add("rental_type", rentalType);
            dict.Add("daily_rate", dailyRate);
            dict.Add("weekly_rate", weeklyRate);
            dict.Add("lat", lat);
            dict.Add("lon", lon);
            dict.Add("images", images);

            GetNumberOfNightsUnavailable(url);

            return dict;
        }


        public void InsertIntoDB(List<Dictionary<String, String>> results)
        {
            using (MySqlConnection conn = new MySqlConnection())
            {
                MySqlConnection cnn;
                String connectionString = "Server=localhost;port=3306;Database=local_sites;Uid=root;Pwd=orange212;SslMode=none";
                cnn = new MySqlConnection(connectionString);

                try
                {
                    cnn.Open();

                    String sql = "";
                    String keys = "";
                    String values = "";
                    foreach (Dictionary<String, String> dict in results)
                    {
                        sql = "INSERT INTO scraped_info ";
                        keys = "(";
                        values = "(";

                        foreach (KeyValuePair<String, String> kvp in dict) {
                            keys += kvp.Key + ", ";
                            values += "\"" + kvp.Value + "\"" + ", ";
                        }

                        keys = keys.Substring(0, keys.Length - 2) + ")";
                        values = values.Substring(0, values.Length - 2) + ")";
                        sql = sql + keys + " VALUES " + values + ";";

                        MySqlCommand cmd = new MySqlCommand(sql, cnn);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine(sql);

                    }
                    cnn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not connect to MySQL");
                    Console.WriteLine(ex.Message);
                }

            }
        }

        //gets a list of strings of the form $xxx, $xxx.00, $x,xxx.00, or $x,xxx
        //and returns the average of these ints in a string
        public virtual String ParseRates(List<String> rates) {
            if (rates.Count == 0) return "";
            int sum = 0;
            foreach (String rate in rates)
            {
                String r = rate.Replace(",", "");
                r = r.Substring(r.IndexOf("$") + 1);
                if (r.IndexOf('.') != -1) r = r.Substring(0, r.IndexOf('.'));
                sum += Int32.Parse(r); //these rates are for a 2 night stay, so divide by 2
            }
            return (sum / rates.Count).ToString();
        }

        public String getMonth(String monthText) {
            monthText = monthText.ToUpper();
            foreach(String m in months) {
                if (monthText.IndexOf(m) != -1) return m.Substring(0, 3);
            }
            foreach (String m in months) {
                if (monthText.IndexOf(m.Substring(0,3)) != -1) return m.Substring(0, 3);
            }
            return null;
        }
    }
}