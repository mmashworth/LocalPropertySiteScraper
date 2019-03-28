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
    public class RetreatMyrtleBeach
    {
        public RetreatMyrtleBeach()
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
            var html = "https://www.retreatmyrtlebeach.com/north-myrtle-beach-vacation-rentals#q=*%3A*";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//riot-solr-result-list[contains(@class, 'haschild-rc-riot-result-list-item')]");


            List<String> urls = new List<String>();
            foreach (var node in nodes)
            {
                Console.WriteLine(node.InnerHtml);
                urls.Add(node.Attributes["href"].Value);
            }
            return urls;
        }
    }
}
