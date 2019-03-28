//Could not get images

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
	public class WhiteRealty : LocalPropertySiteScraper
    {
        public WhiteRealty()
        {
            listingsUrl = null;
            site = "White Realty";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["address"] = "//span[contains(@id, 'DisplayProperty_ctl00_paneDescription_content_LblLongDesc')]";
            fieldXPath["bedrooms"] = "//ul[contains(@class, 'details')]/li[3]";
            fieldXPath["bathrooms"] = "//ul[contains(@class, 'details')]/li[4]";
            fieldXPath["rentalType"] = "//ul[contains(@class, 'details')]/li[2]";
            fieldXPath["dailyRate"] = "//table[contains(@id, 'DisplayProperty_ctl00_paneRates_content_GridPricing')]/tr/td[2]";
            fieldXPath["weeklyRate"] = "//table[contains(@id, 'DisplayProperty_ctl00_paneRates_content_GridPricing')]/tr/td[5]";

            fieldXPath["images"] = "div[contains(@id, 'DisplayProperty_ctl00_pnlSlidshow')]";//"div[contains(@class, 'ps-carousel-content')]";

            fieldXPath["lat"] = "//script[contains(.,'33.8')]";//"//script"//"//div[@class='map_canvas']"; //div[contains(@id, 'map_canvas')]";
            fieldXPath["lon"] = "//script[contains(.,'33.8')]";
        }

        //can only get 2 months
        public override void GetNumberOfNightsUnavailable(String url)
        {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);
            var tables = listingPage.DocumentNode.SelectNodes("//*[@id='DisplayProperty_ctl00_UpdatePanel1']/div[4]/div");

            foreach (var table in tables)
            {
                //Console.WriteLine(table.InnerHtml);
                String captionXPath = table.XPath + "/table/tr[1]";
                String caption = listingPage.DocumentNode.SelectSingleNode(captionXPath).InnerText;
                String month = getMonth(caption);
                Console.WriteLine(month);

                String unavailableXPath = table.XPath + "/table/tr/td[contains(@class, 'AjaxAnnualCalRented')]";
                HtmlNodeCollection unavailable = listingPage.DocumentNode.SelectNodes(unavailableXPath);
                if (unavailable != null) {
                    nightsUnavailable[month] = unavailable.Count.ToString();
                }
            }
        }



        public override String ParseImages(HtmlNodeCollection imageNodes) {
            Console.WriteLine(imageNodes.Count);
            String imagesStr = "[";
            foreach(var node in imageNodes) {
                Console.WriteLine(node.InnerHtml);
            }
            return null;
        }
        public override String ParseLat(String latText) {
            //Console.WriteLine(latText);
            var myRegex = new Regex(@"(33\.\d*)");
            Match match = myRegex.Match(latText);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return "";//no match
        }
        public override String ParseLong(String longText)
        {
            //Console.WriteLine(latText);
            var myRegex = new Regex(@"(-78\.\d*)");
            Match match = myRegex.Match(longText);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return "";//no match
        }
        public override String ParseDailyRates(HtmlNodeCollection dailyRateNodes) {
            List<String> rates = new List<string>();
            foreach(var node in dailyRateNodes) {
                if (node.InnerHtml.IndexOf('$') == -1) continue; //header row
                rates.Add(node.InnerHtml.Trim());
            }
            int twoDayRate = Int32.Parse(ParseRates(rates));
            if (twoDayRate.Equals("")) return "";
            return "$" + (twoDayRate / 2).ToString();
        }

        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes)
        {
            List<String> rates = new List<string>();
            foreach (var node in weeklyRateNodes)
            {
                if (node.InnerHtml.IndexOf('$') == -1) continue; //skip header row
                rates.Add(node.InnerHtml.Trim());
            }
            return "$" + ParseRates(rates);
        }
        public override string ParseRentalType(String rentalTypeText) {
            var myRegex = new Regex(@"</strong>\s(.*)");
            Match match = myRegex.Match(rentalTypeText);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return "";//no match
        }
        public override string ParseAddress(string addressText) {
            var myRegex = new Regex(@"(\d{3}.*)\n");
            Match match = myRegex.Match(addressText);
            if (match.Success) {
                return match.Groups[1].Value.Replace("*", "");
            }
            return "";//no match
        }
        public override string ParseBedrooms(string bedroomsText) {
            var myRegex = new Regex(@"(\d{1,3})");
            Match match = myRegex.Match(bedroomsText);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return ""; // no match
        }
        public override string ParseBathrooms(string bathroomsText)
        {
            var myRegex = new Regex(@"(\d{1,3})");
            Match match = myRegex.Match(bathroomsText);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return ""; // no match
        }

        public override List<String> FetchListingUrls(String url) {
            List<String> listingUrls = new List<String>();
            listingUrls.Add("https://www.white-realty.com/vacation/category/all#&&/wEXEwUISGFuZGljYXAFCEhhbmRpY2FwBQRQZXRzBQRQZXRzBQ1Tb3J0RGlyZWN0aW9uBQNBU0MFDlNvcnRFeHByZXNzaW9uZQUHU21va2luZwUHU21va2luZwUJQW1lbml0aWVzBQItMQUPY2JQcm9wZXJ0eVR5cGVzBQItMQUGU2xlZXBzBQlOdW1HdWVzdHMFC2NiTG9jYXRpb25zBQItMQULQ3VycmVudFBhZ2UFATAFDGxzdFdlYkdyb3VwcwUCLTEFDFByb3BlcnR5VHlwZQUCLTEFB0Fycml2YWxlBQhCZWRyb29tcwUIQmVkcm9vbXMFBUJhdGhzBQVCYXRocwUKUHJvcGVydGllcwUCLTEFCUxvY2F0aW9ucwUCLTEFCURlcGFydHVyZWUFDXR4dFByb3BlcnRpZXNl+o3gBzsCwR8JHsYC/86xYamB8Sf5yB4sK15ULragr8s=");
            listingUrls.Add("https://www.white-realty.com/vacation/category/all#&&/wEXEwUISGFuZGljYXAFCEhhbmRpY2FwBQRQZXRzBQRQZXRzBQ1Tb3J0RGlyZWN0aW9uBQNBU0MFDlNvcnRFeHByZXNzaW9uZQUHU21va2luZwUHU21va2luZwUJQW1lbml0aWVzBQItMQUPY2JQcm9wZXJ0eVR5cGVzBQItMQUGU2xlZXBzBQlOdW1HdWVzdHMFC2NiTG9jYXRpb25zBQItMQULQ3VycmVudFBhZ2UFATEFDGxzdFdlYkdyb3VwcwUCLTEFDFByb3BlcnR5VHlwZQUCLTEFB0Fycml2YWxlBQhCZWRyb29tcwUIQmVkcm9vbXMFBUJhdGhzBQVCYXRocwUKUHJvcGVydGllcwUCLTEFCUxvY2F0aW9ucwUCLTEFCURlcGFydHVyZWUFDXR4dFByb3BlcnRpZXNlKt3TVTuHbA/LcClWmRIuW5qGI7mCeHGKAea6dFq8uNo=");
            listingUrls.Add("https://www.white-realty.com/vacation/category/all#&&/wEXEwUISGFuZGljYXAFCEhhbmRpY2FwBQRQZXRzBQRQZXRzBQ1Tb3J0RGlyZWN0aW9uBQNBU0MFDlNvcnRFeHByZXNzaW9uZQUHU21va2luZwUHU21va2luZwUJQW1lbml0aWVzBQItMQUPY2JQcm9wZXJ0eVR5cGVzBQItMQUGU2xlZXBzBQlOdW1HdWVzdHMFC2NiTG9jYXRpb25zBQItMQULQ3VycmVudFBhZ2UFATIFDGxzdFdlYkdyb3VwcwUCLTEFDFByb3BlcnR5VHlwZQUCLTEFB0Fycml2YWxlBQhCZWRyb29tcwUIQmVkcm9vbXMFBUJhdGhzBQVCYXRocwUKUHJvcGVydGllcwUCLTEFCUxvY2F0aW9ucwUCLTEFCURlcGFydHVyZWUFDXR4dFByb3BlcnRpZXNlGrrXNnCrVXdn4x9fZPtboofNuJPkOiaMse5EO25/G80=");
            listingUrls.Add("https://www.white-realty.com/vacation/category/all#&&/wEXEwUISGFuZGljYXAFCEhhbmRpY2FwBQRQZXRzBQRQZXRzBQ1Tb3J0RGlyZWN0aW9uBQNBU0MFDlNvcnRFeHByZXNzaW9uZQUHU21va2luZwUHU21va2luZwUJQW1lbml0aWVzBQItMQUPY2JQcm9wZXJ0eVR5cGVzBQItMQUGU2xlZXBzBQlOdW1HdWVzdHMFC2NiTG9jYXRpb25zBQItMQULQ3VycmVudFBhZ2UFATMFDGxzdFdlYkdyb3VwcwUCLTEFDFByb3BlcnR5VHlwZQUCLTEFB0Fycml2YWxlBQhCZWRyb29tcwUIQmVkcm9vbXMFBUJhdGhzBQVCYXRocwUKUHJvcGVydGllcwUCLTEFCUxvY2F0aW9ucwUCLTEFCURlcGFydHVyZWUFDXR4dFByb3BlcnRpZXNlUH5iboQTC+3WnzfxcJBvaheppYZX0plAdy8WkTAaleA=");
            listingUrls.Add("https://www.white-realty.com/vacation/category/all#&&/wEXEwUISGFuZGljYXAFCEhhbmRpY2FwBQRQZXRzBQRQZXRzBQ1Tb3J0RGlyZWN0aW9uBQNBU0MFDlNvcnRFeHByZXNzaW9uZQUHU21va2luZwUHU21va2luZwUJQW1lbml0aWVzBQItMQUPY2JQcm9wZXJ0eVR5cGVzBQItMQUGU2xlZXBzBQlOdW1HdWVzdHMFC2NiTG9jYXRpb25zBQItMQULQ3VycmVudFBhZ2UFATQFDGxzdFdlYkdyb3VwcwUCLTEFDFByb3BlcnR5VHlwZQUCLTEFB0Fycml2YWxlBQhCZWRyb29tcwUIQmVkcm9vbXMFBUJhdGhzBQVCYXRocwUKUHJvcGVydGllcwUCLTEFCUxvY2F0aW9ucwUCLTEFCURlcGFydHVyZWUFDXR4dFByb3BlcnRpZXNlEIlm/oNM4f012kpLpp+Z0WBWBWi4MD+T+hkaem751ho=");

            List<String> urls = new List<string>();
            foreach (String url_ in listingUrls) {
                var html = url_;
                HtmlWeb web = new HtmlWeb();
                var htmlDoc = web.Load(html);
                var nodes = htmlDoc.DocumentNode.SelectNodes("//td/div/div/div[contains(@class,'row')]/div/div[contains(@class, 'thumb')]/a[1]");
                foreach (var node in nodes)
                {
                    Console.WriteLine("https://www.white-realty.com" + node.Attributes["href"].Value);
                    urls.Add("https://www.white-realty.com" + node.Attributes["href"].Value);
                }
            }
            return urls;
        }
    }
}
