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
    public class ODBeachRentals : LocalPropertySiteScraper
    {
        public ODBeachRentals()
        {
            listingsUrl = "https://www.odbeachrentals.com/north-myrtle-beach-rentals?avail_filter%5Brcav%5D%5Bbegin%5D=&avail_filter%5Brcav%5D%5Bend%5D=&avail_filter%5Brcav%5D%5Bflex_type%5D=d&avail_filter%5Brcav%5D%5Badult%5D=1&avail_filter%5Brcav%5D%5Bchild%5D=0&beds_numeric=&baths_numeric=&occ_total_numeric=&field_vr_unit_type_tid=All&weblink_area=All&ldrc_type=All&sort_by=random_seed&items_per_page=60";
            site = "Ocean Drive Beach Rentals";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["address"] = "//section[@id='location']/div";
            fieldXPath["bedrooms"] = "//div[contains(@class, 'rc-lodging-beds')]";
            fieldXPath["bathrooms"] = "//div[contains(@class, 'rc-lodging-baths')]";
            fieldXPath["dailyRate"] = "//table[contains(@class, 'rc-item-prices')]/tbody/tr/td[contains(@class, rc-price-col)]/span/span";
            fieldXPath["weeklyRate"] = "//table[contains(@class, 'rc-item-prices')]/tbody/tr/td[contains(@class, rc-price-col)]/span/span";
            fieldXPath["images"] = "//img[contains(@class,'rsTmb')]";
        }

        public override void GetNumberOfNightsUnavailable(String url) {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);
            var tables = listingPage.DocumentNode.SelectNodes("//table[contains(@class,'rc-calendar')]");

            for (int i = 0; i < 12; i++) {
                String captionXPath = tables.ElementAt(i).XPath + "/caption";
                String caption = listingPage.DocumentNode.SelectSingleNode(captionXPath).InnerText;
                String month = getMonth(caption); 

                String unavailableXPath = tables.ElementAt(i).XPath + "/tr/td[contains(@class, 'av-X')]";
                HtmlNodeCollection unavailable = listingPage.DocumentNode.SelectNodes(unavailableXPath);
                if(unavailable != null){
                    nightsUnavailable[month] = unavailable.Count.ToString();
                }
            }
        }



      
        public override String ParseAddress(String scriptStr) {
            int startIndex = scriptStr.IndexOf("daddr");
            int endIndex = scriptStr.IndexOf("saddr");
            String addy = scriptStr.Substring(startIndex, endIndex - startIndex);
            addy = addy.Substring(6).Replace("+", " ").Replace(" North Myrtle Beach SC&amp;", "");
            Console.WriteLine(addy);
            return addy;
        }

        public override String ParseBedrooms(String bedroomsText)
        {
            return bedroomsText.Substring(0, bedroomsText.IndexOf(' '));
        }
        public override String ParseBathrooms(String bathroomsText) {
            return bathroomsText.Substring(0, bathroomsText.IndexOf(' '));
        }


        public String ParseRates(List<String> rates) {
            int sum = 0;
            foreach (String r in rates) {
                sum += Int32.Parse(r.Substring(1).Replace(",","")); //remove $ and commas
            }
            int avgRate = ((int)sum / rates.Count);
            return "$" + avgRate.ToString();
        }


        public override String ParseDailyRates(HtmlNodeCollection dailyRateNodes) {
            List<String> dailyRatesList = new List<String>();
            int i = 0;
            foreach (var rates in dailyRateNodes)
            {//daily and weekly alternate
                if (i % 2 == 0) dailyRatesList.Add(rates.InnerHtml);
                i++;
            }
            return ParseRates(dailyRatesList);
        }

        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes) {
            List<String> weeklyRates = new List<String>();
            int i = 0;
            foreach (var rates in weeklyRateNodes)
            {//daily and weekly alternate
                if (i % 2 == 1) weeklyRates.Add(rates.InnerHtml);
                i++;
            }
            return ParseRates(weeklyRates);
        }
        public override String ParseImages(HtmlNodeCollection imageNodes) {
            String imagesStr = "[";
            foreach (var image in imageNodes)
                imagesStr += image.Attributes["src"].Value + ", ";
            imagesStr = imagesStr.Substring(0, imagesStr.Length - 2) + "]";
            return imagesStr;
        }




        public override List<String> FetchListingUrls(String url)
        {
            var html = "https://www.odbeachrentals.com/north-myrtle-beach-rentals?avail_filter%5Brcav%5D%5Bbegin%5D=&avail_filter%5Brcav%5D%5Bend%5D=&avail_filter%5Brcav%5D%5Bflex_type%5D=d&avail_filter%5Brcav%5D%5Badult%5D=1&avail_filter%5Brcav%5D%5Bchild%5D=0&beds_numeric=&baths_numeric=&occ_total_numeric=&field_vr_unit_type_tid=All&weblink_area=All&ldrc_type=All&sort_by=random_seed&items_per_page=60";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls for normal listings
            var nodes = htmlDoc.DocumentNode.SelectNodes("//h3[contains(@class, 'rc-core-item-name')]/a");


            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                //Console.WriteLine(node.InnerHtml);
                urls.Add("https://www.odbeachrentals.com" + node.Attributes["href"].Value);
            }

            //get urls from condo nodes
            var condoUrlNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'field') and contains(@class, 'field-name-title')]/h3/a");
            String condoUrl = "";
            foreach(var node in condoUrlNodes) {
                condoUrl = "https://www.odbeachrentals.com" + node.Attributes["href"].Value;

                //scrape each of these pages
                htmlDoc = web.Load(condoUrl);
                var condoNodes = htmlDoc.DocumentNode.SelectNodes("//h3[contains(@class, 'rc-core-item-name')]/a");
                foreach(var condoNode in condoNodes) {
                    urls.Add("https://www.odbeachrentals.com" + condoNode.Attributes["href"].Value);
                }
            }
            return urls;
        }


    }
}
