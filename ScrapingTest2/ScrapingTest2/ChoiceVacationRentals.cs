//STUCK

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
    public class ChoiceVacationRentals
    {
        public ChoiceVacationRentals()
        {
            Console.WriteLine("Hello, World!");

            List<String> listing_urls = FetchListingUrls();
            List<Dictionary<String, String>> results = new List<Dictionary<String, String>>();

            foreach (String url in listing_urls)
            {
                Console.WriteLine(url);
                //results.Add(ParseListing(url));
            }
            //InsertIntoDB(results);

            Console.WriteLine("end of constructor");
        }

        public List<String> FetchListingUrls()
        {
            var html = "https://www.choicevacationrentals.com/usa/south-carolina/myrtle-beach/north-myrtle-beach";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'results')]/search-canvas");

            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                String url = node.InnerHtml;
                //Console.WriteLine(url);
                //Console.WriteLine("\n\n");
                urls.Add(url);
            }
            return urls;
        }
    }
}

