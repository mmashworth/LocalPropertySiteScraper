// @nuget: HtmlAgilityPack

using System;
using System.Xml;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
//using System.Data.SqlClient;

using MySql.Data.MySqlClient;
using System.Data.SqlTypes;


namespace ScrapingTest2
{
    public class TurnKey_VR
    {
        public TurnKey_VR()
        {
            Console.WriteLine("Hello, World!");

            List<String> listing_urls = fetchListingUrls();
            Console.Write(listing_urls.Count.ToString());
            List<Dictionary<String, String>> results = new List<Dictionary<String, String>>();

            foreach (String url in listing_urls)
            {
                Console.WriteLine(url);
                //results.Add(parseListing(url));
            }
            //insertIntoDB(results);

            Console.WriteLine("end of constructor");

        }

        public Dictionary<String, String> parseListing(String url)
        {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);

            //get city
            var cityNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeRoomsCity')]");
            String city = parseCity(cityNode.InnerHtml);
            //get address
            var addressNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeName')]");
            String address = addressNode.InnerHtml;
            //get bedrooms
            var bedroomsNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeRoomsBeedrooms')]");
            String bedrooms = parseRoom(bedroomsNode.InnerHtml);
            //get bathrooms
            var bathroomsNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeRoomsBathrooms')]");
            String bathrooms = parseRoom(bathroomsNode.InnerHtml);
            //get rental type
            var rentalTypeNode = listingPage.DocumentNode.SelectSingleNode("//span[contains(@class, 'ldgxPropBadgeRoomsType')]");
            String rentalType = rentalTypeNode.InnerHtml;

            //get prices
            String dailyRate = "", weeklyRate = "", monthlyRate = "";

            var dailyNode = listingPage.DocumentNode.SelectSingleNode("//div[contains(@class, 'ldgxPropBadgeRatesDaily')]");
            if (dailyNode != null) dailyRate = parseRate(dailyNode.InnerHtml);
            var weeklyNode = listingPage.DocumentNode.SelectSingleNode("//div[contains(@class, 'ldgxPropBadgeRatesWeekly')]");
            if (weeklyNode != null) weeklyRate = parseRate(weeklyNode.InnerHtml);
            var monthlyNode = listingPage.DocumentNode.SelectSingleNode("//div[contains(@class, 'ldgxPropBadgeRatesMonthly')]");
            if (monthlyNode != null) monthlyRate = parseRate(monthlyNode.InnerHtml);


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
            String lat = parseLat(locationNode.InnerText);
            String lon = parseLong(locationNode.InnerText);

            //insert everything into the dictionary
            Dictionary<String, String> dict = new Dictionary<String, String>();
            dict.Add("city", city);
            dict.Add("address", address);
            dict.Add("bedrooms", bedrooms);
            dict.Add("bathrooms", bathrooms);
            dict.Add("rental_type", rentalType);
            dict.Add("daily_rate", dailyRate);
            dict.Add("weekly_rate", weeklyRate);
            dict.Add("monthly_rate", monthlyRate);
            dict.Add("lat", lat);
            dict.Add("lon", lon);
            dict.Add("images", images);

            return dict;
        }

        public List<String> fetchListingUrls() {
            var html = "https://www.turnkeyvr.com/vacation-rentals/south-carolina/myrtle-beach?showall";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls

            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'descriptionText')]/a");                                                                   
                                                       //"//div[contains(@class, 'ldgxTabContent') and contains(@class, 'ldgxTabContentLocation')]");

            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                //Console.WriteLine(node.InnerHtml);
                urls.Add("turnkeyvr.com" + node.Attributes["href"].Value);
            }
            return urls;
        }
    }
}
