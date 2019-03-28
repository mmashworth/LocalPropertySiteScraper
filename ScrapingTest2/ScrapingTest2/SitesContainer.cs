using System;
namespace ScrapingTest2
{
    using System.Collections.Generic;
    public class SitesContainer
    {
        List<LocalPropertySiteScraper> siteScrapers = new List<LocalPropertySiteScraper>();
        public SitesContainer()
        {
            //siteScrapers.Add(new ODBeachRentals());
            //siteScrapers.Add(new SeaSideVacations());
            //siteScrapers.Add(new BeachTravelHomes());
            //siteScrapers.Add(new BeachComber());
            //siteScrapers.Add(new GrandStrand());
            //siteScrapers.Add(new GrandStrandVacations());
            //siteScrapers.Add(new McMillanVacations());
            //siteScrapers.Add(new CondoLux());
            //siteScrapers.Add(new OwnerDirect());
            //siteScrapers.Add(new WhiteRealty());
        }
        public void ScrapeAllSites() {
            foreach(LocalPropertySiteScraper scraper in siteScrapers) {
                scraper.Scrape();
            }
        }
    }
}
