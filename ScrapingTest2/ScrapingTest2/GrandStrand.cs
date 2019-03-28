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
    public class GrandStrand : LocalPropertySiteScraper
    {
        public GrandStrand()
        {
            listingsUrl = "https://www.northmyrtlebeach.com/all-resorts";
            site = "Grand Strand Resorts";
            city = "North Myrtle Beach";
            state = "South Carolina";

            fieldXPath["address"] = "//address";
            fieldXPath["bedrooms"] = "//div[contains(@class, 'row')]/div[contains(@class, 'col-md-2')][2]/div/h2";
            fieldXPath["bathrooms"] = "//div[contains(@class, 'row')]/div[contains(@class, 'col-md-2')][3]/div/h2";
            fieldXPath["rentalType"] = "//ul[@class='repeatPropertyInfo']";
            fieldXPath["dailyRate"] = "//div[contains(@class, 'panel-body')]/table/tr/td[3]";
            fieldXPath["weeklyRate"] = "//div[contains(@class, 'panel-body')]/table/tr/td[4]";
            fieldXPath["images"] = "//span[contains(@class, 'p-thumbs-wrap')]/span";
        }

        public override void GetNumberOfNightsUnavailable(String url)
        {
            HtmlWeb web = new HtmlWeb();
            var listingPage = web.Load(url);
            var tables = listingPage.DocumentNode.SelectNodes("//div[contains(@class, 'cal-container')]");

            foreach (var table in tables)
            {
                String captionXPath = table.XPath + "/div[1]/strong";
                String caption = listingPage.DocumentNode.SelectSingleNode(captionXPath).InnerText;
                String month = getMonth(caption);
                Console.WriteLine(month);

                String unavailableXPath = table.XPath + "/table/tr/td[contains(@class, 'booked')]";
                HtmlNodeCollection unavailable = listingPage.DocumentNode.SelectNodes(unavailableXPath);
                if (unavailable != null)
                {
                    nightsUnavailable[month] = unavailable.Count.ToString();
                }
            }

            foreach(KeyValuePair<String, String> kvp in nightsUnavailable) {
                Console.WriteLine(kvp.Key + ": " + kvp.Value);
            }
        }

        public override String ParseBedrooms(String bedroomsText) { return bedroomsText; }
        public override String ParseBathrooms(String bathroomsText) { return bathroomsText; }

        public override String ParseRentalType(String rentalTypeText) {
            if (rentalTypeText.IndexOf("Type: ") == -1) return ""; //no type listed
            rentalTypeText = rentalTypeText.Substring(rentalTypeText.IndexOf("Type: ") + 6);
            rentalTypeText = rentalTypeText.Substring(0, rentalTypeText.IndexOf("</li>"));
            return rentalTypeText;
        }

        public override String ParseImages(HtmlNodeCollection imageNodes) {
            String imagesStr = "";
            if (imageNodes != null) {
                imagesStr = "[";
                foreach (var image in imageNodes) {
                    String image_url = ParseImage(image.Attributes["style"].Value);
                    if (image_url != null) {
                        imagesStr += image_url + ", ";
                    }
                }
                imagesStr = imagesStr.Substring(0, imagesStr.Length - 2) + "]";
            }
            return imagesStr;
        }
        public String ParseImage(String image) {
            int startIndex = image.IndexOf("url('");
            image = image.Substring(startIndex + 5);
            int endIndex = image.IndexOf("');");
            image = image.Substring(0, endIndex);
            if (image[0].Equals('/')) //get rid of youtube icons
                return null;
            else
                return image;
        }

        public override String ParseDailyRates(HtmlNodeCollection dailyRateNodes) {
            List<String> dailyRates = new List<String>();
            if (dailyRateNodes != null) {
                foreach (var rateNode in dailyRateNodes) {
                    dailyRates.Add(rateNode.InnerHtml);
                }
            }
            return ParseRates(dailyRates);
        }
        public override String ParseWeeklyRates(HtmlNodeCollection weeklyRateNodes) {
            List<String> weeklyRates = new List<String>();
            if (weeklyRateNodes != null) {
                foreach (var rateNode in weeklyRateNodes) {
                    weeklyRates.Add(rateNode.InnerHtml);
                }
            }
            return ParseRates(weeklyRates);
        }
        public String ParseRates(List<String> rates)
        {
            if (rates.Count == 0) return "";
            int sum = 0;
            foreach (String rate in rates) {
                String r = rate.Substring(0, rate.IndexOf("."));
                r = r.Substring(1).Replace(",", ""); //get rid of $ and commas
                sum += Int32.Parse(r);
            }
            return "$" + (sum / rates.Count);
        }

        public override String ParseAddress(String addressText) {
            addressText = addressText.Substring(addressText.LastIndexOf("</b>")+4);
            addressText = addressText.Replace("\"", "").Trim();
            return addressText;
        }

        public override List<String> FetchListingUrls(String url)
        {
            var html = url;
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            //HOME URLS
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, panel-heading)]/h5/a");
            List<String> condo_urls = new List<String>();
            foreach (var node in nodes) {
                condo_urls.Add("https://www.northmyrtlebeach.com" + node.Attributes["href"].Value);
            }

            //CONDO URLS
            List<String> urls = new List<string>();
            foreach (String condo_url in condo_urls)
            {
                Console.WriteLine(condo_url);
                htmlDoc = web.Load(condo_url);
                var condoNodes = htmlDoc.DocumentNode.SelectNodes("//ul[contains(@class,'resorts-list')]/li/div/div/h5/a");
                foreach (var condoNode in condoNodes) {
                    urls.Add("https://www.northmyrtlebeach.com" + condoNode.Attributes["href"].Value);
                }
            }
            return urls;
        }
    }
}
