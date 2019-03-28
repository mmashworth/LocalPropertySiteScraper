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
    public class McMillanVacations : LocalPropertySiteScraper
    {
        public McMillanVacations()
        {
            listingsUrl = "http://www.mcmillanvacations.com/site/PropertyList/32995/default.aspx";
            site = "McMillan Vacations";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["city"] = "//span[@class='locality']";
            fieldXPath["address"] = "//span[@class='street-address']";
            fieldXPath["bedrooms"] = "//p[@id='quickDescription']";
            fieldXPath["bathrooms"] = "//p[@id='quickDescription']";
            fieldXPath["rentalType"] = "//p[@id='quickDescription']";
            fieldXPath["dailyRate"] = "//table[@id='rateRanges']/tbody/tr/td[2]";
            fieldXPath["weeklyRate"] = "//table[@id='rateRanges']/tbody/tr/td[3]";
            fieldXPath["images"] = "//div[@class='item']/img";
        }


        public override void GetNumberOfNightsUnavailable(String url)
        {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);
            var tables = listingPage.DocumentNode.SelectNodes("//*[@id='calendar']");
            //nothing within this node from original html
            /*
            foreach (var table in tables)
            {
                String captionXPath = "//div[@id='calendar']/div[1]/div/div/span[1]";
                String caption = listingPage.DocumentNode.SelectSingleNode(captionXPath).InnerHtml;
                String month = getMonth(caption);
                Console.WriteLine(month);
                //*[@id="calendar"]/div/div[5]/table/tbody/tr[3]/td[6]
                String unavailableXPath = "//*[@id='calendar']/div/div[1]/table/tr/td";//[contains(@class, 'ui-datepicker-unselectable')]";
                HtmlNodeCollection unavailable = listingPage.DocumentNode.SelectNodes(unavailableXPath);
                if (unavailable != null){
                    Console.WriteLine(unavailable.Count);
                    nightsUnavailable[month] = unavailable.Count.ToString();
                }
            }

            foreach(KeyValuePair<String, String> kvp in nightsUnavailable) {
                Console.WriteLine(kvp.Key + ": " + kvp.Value);
            }
            */

        }



        public override string ParseImages(HtmlNodeCollection imageNodes)
        {
            String imagesStr = "[";
            foreach (var node in imageNodes) {
                imagesStr += node.Attributes["src"].Value.Substring(2) + ", ";  
            }
            imagesStr = imagesStr.Substring(0, imagesStr.Length - 2) + "]";
            Console.WriteLine(imagesStr);
            return imagesStr;
        }

        public override String ParseAddress(String addressText) {
            return addressText.Trim();
        }

        public override String ParseDailyRates(HtmlNodeCollection dailyRateNodes) {
            List<String> dailyRates = new List<string>();
            foreach (var node in dailyRateNodes)
                dailyRates.Add(node.InnerHtml.Trim());  
            return ParseRates(dailyRates);
        }

        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes) {
            List<String> weeklyRates = new List<string>();
            foreach (var node in weeklyRateNodes)
                weeklyRates.Add(node.InnerHtml.Trim());
            return ParseRates(weeklyRates);
        }

        public String ParseRates(List<String> rates)
        {
            if (rates.Count == 0) return "";

            int sum = 0;
            foreach (String rate in rates) {
                String r = rate;
                if (r.Equals("na")) continue;
                if (r.IndexOf('-') != -1) r = r.Substring(r.IndexOf('-') + 1); //$xxx-$yyy to $yyy

                r = r.Substring(1).Replace(",", ""); //get rid of $ and commas ($x,xxx to xxxx)
                sum += Int32.Parse(r);
            }

            if (sum == 0) return "";
            return "$" + (sum / rates.Count);
        }

        public override String ParseRentalType(String rentalTypeText) {
            if (rentalTypeText.IndexOf("condo") == -1) return "";
            return "Condo";
        }

        public override String ParseBedrooms(String bedroomsText) {
            bedroomsText = bedroomsText.Substring(bedroomsText.IndexOf(", ") + 2);
            return bedroomsText.Substring(0, bedroomsText.IndexOf(" "));
        }

        public override String ParseBathrooms(String bathroomsText) {
            bathroomsText = bathroomsText.Substring(bathroomsText.LastIndexOf(", ") + 2);
            return bathroomsText.Substring(0, bathroomsText.IndexOf(" "));
        }

        public override List<String> FetchListingUrls(String url)
        {
            var html = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='header']/h3/a");
            Console.WriteLine(nodes.Count);
            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                urls.Add("http://www.mcmillanvacations.com" + node.Attributes["href"].Value);
            }
            return urls;
        }
    }
}

