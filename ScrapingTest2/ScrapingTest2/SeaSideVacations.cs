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
    public class SeaSideVacations : LocalPropertySiteScraper
    {
        public SeaSideVacations()
        {
            listingsUrl = "https://www.seasidevacations.com/north-myrtle-beach-vacation-rentals";
            site = "Sea Side Vacations";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["city"] = "//div[contains(@class, 'span12')]/h4";
            fieldXPath["address"] = "//div[contains(@class, 'span12')]/h4";
            fieldXPath["bedrooms"] = "//div[contains(@class, 'span12')]/h4";
            fieldXPath["bathrooms"] = "//div[contains(@class, 'span12')]/h4";
            fieldXPath["rentalType"] = "//div[contains(@class, 'span12')]/h4";
            fieldXPath["dailyRate"] = "//div[contains(@id, 'rentalrates')]/table/tr";
            fieldXPath["weeklyRate"] = "//div[contains(@id, 'rentalrates')]/table/tr";
            fieldXPath["images"] = "//ul[contains(@class,'thumbnails')]/li[contains(@class,'span3')]/a/img";
        }


        public override void GetNumberOfNightsUnavailable(String url)
        {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);
            var tables = listingPage.DocumentNode.SelectNodes("//div[contains(@class, 'b-calendar')]/div[contains(@class, 'span4')]");

            foreach (var table in tables) {
                String captionXPath = table.XPath + "/div/div/strong";
                String caption = listingPage.DocumentNode.SelectSingleNode(captionXPath).InnerText;
                String month = getMonth(caption);
                //Console.WriteLine(month);

                String unavailableXPath = table.XPath + "/table/tr/td[contains(@class, 'booked')]";
                HtmlNodeCollection unavailable = listingPage.DocumentNode.SelectNodes(unavailableXPath);
                if (unavailable != null) {
                    nightsUnavailable[month] = unavailable.Count.ToString();
                }
            }
            /*
            foreach(KeyValuePair<String, String> kvp in nightsUnavailable) {
                Console.WriteLine(kvp.Key + ": " + kvp.Value);
            }
            */
        }

        public override String ParseImages(HtmlNodeCollection imageNodes) {
            String imagesStr = "[";
            foreach (var imageNode in imageNodes)
            {
                imagesStr += imageNode.Attributes["src"].Value + ", ";
            }
            return imagesStr.Substring(0, imagesStr.Length - 2) + "]";
        }


        public override String ParseDailyRates(HtmlNodeCollection dailyRateNodes) {
            List<String> dailyRates = new List<String>();
            foreach (var rateNode in dailyRateNodes)
            {
                if (rateNode.InnerHtml.IndexOf("<th>") != -1) continue; //skip header row
                dailyRates.Add(ParseTableForDailyRates(rateNode.InnerHtml));
            }
            return ParseRates(dailyRates);
        }
        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes)
        {
            List<String> weeklyRates = new List<String>();
            foreach (var rateNode in weeklyRateNodes)
            {
                if (rateNode.InnerHtml.IndexOf("<th>") != -1) continue; //skip header row
                weeklyRates.Add(ParseTableForWeeklyRates(rateNode.InnerHtml));
            }
            return ParseRates(weeklyRates);
        }
        public String ParseRates(List<String> rates){
            if (rates.Count == 0) return "";
            int sum = 0;
            foreach(String rate in rates) {
                if (rate.Trim().Equals("")) continue; //no price listed
                String rateAsInt = rate.Substring(1, rate.IndexOf(".") - 1).Replace(",", "");//remove $, cents, and commas
                sum += Int32.Parse( rateAsInt ); 
            }
            return "$" + (sum / rates.Count).ToString();
        }
        public String ParseTableForDailyRates(String table) {
            table = table.Substring(table.IndexOf("<td>") + 1); //start date column
            table = table.Substring(table.IndexOf("<td>") + 1); //end date column
            table = table.Substring(table.IndexOf("<td>") + 4); //daily rate
            return table.Substring(0, table.IndexOf("</td>"));
        }

        public String ParseTableForWeeklyRates(String table) {
            table = table.Substring(table.IndexOf("<td>") + 1); //start date column
            table = table.Substring(table.IndexOf("<td>") + 1); //end date column
            table = table.Substring(table.IndexOf("<td>") + 1); //daily rate
            table = table.Substring(table.IndexOf("<td>") + 4); //weekly rate
            return table.Substring(0, table.IndexOf("</td>"));
        }



        public override String ParseRentalType(String type) {
            if (type.IndexOf("Condo") != -1) return "Condo";
            if (type.IndexOf("House") != -1) return "House";
            return "";
        }
        public override String ParseBedrooms(String bedroom) {
            return bedroom.Substring(bedroom.IndexOf("Bed")-2, 1);
        }
        public override String ParseBathrooms(String bathroom) {
            return bathroom.Substring(bathroom.IndexOf("/")+2, 1);
        }
        public override String ParseCity(String city) {
            if (city.IndexOf("North Myrtle Beach") == -1) return "";
            return "North Myrtle Beach";
        }

        public override String ParseAddress(String address) {
            address = address.Substring(address.IndexOf("<br>")+4);
            address = address.Substring(0).Trim();
            return address;
        }





        public override List<String> FetchListingUrls(String url)
        {
            var housesHtml = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(housesHtml);

            //HOME URLS
            var nodes = htmlDoc.DocumentNode.SelectNodes("//ul[contains(@class, 'resort-list')]/li/a");

            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                urls.Add("https://www.seasidevacations.com" + node.Attributes["href"].Value);
            }

            //CONDO URLS
            var condosHtml = "https://www.seasidevacations.com/north-myrtle-beach-condo-rentals";
            htmlDoc = web.Load(condosHtml);
            var allCondosNodes = htmlDoc.DocumentNode.SelectNodes("//ul[contains(@class, 'resort-list')]/li/a");
            foreach (var node in allCondosNodes)
            {
                //Console.WriteLine(node.Attributes["href"].Value);
                htmlDoc = web.Load("https://www.seasidevacations.com" + node.Attributes["href"].Value);
                var condoNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'well')]/div/div[contains(@class, 'span9')]/h3/a");
                foreach (var condoNode in condoNodes)
                {
                    urls.Add("https://www.seasidevacations.com" + condoNode.Attributes["href"].Value);
                }
            }
            return urls;
        }
    }
}






