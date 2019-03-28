//DONE


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
    public class HomesAndVacations
    {
        public HomesAndVacations()
        {
            Console.WriteLine("Hello, World!");

            List<String> listing_urls = FetchListingUrls();
            List<Dictionary<String, String>> results = new List<Dictionary<String, String>>();

            foreach (String url in listing_urls)
            {
                Console.WriteLine(url);
                results.Add(ParseListing(url));
            }
            InsertIntoDB(results);

            Console.WriteLine("end of constructor");
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

                        foreach (KeyValuePair<String, String> kvp in dict)
                        {
                            keys += kvp.Key + ", ";
                            values += "\"" + kvp.Value + "\"" + ", ";
                        }
                        keys = keys.Substring(0, keys.Length - 2) + ")";
                        values = values.Substring(0, values.Length - 2) + ")";

                        sql = sql + keys + " VALUES " + values + ";";
                        Console.WriteLine(sql);

                        MySqlCommand cmd = new MySqlCommand(sql, cnn);
                        cmd.ExecuteNonQuery();

                    }
                    cnn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can not open connection ! ");
                    Console.WriteLine(ex.Message);
                }

            }
        }




        public Dictionary<String, String> ParseListing(String url)
        {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);

            //get city
            var cityNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeRoomsCity')]");
            String city = ParseCity(cityNode.InnerHtml);
            //get address
            var addressNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeName')]");
            String address = addressNode.InnerHtml;
            //get bedrooms
            var bedroomsNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeRoomsBeedrooms')]");
            String bedrooms = ParseRoom(bedroomsNode.InnerHtml);
            //get bathrooms
            var bathroomsNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeRoomsBathrooms')]");
            String bathrooms = ParseRoom(bathroomsNode.InnerHtml);
            //get rental type
            var rentalTypeNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeRoomsType')]");
            String rentalType = rentalTypeNode.InnerHtml;

            //get prices
            String dailyRate = "", weeklyRate = "";

            var dailyNode = listingPage.DocumentNode.SelectSingleNode("//div[contains(@class, 'ldgxPropBadgeRatesDaily')]");
            if (dailyNode != null) dailyRate = ParseRate(dailyNode.InnerHtml);
            var weeklyNode = listingPage.DocumentNode.SelectSingleNode("//div[contains(@class, 'ldgxPropBadgeRatesWeekly')]");
            if (weeklyNode != null) weeklyRate = ParseRate(weeklyNode.InnerHtml);

            //get images
            var imageNodes = listingPage.DocumentNode.SelectNodes("//div[contains(@class,'ldgxSlider') " +
                                                                  "and contains(@class, 'royalSlider')" +
                                                                  "and contains(@class, 'rsDefaultInv')]/a");
            String images = "[";
            foreach (var image in imageNodes)
            {
                images += image.Attributes["href"].Value + ", ";
                //Console.WriteLine(image.Attributes["href"].Value);
            }
            images = images.Substring(0, images.Length - 2) + "]";

            //get lat/llong
            var locationNode = listingPage.DocumentNode.SelectSingleNode("//div[contains(@class, 'ldgxTabContent') " +
                                                                         "and contains(@class, 'ldgxTabContentLocation')]");
            String lat = ParseLat(locationNode.InnerText);
            String lon = ParseLong(locationNode.InnerText);

            //insert everything into the dictionary
            Dictionary<String, String> dict = new Dictionary<String, String>();
            dict.Add("city", city);
            dict.Add("state", "South Carolina");
            //dict.Add("address", address);
            dict.Add("bedrooms", bedrooms);
            dict.Add("bathrooms", bathrooms);
            dict.Add("rental_type", rentalType);
            dict.Add("daily_rate", dailyRate);
            dict.Add("weekly_rate", weeklyRate);
            dict.Add("lat", lat);
            dict.Add("lon", lon);
            dict.Add("images", images);

            return dict;
        }

        public String ParseRate(String rate)
        {
            int dollarSignIndex = rate.IndexOf('$');
            rate = rate.Substring(dollarSignIndex);
            rate = rate.Substring(0, rate.IndexOf(' '));
            return rate;
        }
        public String ParseLat(String divText)
        {
            int index = divText.IndexOf("lat: ");
            String lat = divText.Substring(index);
            lat = lat.Substring(5, lat.IndexOf("\n") - 6);
            return lat;

        }
        public String ParseLong(String divText)
        {
            int index = divText.IndexOf("lon: ");
            String lon = divText.Substring(index);
            lon = lon.Substring(5, lon.IndexOf("\n") - 6);
            return lon;

        }

        public String ParseCity(String city)
        {
            city = city.Substring(city.IndexOf(' ') + 1);
            return city;
        }


        public String ParseRoom(String room)
        {
            if (room.Equals("Studio"))
            {
                return "1";
            }
            int spaceIndex = room.IndexOf(' ');
            if (spaceIndex == -1)
                return room;
            return room.Substring(0, spaceIndex);
        }


        public List<String> FetchListingUrls()
        {
            var html = "http://www.myrtlebeachhomesandvacations.com/vacation-rentals-2/";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'ldgxListingName')]/a");

            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                urls.Add(node.Attributes["href"].Value);
            }
            return urls;
        }
    }
}
