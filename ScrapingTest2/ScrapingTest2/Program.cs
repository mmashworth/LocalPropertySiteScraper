// @nuget: HtmlAgilityPack

using System;
using System.Xml;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
//using System.Data.SqlClient;

using MySql.Data.MySqlClient;  
using System.Data.SqlTypes;

using ScrapingTest2;

public class Program
{
    
    public static void Main()
    {
        //HomesAndVacations hav = new HomesAndVacations();
        (new SitesContainer()).ScrapeAllSites();

        /*
        //(new ODBeachRentals()).Scrape();
        //(new SeaSideVacations()).Scrape();
        //(new BeachTravelHomes()).Scrape();
        //(new BeachComber()).Scrape();
        //(new GrandStrand()).Scrape();
        //(new GrandStrandVacations()).Scrape();
        //(new McMillanVacations()).Scrape();
        //(new CondoLux()).Scrape();
        //(new OwnerDirect()).Scrape();
        //(new WhiteRealty()).Scrape();
        */


        //(new NorthBeachVacations()).Scrape();                         //79
        //CondoWorld cw = new CondoWorld();                             //600+
        //ChoiceVacationRentals cvr = new ChoiceVacationRentals();      //741
        //RetreatMyrtleBeach rmb = new RetreatMyrtleBeach();            //51
                                                                        //88
                                                                        //60
    }




}
