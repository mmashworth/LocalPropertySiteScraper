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

    public class CondoWorld
    {
        public CondoWorld()
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
            var html = "https://www.condo-world.com/north-myrtle-beach-condo-rentals";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //get urls
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'box_text')]/h3/a");
            List<String> condo_urls = new List<String>();
            foreach (var node in nodes)
            {
                //Console.WriteLine("https://www.condo-world.com" + node.Attributes["href"].Value);
                condo_urls.Add("https://www.condo-world.com" + node.Attributes["href"].Value);
            }

            List<String> urls = new List<string>();
            foreach( String url in condo_urls) {
                if (url.IndexOf("condos") == -1) //it's a home
                    urls.Add(url);
                else { //condo page
                    var condoPage = web.Load(url + "#tab3");
                    Console.WriteLine(url);
                    var y = condoPage.DocumentNode.SelectSingleNode("//body");
                    Console.WriteLine(y.InnerHtml);
                    var condoUnits = condoPage.DocumentNode.SelectNodes("//div[contains(@id, 'tab3')]/div[contains(@id, 'ulist')]/div");
                    var x = condoPage.DocumentNode.SelectSingleNode("//body");
                    //Console.WriteLine(x.InnerHtml);
                    foreach(var unit in condoUnits) {
                        //Console.WriteLine(unit.InnerHtml);
                        //urls.Add(unit.Attributes["href"].Value);
                    }
                }
            }

            return urls;
        }

    }
}

