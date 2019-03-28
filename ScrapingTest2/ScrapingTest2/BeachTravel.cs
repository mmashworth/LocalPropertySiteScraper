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
    public class BeachTravel
    {
        public BeachTravel()
        {
            
        }
    }

    public List<String> FetchHouseListingUrls()
    {
        var html = "https://www.northmyrtlebeachtravel.com/rental/alpha.html";
        HtmlWeb web = new HtmlWeb();
        var htmlDoc = web.Load(html);

        //get urls
        var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'ldgxListingName')]/a");

        List<String> urls = new List<String>();
        foreach (var node in nodes)
        {
            urls.Add(node.Attributes["href"].Value);
        }
        return urls;
    }
}
