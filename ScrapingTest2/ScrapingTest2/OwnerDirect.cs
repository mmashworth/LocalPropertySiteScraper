//SITE: http://www.ownerdirectvacationrentals.net

using System;
using System.Xml;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

namespace ScrapingTest2
{
    public class OwnerDirect : LocalPropertySiteScraper
    {
        public OwnerDirect() {
            listingsUrl = "http://www.ownerdirectvacationrentals.net/DesktopDefault.aspx?page_num=1&PageID=33624";
            site = "Owner Direct Vacation Rentals";
            city = "North Myrtle Beach";

            fieldXPath["city"] = "//p[@id='location']/span[@class='locality']";
            fieldXPath["address"] = "//p[@id='location']/span[@class='street-address']";
            fieldXPath["bedrooms"] = "//p[@id='quickDescription']";
            fieldXPath["bathrooms"] = "//p[@id='quickDescription']";
            fieldXPath["rentalType"] = "//table[@id='unitAmenities']/tr[1]/td[2]";
            //fieldXPath["dailyRate"] = ;
            //fieldXPath["weeklyRate"] = ;
            fieldXPath["images"] = "//div[@class='item']/img";
        }





        public override String ParseImages(HtmlNodeCollection imageNodes) {
            String imagesStr = "[";
            foreach(var node in imageNodes) {
                imagesStr += node.Attributes["src"].Value.Substring(2) + ", ";
            }
            imagesStr = imagesStr.Substring(0, imagesStr.Length - 2) + "]";
            return imagesStr;
        }
        public override String ParseCity(String cityText) {
            return cityText;
        }
        public override String ParseAddress(String addressText) {
            return addressText.Trim();
        }
        public override String ParseBedrooms(String bedroomsText) {
            var myRegex = new Regex(@"(\d{1,2}) bedrooms");
            Match match = myRegex.Match(bedroomsText);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return "";//no match
        }
        public override String ParseBathrooms(String bathroomsText) {
            var myRegex = new Regex(@"(\d{1,2}\.\d|\d{1,2}) bathroom");
            Match match = myRegex.Match(bathroomsText);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return "";//no match
        }
        public override String ParseRentalType(String rentalTypeText) {
            var myRegex = new Regex(@"([^\,]*)");
            Match match = myRegex.Match(rentalTypeText);
            if (match.Success) {
                return match.Groups[1].Value.Trim();
            }
            return ""; //no match
        }

        public override List<String> FetchListingUrls(String url) {
            List<String> urls = new List<string>();

            for (int i = 1; i <= 8; i++) {
                url = "http://www.ownerdirectvacationrentals.net/DesktopDefault.aspx?page_num=" + i.ToString() + "&PageID=33624";
                var html = url;
                HtmlWeb web = new HtmlWeb();
                var htmlDoc = web.Load(html);
                var nodes = htmlDoc.DocumentNode.SelectNodes("//ul[@id='propertyList']/li/div[@class='body']/a");
                foreach (var node in nodes)
                {
                    Console.WriteLine("http://www.ownerdirectvacationrentals.net" + node.Attributes["href"].Value);
                    urls.Add("http://www.ownerdirectvacationrentals.net" + node.Attributes["href"].Value);
                }
            }
            return urls;
        }


    }
}
