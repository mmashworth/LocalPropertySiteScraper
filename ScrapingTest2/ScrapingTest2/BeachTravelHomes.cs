//DONE
//SITE: https://www.northmyrtlebeachtravel.com

using System;
using System.Xml;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;

namespace ScrapingTest2
{
    public class BeachTravelHomes : LocalPropertySiteScraper
    {
        public BeachTravelHomes()
        {
            listingsUrl = "https://www.northmyrtlebeachtravel.com/rental/alpha.html";
            site = "North Myrtle Beach Travel";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["city"] = "//div[contains(@class, 'houseAddress')]/span[contains(@class, 'houseAddress')][2]";
            fieldXPath["address"] = "//div[contains(@class, 'houseAddress')]/span[contains(@class, 'houseAddress')][1]";
            fieldXPath["bedrooms"] = "//div[contains(@class, 'houseDetailBar')]/span[contains(@class, 'housespecs')][4]";
            fieldXPath["bathrooms"] = "//div[contains(@class, 'houseDetailBar')]/span[contains(@class, 'housespecs')][5]";
            fieldXPath["dailyRate"] = "//table[contains(@class, 'ratestable')]/tr";
            fieldXPath["weeklyRate"] = "//table[contains(@class, 'ratestable')]/tr";
            fieldXPath["images"] = "//div[contains(@id, 'thumbimages')]/div[contains(@id, 'scrollarea')]/img";
        }

        public override List<String> FetchListingUrls(String url) {
            List<String> homes = FetchHomeListingUrls(url);
            List<String> condos = FetchCondoListingUrls(url);
            return homes.Concat(condos).ToList();
        }
        public override Dictionary<String, String> ParseListing(String url)
        {//rental type not on page, have to get it from url
            Dictionary<String, String> dict = base.ParseListing(url);
            dict["rental_type"] = "";
            if (url.IndexOf("condo") != -1) dict["rental_type"] = "Condo";
            if (url.IndexOf("house") != -1) dict["rental_type"] = "House";
            return dict;
        }

        /*
         * PARSING
         */
        public override String ParseCity(String cityText) {
            return cityText.Trim();
        }
        public override String ParseAddress(String addressText) {
            return addressText.Trim();
        }
        public override String ParseBedrooms(String bedroomsText) {
            return bedroomsText.Trim();
        }

        public override String ParseBathrooms(String bathroomsText) {
            Console.WriteLine("Parsing bathrooms");
            Console.WriteLine("\t" + bathroomsText.Substring(0, bathroomsText.IndexOf(' ')).Trim());
            return bathroomsText.Substring(0, bathroomsText.IndexOf(' ')).Trim();
        }

        //DAILY RATE
        public override String ParseDailyRates(HtmlNodeCollection dailyRateNodes) {
            List<String> dailyRates = new List<String>();
            foreach (var rate in dailyRateNodes) {
                dailyRates.Add(parseDailyRates(rate.InnerHtml));
            }
            String dailyRate = getDailyRate(dailyRates);
            return dailyRate;
        }
        public String parseDailyRates(String text) {
            if (text.IndexOf("</th>") != -1) return "$0";
            text = text.Substring(text.IndexOf('$'));
            text = text.Substring(0, text.IndexOf("</td>"));

            if (text.IndexOf("</span>") != -1) text = text.Substring(0, text.IndexOf("</span>"));
            return text;
        }
        public String getDailyRate(List<String> dailyRates) {
            if (dailyRates.Count == 0) return "$0";
            int sum = 0;
            foreach(String rate in dailyRates) {
                int rateAsInt = Int32.Parse( rate.Substring(1).Replace(",", "") );
                sum += rateAsInt;
            }
            return "$" + (sum / dailyRates.Count).ToString();
        }


        //WEEKLY RATE
        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes)
        {
            List<String> weeklyRates = new List<String>();
            foreach (var rate in weeklyRateNodes)
            {
                weeklyRates.Add(parseWeeklyRates(rate.InnerHtml));
            }
            String weeklyRate = getWeeklyRate(weeklyRates);
            return weeklyRate;
        }
        public String parseWeeklyRates(String text)
        {
            if (text.IndexOf("</th>") != -1) return "$0";
            text = text.Substring(text.IndexOf('$')+1);
            text = text.Substring(text.IndexOf('$'));
            text = text.Substring(0, text.IndexOf("</td>"));

            if (text.IndexOf("</span>") != -1) text = text.Substring(0, text.IndexOf("</span>"));
            return text;
        }
        public String getWeeklyRate(List<String> weeklyRates)
        {
            if (weeklyRates.Count == 0) return "$0";
            int sum = 0;
            foreach (String rate in weeklyRates)
            {
                int rateAsInt = Int32.Parse(rate.Substring(1).Replace(",", ""));
                sum += rateAsInt;
            }
            return "$" + (sum / weeklyRates.Count).ToString();
        }


        //IMAGES
        public override String ParseImages(HtmlNodeCollection imageNodes)
        {
            String images = "[";
            foreach (var image in imageNodes) {
                images += "https://www.northmyrtlebeachtravel.com" + image.Attributes["src"].Value + ", ";
            }
            images = images.Substring(0, images.Length - 2) + "]";
            return images;
        }

        //FETCHING URLS
        public List<String> FetchHomeListingUrls(String url)
        {
            var html = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'alphatable')][1]/tr/td[contains(@class, 'alphadata')]/a");

            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                urls.Add("https://www.northmyrtlebeachtravel.com" + node.Attributes["href"].Value);
            }

            return urls;
        }
        public List<String> FetchCondoListingUrls(String url)
        {
            var html = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            var condoNodes = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'alphatable')][2]/tr/td[contains(@class, 'alphadata')]/a");

            List<String> urls = new List<string>();
            foreach (var condoNode in condoNodes)
            {
                var condoListingsDoc = web.Load("https://www.northmyrtlebeachtravel.com" + condoNode.Attributes["href"].Value);
                Console.WriteLine("Page url: " + "https://www.northmyrtlebeachtravel.com" + condoNode.Attributes["href"].Value);
                var units = condoListingsDoc.DocumentNode.SelectNodes("//table[contains(@class, 'condoratestable')]/tbody/tr/td[contains(@class, 'condorateslabel')]/a");

                if (units != null)
                {
                    foreach (var unit in units)
                    {
                        urls.Add("https://www.northmyrtlebeachtravel.com" + unit.Attributes["href"].Value);
                    }
                }
            }
            return urls;
        }
    }
}
