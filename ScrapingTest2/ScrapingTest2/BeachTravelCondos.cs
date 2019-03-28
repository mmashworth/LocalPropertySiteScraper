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
    public class BeachTravelCondos
    {
        public BeachTravelCondos()
        {
            Console.WriteLine("Hello, World!");

            List<String> listing_urls = FetchListingUrls();
            List<Dictionary<String, String>> results = new List<Dictionary<String, String>>();

            foreach (String url in listing_urls)
            {
                //Console.WriteLine("URL: " + url);
                //Dictionary<String, String> result = ParseListing(url);
            }

            Console.WriteLine(listing_urls.Count);
            //InsertIntoDB(results);

            Console.WriteLine("end of constructor");
        }

        public List<String> FetchListingUrls()
        {
            var html = "https://www.northmyrtlebeachtravel.com/rental/alpha.html";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);
            List<String> urls = new List<String>();

            //-------------------------------------------------------------
            //get urls for CONDOS
            var condoNodes = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'alphatable')][2]/tr/td[contains(@class, 'alphadata')]/a");

            foreach(var condoNode in condoNodes) {
                Console.WriteLine(condoNode.InnerHtml);
            }


            foreach (var condoNode in condoNodes)
            {
                var condoListingsDoc = web.Load("https://www.northmyrtlebeachtravel.com" + condoNode.Attributes["href"].Value);
                Console.WriteLine("Page url: " + "https://www.northmyrtlebeachtravel.com" + condoNode.Attributes["href"].Value);
                var units = condoListingsDoc.DocumentNode.SelectNodes("//table[contains(@class, 'condoratestable')]/tbody/tr/td[contains(@class, 'condorateslabel')]");

                if (units != null) {
                    foreach (var unit in units) {
                        Console.WriteLine("\tNew Unit: " + "https://www.northmyrtlebeachtravel.com" + unit.InnerHtml);
                        urls.Add("https://www.northmyrtlebeachtravel.com" + unit.InnerHtml);
                    }
                }
            }

            return urls;
        }
    }
}
