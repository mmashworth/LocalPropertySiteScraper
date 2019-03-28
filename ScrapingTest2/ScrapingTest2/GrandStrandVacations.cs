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
    public class GrandStrandVacations : LocalPropertySiteScraper
    {
        public GrandStrandVacations()
        {
            listingsUrl = "https://www.grandstrandvacations.com/vacation-rentals-list/";
            site = "Grand Strand Vacations";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["address"] = "//div[@class='vrp-row'][2]";
            fieldXPath["bedrooms"] = "//div[@class='vrp-row'][3]";
            fieldXPath["bathrooms"] = "//div[@class='vrp-row'][3]";
            fieldXPath["rentalType"] = "//div[@class='vrp-row'][1]";
            fieldXPath["dailyRate"] = "//table[@class='rate']/tr/td[3]";
            fieldXPath["weeklyRate"] = "//table[@class='rate']/tr/td[2]";
            fieldXPath["images"] = "//div[contains(@class, 'et_pb_gallery_image')]/a";
        }

        public override String ParseRentalType(String rentalTypeText) {
            int condoIndex = rentalTypeText.IndexOf("Condo");
            if (condoIndex == -1) return "";
            return "Condo";                 
        }

        public override String ParseAddress(String addressText) {
            return addressText.Trim().Replace("| ", "");
        }

        public override String ParseDailyRates(HtmlNodeCollection dailyRateNodes) {
            List<String> dailyRates = new List<String>();
            foreach (var rateNode in dailyRateNodes) {
                dailyRates.Add(rateNode.InnerHtml);
            }
            return ParseRates(dailyRates);
        }
        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes)
        {
            List<String> weeklyRates = new List<String>();
            foreach (var rateNode in weeklyRateNodes) {
                weeklyRates.Add(rateNode.InnerHtml);
            }
            return ParseRates(weeklyRates);
        }

        public override String ParseRates(List<String> rates) {
            if (rates.Count == 0) return "";
            int sum = 0;
            foreach (String rate in rates)
            {
                if (rate.Equals("N/A")) continue;
                String r = rate.Substring(0, rate.IndexOf(".")); //$x,xxx.00 to $x,xxx
                r = r.Substring(1).Replace(",", ""); //get rid of $ and commas
                sum += Int32.Parse(r);
            }
            if (sum == 0) return "";
            return "$" + (sum / rates.Count);
        }

        public override String ParseBedrooms(String bedroomsText) {
            String text = bedroomsText.Replace(" ", "").Replace("\t", "");
            int roomIndex = text.IndexOf("Bedroom");
            if (roomIndex == -1) return "";
            String rooms = text.Substring(roomIndex-1, 1);
            if (int.TryParse(rooms, out int x)) {
                return rooms;
            }
            else { return ""; }
        }
        public override String ParseBathrooms(String bathroomsText)
        {
            String text = bathroomsText.Replace(" ", "").Replace("\t", "");
            int roomIndex = text.IndexOf("Bathroom");
            if (roomIndex == -1) return "";
            String rooms = text.Substring(roomIndex - 1, 1);
            if (int.TryParse(rooms, out int x))
            {
                return rooms;
            }
            else { return ""; }
        }

        public override String ParseImages(HtmlNodeCollection imageNodes) {
            String imagesStr = "[";
            foreach (var image in imageNodes) {
                String image_url = image.Attributes["href"].Value;
                if (image_url != null) {
                    imagesStr += image_url + ", ";
                }
            }
            return imagesStr.Substring(0, imagesStr.Length - 2) + "]";
        }
                              
        public override List<String> FetchListingUrls(String url)
        {
            var html = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'search_area')]/ul/li/h5/a");
            List<String> urls = new List<String>();
            foreach (var node in nodes) {
                urls.Add(node.Attributes["href"].Value);
            }
            return urls;
        }                    
    }
}



