//STUCK, need to scroll down to load all listings

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
	public class NorthBeachVacations : LocalPropertySiteScraper
    {
        public NorthBeachVacations()
        {
            listingsUrl = "https://www.northbeachvacations.com/booking/results.cfm";
            site = "North Beach Vacations";
            city = "North Myrtle Beach";
            state = "South Carolina";
        }

        public override List<String> FetchListingUrls(String url) {
            var html = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//h3[@class='panel-title']/a");
            Console.WriteLine(nodes.Count);
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
