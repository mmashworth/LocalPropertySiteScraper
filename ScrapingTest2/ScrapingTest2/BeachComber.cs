//TODO add units for each location


//SITE: https://www.beachcombervacations.com

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
    public class BeachComber : LocalPropertySiteScraper
    {
        public BeachComber()
        {
            listingsUrl = "https://www.beachcombervacations.com/showall.cfm";
            site = "Beach Comber Vacations";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["address"] = "//table/tr/td[contains(@valign, 'center')]/font/b[2]";
            fieldXPath["bedrooms"] = "//table[contains(@class, 'showprop-wrapper')]/tr/td/div/div/table[2]/tr/td/center/table/tr/td/font/strong";
            fieldXPath["weeklyRate"] = "//table/tr/td/center/table/tr/th[2]/font";
            fieldXPath["images"] = "//a/img[1]";
        }

        public override List<String> FetchListingUrls(String url)
        {
            var html = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//table/tr/td[contains(@class, 'content-interior')]/font/a");
            Console.WriteLine(nodes.Count);
            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                urls.Add("https://www.beachcombervacations.com/" + node.Attributes["href"].Value);
            }
            return urls;
        }


        public override String ParseAddress(String addressText)
        {
            String addy = addressText.Replace("BLVD VIEW", "");
            int BR_index = addy.IndexOf("BR");                               //remove bathrooms from address
            if (BR_index != -1) addy = addy.Substring(BR_index + 2);
            int BDM_index = addy.IndexOf("BDM");                             //remove bedroom info from address
            if (BDM_index != -1) addy = addy.Substring(BDM_index + 3);
            addy = addy.Trim();                                           //remove - from begining
            if (addy[0] == '-') addy = addy.Substring(1);

            return addy.Trim();
        }

        public override String ParseBedrooms(String bedroom)
        {
            if (bedroom.IndexOf("BEDROOM") == -1) return "";
            bedroom = bedroom.Substring(bedroom.IndexOf("BEDROOM") - 2, 1);
            return bedroom;
        }

        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes)
        {
            List<String> weeklyRates = new List<string>();
            foreach (var node in weeklyRateNodes)
            {
                weeklyRates.Add(node.InnerHtml);
            }

            if (weeklyRates.Count == 0) return "";
            int sum = 0;
            foreach (String rate in weeklyRates)
            {
                String r = rate.Substring(0, rate.IndexOf("."));
                r = r.Substring(1).Replace(",", ""); //get rid of $ and commas
                sum += Int32.Parse(r);
            }

            return "$" + (sum / weeklyRates.Count);
        }

        public override string ParseImages(HtmlNodeCollection imageNodes)
        {
            String images = "[";
            foreach (var node in imageNodes) {
                images += "https://www.beachcombervacations.com/" + node.Attributes["src"].Value + ", ";
            }
            images = images.Substring(0, images.Length - 2) + "]";
            return images;
        }
    }
}