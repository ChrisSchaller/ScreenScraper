using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace ScreenScraper
{
    internal class GumTreeMobileNumberScraper : Scraper
    {
        Random rand = new System.Random();

        public GumTreeMobileNumberScraper(string url, string previousHistoryFile = null)
            : base(url, previousHistoryFile)
        {

        }

        protected IEnumerable<Uri> EnumerateResultsPages(string rootPage)
        {
            Console.WriteLine("Enumerate Results Pages: " + rootPage);

            WebClient client = GetWebClient();


            //client.BaseAddress = rootPage;


            var html = client.DownloadString(new Uri(rootPage));

            // this is the first page, so emit it anyway
            yield return new Uri(rootPage);

            // Now we are only interested in the page count
            // but maybe it's just easier to go by the links...

            //< a href = "/flats-houses/uk/page2?search_time=1460907614381" data - analytics = "gaEvent:PaginationPage" >2</ a >
            var index = html.IndexOf("data-analytics=\"gaEvent:PaginationPage\"");
            if (index > 0)
            {
                index = html.IndexOf(">", index);
                var href = html.Substring(0, index).LastIndexOf("href=\"");
                var end = html.IndexOf("\"", href + "href=\"".Length + 1);

                string url = html.Substring(href, end - href);
                string token = url.Split('&', '?').FirstOrDefault(c => c.ToLower().Trim().StartsWith("search_time"));

                if (token != null)
                {
                    for (int page = 2; page < 2000; page++)
                    {
                        yield return new Uri(new Uri(rootPage), string.Format("/uk/page{0}?{1}", page, token));
                    }
                }
            }
        }

        private static WebClient GetWebClient()
        {
            WebClient client = new WebClient();

            client.Headers.Add(HttpRequestHeader.Accept, "text / html, application / xhtml + xml, image / jxr, */*");
            //client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-gb");
            //client.Headers.Add(HttpRequestHeader.Cookie: gt_ab=rn:ODM1; gt_p=id:Y2I4YTZkMjQtOWNjOC00NjgyLWFjOTUtMWNhMTY5ZjgxZTFh; gt_s=sc:MTAyMDE=|ar:aHR0cDovL3d3dy5ndW10cmVlLmNvbS9mbGF0cy1ob3VzZXMvdWsvcGFnZTc/c2VhcmNoX3RpbWU9MTQ2MDg2MDg5MTEyMw==|clicksource_featured:MTE2MzE3NDY4MCwxMTYyMzUyMjg3LDExNjQ2MTg2MTQsMTE2MzYzNDYyNywxMTQ5ODg5NDU1|sk:|clicksource_nearby:|id:MzRERjUxNDM0RjA2MzRCNzI0MzM2MUM0NDIzMDJGMDI=|clicksource_natural:MTE2MTI1MzUwMSwxMTQ5MDA4NjMwLDEwNzA2MDkxMzUsMTE1Mzk0MDQ1MywxMTYzMTMwNTc3LDEwNTQyNjczNDAsMTE2MTA2Mzg1NSwxMTU1ODk1NTA5LDEwNzM0MzAxMzcsMTE2MzQ2MTQ4OSwxMDQ4MjgxMTE2LDExNjA0MjMxNjcsMTExMDE5MjIyMywxMTYzNDU4MzQzLDExNjAyMjYwMjQsMTA0NTg2MDM5NywxMDcwNjEwOTE2LDEwNTIwNTY0MzMsMTE2NDk4MzQyMiwxMTYwNDI0MzYzLDExNDkyMjY0OTMsMTA0OTA3Njc1MCwxMDQwNTc3NDkyLDExNTc5NjI5OTYsMTA3MTY2MjA2MA==; gt_userPref=isSearchOpen:dHJ1ZQ==|recentAdsOne:Y2Fycy12YW5zLW1vdG9yYmlrZXM=|cookiePolicy:dHJ1ZQ==|recentAdsTwo:Zm9yLXNhbGU=|location:dWs=; optimizelyEndUserId=oeu1460860895082r0.06583080365567273; optimizelySegments=%7B%222156960458%22%3A%22false%22%2C%222163830237%22%3A%22edge%22%2C%222181900218%22%3A%22direct%22%2C%222189160803%22%3A%22none%22%7D; optimizelyBuckets=%7B%7D; _ga=GA1.2.820497926.1460860896; __gads=ID=f248fdcc04879eb1:T=1460860896:S=ALNI_MZFbCQmtBfpGDwwEkcMnbda2QoWxQ; _gat=1; cto2_gumtree=; _gali=fullListings; ki_t=1460861225105%3B1460861225105%3B1460861225105%3B1%3B1; ki_r=
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586");
            return client;
        }

        protected IEnumerable<Uri> EnumerateListingLinks(Uri page)
        {
            Console.Write("Enumerate Page Links: " + page.AbsolutePath);

            WebClient client = GetWebClient();
            var html = client.DownloadString(page);

            // <a class="listing-link" href="/p/3-bedrooms-rent/amazing-3-bedroom-2-bathroom-apartment-in-merchant-square-w2-offers-accepted-/1157538078" itemprop="url">
            string expression = "<a class=\"listing-link\" href=\"([^\"]+)\"";
            // @"\<a class=\""listing-link\"" href=\""([^\""]+)\>"
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(expression);
            var matches = regex.Matches(html);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                // is this the codez
                if (match.Success && match.Groups.Count > 1)
                {
                    yield return new Uri(page, match.Groups[1].Value);
                }
            }

        }

        protected override Dictionary<string, string> ScrapePageData(Uri page)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            results.Add("url", page.AbsoluteUri);
            results.Add("mobile", GetMobileNumberFromThread(page));

            return results;
        }

        private string GetMobileNumberFromThread(Uri url)
        {
            //Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            Console.Write("Read Mobile from: " + url.AbsolutePath);
            using (var br = new WebBrowser())
            {
                br.ScriptErrorsSuppressed = true;

                bool navigated = false;
                bool filterNavPages = true;
                /*
                br.Navigating += (object s, WebBrowserNavigatingEventArgs navE) =>
                {
                    if (filterNavPages)
                    {
                        // cancel all non-essential and all tracking navigation attempts
                        var path = navE.Url.AbsolutePath.ToLower();
                        if (!path.EndsWith(".js")
                                && (path.StartsWith("https://www.google")
                                || path.Contains("google"))
                                || path == "blank"
                                )
                            navE.Cancel = true;
                        else if (path.EndsWith(".png")
                            || path.EndsWith(".bmp")
                            || path.EndsWith(".jpg")
                            )
                            navE.Cancel = true;

                        if (navE.TargetFrameName != "")
                            navE.Cancel = true;
                    }
                };
                */
                br.DocumentCompleted += (object s, WebBrowserDocumentCompletedEventArgs navE) =>
                {
                    navigated = true;
                    //Console.WriteLine(navE.Url);
                };


                br.Navigate(url);

                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                while ((br.IsBusy || !navigated) && watch.Elapsed < TimeSpan.FromSeconds(15))
                {
                    Application.DoEvents();
                    Thread.Sleep(rand.Next(500, 2000));
                }

                if (!navigated)
                {
                    Console.Write(" ... Timeout");

                    br.Navigate(url); // refresh

                    watch.Restart();
                    while ((br.IsBusy || !navigated) && watch.Elapsed < TimeSpan.FromSeconds(15))
                    {
                        Application.DoEvents();
                        Thread.Sleep(rand.Next(500, 2000));
                    }
                }
                watch.Stop();

                if (!navigated)
                {
                    Console.WriteLine(" ... Timeout");
                    return "TIMEOUT";
                }

                Console.Write(" ... Loaded");
                //Console.WriteLine("Natigated to {0}", url);

                // Inject custom Alert function to prevent javascript user interactions
                var head = br.Document.GetElementsByTagName("head")[0];
                var scriptEl = br.Document.CreateElement("script");
                string alertBlocker = "window.alert = function () { }";
                scriptEl.SetAttribute("text", alertBlocker);
                head.AppendChild(scriptEl);


                var links = br.Document.GetElementsByTagName("a");
                bool linkFound = false;
                foreach (HtmlElement link in links)
                {
                    if (!string.IsNullOrWhiteSpace(link.GetAttribute("data-reveal")))
                    {
                        // get the mobile before
                        string mobile = "";
                        foreach (System.Windows.Forms.HtmlElement node in link.Parent.Children)
                        {
                            if (node.GetAttribute("itemprop") == "telephone")
                            {
                                mobile = node.InnerText;
                                linkFound = true;
                                break;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(mobile))
                        {
                            if (linkFound)
                                break; // If it had a telephone field but it was empty, skip this page altogether
                            else
                                continue; // if it was not a telephone, skip to the next link
                        }

                        int counter = 0;

                        // Only bother resolving numbers that start with 07.
                        while (string.IsNullOrWhiteSpace(mobile) || (mobile.ToLower().EndsWith("xxxx") && (mobile.Trim().StartsWith("07") || mobile.Trim().StartsWith("7"))))
                        {
                            counter++;
                            Console.Write(" ." + counter.ToString() + " . " + mobile);

                            if (counter > 5)
                            {
                                break;
                            }

                            foreach (System.Windows.Forms.HtmlElement node in link.Parent.Children)
                            {
                                if (node.GetAttribute("itemprop") == "telephone")
                                {
                                    mobile = node.InnerText;
                                    break;
                                }
                            }

                            if (mobile.Trim().ToLower().EndsWith("xxxx"))
                            {
                                int sleep = rand.Next(200, 2000);
                                Console.Write(" (" + sleep + "ms) ");
                                Thread.Sleep(sleep);

                                bool focus = false;
                                link.GotFocus += (object s, HtmlElementEventArgs navE) =>
                                {
                                    focus = true;
                                };
                                link.Focus();

                                Thread.Sleep(100);
                                Application.DoEvents();
                                Thread.Sleep(rand.Next(100, 1000));


                                navigated = false;
                                filterNavPages = false;

                                mshtml.IHTMLElement nativeElement = link.DomElement as mshtml.IHTMLElement;
                                nativeElement.click();

                                Thread.Sleep(100);
                                Application.DoEvents();
                                Thread.Sleep(rand.Next(100, 1000));

                                // wait for the click to finish executing
                                watch.Restart();
                                while ((br.IsBusy || !navigated) && watch.Elapsed < TimeSpan.FromSeconds(15))
                                {
                                    Application.DoEvents();
                                    Thread.Sleep(rand.Next(200, 1200));
                                }

                                if (!navigated)
                                {
                                    Console.Write(" ... Timeout");

                                    nativeElement.click(); // re-click

                                    watch.Restart();
                                    while ((br.IsBusy || !navigated) && watch.Elapsed < TimeSpan.FromSeconds(15))
                                    {
                                        Application.DoEvents();
                                        Thread.Sleep(rand.Next(200, 1200));
                                    }
                                }
                                watch.Stop();

                                // Now, re-read
                                foreach (System.Windows.Forms.HtmlElement node in link.Parent.Children)
                                {
                                    if (node.GetAttribute("itemprop") == "telephone")
                                    {
                                        mobile = node.InnerText;
                                        break;
                                    }
                                }
                            }
                        }

                        Console.WriteLine(" ... " + mobile);

                        return mobile;
                    }
                }

                Console.WriteLine(" ... NOT FOUND");
                return null;
            }
        }

        protected override IEnumerable<Uri> EnumeratePages(string rootUrl)
        {
            foreach (var page in EnumerateResultsPages(rootUrl))
            {
                foreach (var result in EnumerateListingLinks(page))
                {
                    yield return result;
                }
            }
        }
    }
}