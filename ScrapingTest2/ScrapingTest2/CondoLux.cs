//LOSING ~100 RESULTS FROM AJAX (cannot click on 'view more' button)

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
    public class CondoLux : LocalPropertySiteScraper
    {
        public CondoLux()
        {
            listingsUrl = "http://www.condolux.net/north-myrtle-beach-condos.html";
            site = "Condo Lux";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["city"] = "//div[@class='topUnitAddress']";
            fieldXPath["address"] = "//div[@class='topUnitAddress']";
            fieldXPath["bedrooms"] = "//div[@id='unitPageSB']/table/tr[4]/td[2]";
            fieldXPath["bathrooms"] = "//div[@id='unitPageSB']/table/tr[4]/td[2]";
            fieldXPath["rentalType"] = "//div[@id='unitPageSB']/table/tr[1]/td[2]";
            fieldXPath["dailyRate"] = "//table[@id='table1']/tr/td[3]";
            fieldXPath["weeklyRate"] = "//table[@id='table1']/tr/td[3]";
            fieldXPath["images"] = "//div[contains(@class, 'item')]/img";//div[@class='item']";
        }


        public override void GetNumberOfNightsUnavailable(String url)
        {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);
            var tables = listingPage.DocumentNode.SelectNodes("//*[@id='calendar']/table/tr[2]/td/div");
            foreach (var table in tables)
            {
                String captionXPath = table.XPath + "/div/table/tr[1]/td";
                String caption = listingPage.DocumentNode.SelectSingleNode(captionXPath).InnerText;
                String month = getMonth(caption);

                //String unavailableXPath = table.XPath + "/div/table";///td[contains(@style,'background-color:#ea8bcb;')]";
                String unavailableXPath = table.XPath + "/div/table";

                var calendarTable = listingPage.DocumentNode.SelectSingleNode(unavailableXPath);
                nightsUnavailable[month] = (calendarTable.InnerHtml.Split("#ea8bcb").Length - 1).ToString();
            }

            foreach(KeyValuePair<String, String> kvp in nightsUnavailable) {
                Console.WriteLine(kvp.Key + ": " + kvp.Value);
            }

        }

        public override String ParseCity(String cityText) {
            if (cityText.IndexOf("North Myrtle Beach") != -1) return "North Myrtle Beach";
            return ""; //don't care about cities other than NMB
        }
        public override String ParseAddress(String addressText) {
            return addressText.Substring(0, addressText.IndexOf('-') - 1);
        }
        public override String ParseBedrooms(String bedroomsText) {
            return bedroomsText.Substring(0, bedroomsText.IndexOf(" "));
        }
        public override String ParseBathrooms(String bathroomsText) {
            return bathroomsText.Substring(bathroomsText.LastIndexOf(" ") + 1);
        }
        public override String ParseRentalType(String rentalTypeText) {
            return rentalTypeText;
        }
        public override String ParseDailyRates(HtmlNodeCollection dailyRateNodes) {
            List<String> rates = new List<string>();
            foreach (var node in dailyRateNodes) {
                rates.Add(node.InnerHtml.Replace(" ", "").Substring(1));
            }
            return ParseDailyRates(rates);
        }
        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes)
        {
            List<String> rates = new List<string>();
            foreach (var node in weeklyRateNodes) {
                rates.Add(node.InnerHtml.Replace(" ", "").Substring(1));
            }
            return ParseWeeklyRates(rates);
        }

        public override String ParseImages(HtmlNodeCollection imageNodes)
        {
            String imagesStr = "[";
            foreach(var node in imageNodes) {
                imagesStr += node.Attributes["src"].Value.Substring(2) + ", ";
            }
            imagesStr = imagesStr.Substring(0, imagesStr.Length - 2) + "]";
            return imagesStr;
        }

        public String ParseDailyRates(List<String> rates) {
            if (rates.Count == 0) return "";
            int sum = 0;

            for (int i = 0; i < rates.Count; i++) {
                if (i % 2 == 1) {
                    String r = rates[i];
                    if (rates[i].IndexOf(".") != -1) r = rates[i].Substring(0, rates[i].IndexOf(".")); //remove cents
                    sum += Int32.Parse(r);
                }
            }
            return "$" + (sum / rates.Count);
        }
        public String ParseWeeklyRates(List<String> rates)
        {
            if (rates.Count == 0) return "";
            int sum = 0;

            for (int i = 0; i < rates.Count; i++) {
                if (i == 0) continue; //skip header row of table
                if (i % 2 == 0) {
                    String r = rates[i];
                    if (rates[i].IndexOf(".") != -1) r = rates[i].Substring(0, rates[i].IndexOf(".")); //remove cents
                    sum += Int32.Parse(r);
                }
            }
            return "$" + (sum / rates.Count);
        }

        public override List<String> FetchListingUrls(String url) {
            var html = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='maincol']/ul/li/a");

            Console.WriteLine(nodes.Count);
            List<String> condo_urls = new List<String>();
            foreach (var node in nodes) {
                condo_urls.Add("" + node.Attributes["href"].Value);
            }
            List<String> urls = new List<string>();

            foreach(String condo_url in condo_urls) {
                htmlDoc = web.Load(condo_url);
                var condo_nodes = htmlDoc.DocumentNode.SelectNodes("//div/ul[@class='searchResultsUL']/li/div/div/h4/a");
                if (condo_nodes != null) {
                    foreach (var node in condo_nodes) {
                        urls.Add(node.Attributes["href"].Value);
                    }
                }
            }
            return urls;
        }
    }
}

