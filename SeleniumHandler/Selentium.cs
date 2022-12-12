using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using SeleniumHandler.Enums;
using SeleniumHandler.Utils;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace SeleniumHandler
{
    public class Selentium
    {
        public readonly BrowserType? type = null;
        private readonly IWebDriver? driver = null;
        private readonly string downloadpath = "";
        private static Selentium? instance = null;
        public Selentium()
        {
            if (instance == null) return;
            type = instance.type;
            driver = instance.driver;
            downloadpath = instance.downloadpath;
        }

        public Selentium(Chrome chrome)
        {
            type = chrome.Type;
            downloadpath = chrome.DownloadPath;
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.EnableVerboseLogging = false;
            service.HideCommandPromptWindow = true;
            service.EnableAppendLog = false;
            ChromeOptions options = new();
            if (chrome.Silent)
            {
                options.AddArguments("--disable-extensions");
                options.AddArgument("test-type");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("no-sandbox");
                options.AddArgument("headless");
                options.AddArgument("--silent");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--log-level=3");
            }
            options.AddUserProfilePreference("download.default_directory", chrome.DownloadPath);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            driver = new ChromeDriver(service, options);
            instance = this;
        }

        public Selentium(Firefox firefox)
        {
            type = firefox.Type;
            downloadpath = firefox.DownloadPath;
            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            FirefoxProfile profile = new();
            profile.SetPreference("browser.download.dir", firefox.DownloadPath);
            profile.SetPreference("browser.helperApps.neverAsk.saveToDisk", "*");
            FirefoxOptions options = new() { Profile = profile };
            if (firefox.Silent)
            {
                options.AddArguments("--disable-extensions");
                options.AddArgument("test-type");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("no-sandbox");
                options.AddArgument("headless");
                options.AddArgument("--silent");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--log-level=3");
            }
            driver = new FirefoxDriver(service, options);
            instance = this;
        }

        public Selentium(Edge edge)
        {
            type = edge.Type;
            downloadpath = edge.DownloadPath;
            EdgeDriverService service = EdgeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.EnableVerboseLogging = false;
            service.EnableAppendLog = false;
            EdgeOptions options = new();
            if (edge.Silent)
            {
                options.AddArguments("--disable-extensions");
                options.AddArgument("test-type");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("no-sandbox");
                options.AddArgument("headless");
                options.AddArgument("--silent");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--log-level=3");
            }
            options.AddUserProfilePreference("download.default_directory", edge.DownloadPath);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            driver = new EdgeDriver(service, options);
            instance = this;
        }

        public IWebDriver? GetDriver() => driver;
        public BrowserType? GetBrowserType() => type;

        public static By GetBy(FindType type, string value)
        {
            return type switch
            {
                FindType.Id => By.Id(value),
                FindType.Name => By.Name(value),
                FindType.ClassName => By.ClassName(value),
                FindType.CssSelector => By.CssSelector(value),
                FindType.LinkText => By.LinkText(value),
                FindType.PartialLinkText => By.PartialLinkText(value),
                FindType.TagName => By.TagName(value),
                FindType.XPath => By.XPath(value),
                _ => throw new NotImplementedException(),
            };
        }

        public void Close()
        {
            driver?.Close();
            driver?.Quit();
            instance = null;
        }

        public void Dispose()
        {
            driver?.Dispose();
            instance = null;
        }

        public void Navigate(string url)
        {
            driver?.Navigate().GoToUrl(url);
        }
        
        public bool Click(FindType type, string value)
        {
            if (driver == null) return false;
            IWebElement element = driver.FindElement(GetBy(type, value));
            if (element == null || !element.Enabled) return false;
            element.Click();
            return true;
        }

        public async Task CheckIfDownloadisFinished()
        {
            bool downloadFinished = false;
            switch (type)
            {
                case BrowserType.Chrome:
                case BrowserType.Edge:
                    while (!downloadFinished)
                    {
                        foreach (string fi in Directory.GetFiles(downloadpath))
                        {
                            downloadFinished = !Path.GetExtension(fi).Contains("crdownload");
                            if (downloadFinished)
                            {
                                break;
                            }
                        }
                        await Task.Delay(1000);
                    }
                    break;
                case BrowserType.Firefox:
                    while (!downloadFinished)
                    {
                        foreach (string fi in Directory.GetFiles(downloadpath))
                        {
                            downloadFinished = !Path.GetExtension(fi).Contains("part");
                            if (downloadFinished)
                            {
                                break;
                            }
                        }
                        await Task.Delay(1000);
                    }
                    break;
            }
        }

        public async Task DownloadFile(FindType type, string value)
        {
            if (driver == null) return;
            IWebElement element = driver.FindElement(GetBy(type, value));
            if (element == null) return;

            await WaitUnitEnabled(type, value);

            element.Click();
            await CheckIfDownloadisFinished();
        }
        
        public bool CheckIfLoaded(FindType type, string value)
        {
            if (driver == null) return false;
            try
            {
                driver.FindElement(GetBy(type, value));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public async Task WaitUnitLoaded(FindType type, string value)
        {
            if (driver == null) return;
            while (true)
            {
                if (CheckIfLoaded(type, value))
                {
                    break;
                }
                await Task.Delay(1000);
            }
        }

        public bool CheckIfEnabled(FindType type, string value)
        {
            if (driver == null) return false;
            try
            {
                return driver.FindElement(GetBy(type, value)).Enabled;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public async Task WaitUnitEnabled(FindType type, string value)
        {
            if (driver == null) return;
            while (true)
            {
                if (CheckIfEnabled(type, value))
                {
                    break;
                }
                await Task.Delay(1000);
            }
        }

        public bool CheckIfSelected(FindType type, string value)
        {
            if (driver == null) return false;
            try
            {
                return driver.FindElement(GetBy(type, value)).Selected;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public void SendKeys(FindType type, string value, string keys)
        {
            if (driver == null) return;
            driver.FindElement(GetBy(type, value)).SendKeys(keys);
        }

        public void Clear(FindType type, string value)
        {
            if (driver == null) return;
            driver.FindElement(GetBy(type, value)).Clear();
        }

        public void Select(FindType type, string value, string option)
        {
            if (driver == null) return;
            SelectElement select = new(driver.FindElement(GetBy(type, value)));
            select.SelectByText(option);
        }

        public void Select(FindType type, string value, int index)
        {
            if (driver == null) return;
            SelectElement select = new(driver.FindElement(GetBy(type, value)));
            select.SelectByIndex(index);
        }
        
        public void Deselect(FindType type, string value, string option)
        {
            if (driver == null) return;
            SelectElement selectElement = new(driver.FindElement(GetBy(type, value)));
            selectElement.DeselectByText(option);
        }

        public void Deselect(FindType type, string value, int index)
        {
            if (driver == null) return;
            SelectElement selectElement = new(driver.FindElement(GetBy(type, value)));
            selectElement.DeselectByIndex(index);
        }

        public void DeselectAll(FindType type, string value)
        {
            if (driver == null) return;
            SelectElement selectElement = new(driver.FindElement(GetBy(type, value)));
            selectElement.DeselectAll();
        }

        public void Check(FindType type, string value)
        {
            if (driver == null) return;
            IWebElement element = driver.FindElement(GetBy(type, value));
            if (!element.Selected)
            {
                element.Click();
            }
        }

        public void Uncheck(FindType type, string value)
        {
            if (driver == null) return;
            IWebElement element = driver.FindElement(GetBy(type, value));
            if (element.Selected)
            {
                element.Click();
            }
        }

        public bool InputText(FindType type, string value, string text)
        {
            if (driver == null) return false;
            try
            {
                IWebElement element = driver.FindElement(GetBy(type, value));
                element.SendKeys(text);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public List<string> GetOptions(FindType type, string value)
        {
            if (driver == null) return new();
            SelectElement select = new(driver.FindElement(GetBy(type, value)));
            return select.Options.Select(option => option.Text).ToList();
        }

        public string GetText(FindType type, string value)
        {
            if (driver == null) return string.Empty;
            return driver.FindElement(GetBy(type, value)).Text;
        }

        public string GetAttribute(FindType type, string value, string attribute)
        {
            if (driver == null) return string.Empty;
            return driver.FindElement(GetBy(type, value)).GetAttribute(attribute);
        }

        public string GetCssValue(FindType type, string value, string cssvalue)
        {
            if (driver == null) return string.Empty;
            return driver.FindElement(GetBy(type, value)).GetCssValue(cssvalue);
        }

        public string GetTitle()
        {
            if (driver == null) return string.Empty;
            return driver.Title;
        }

        public string GetUrl()
        {
            if (driver == null) return string.Empty;
            return driver.Url;
        }

        public async Task<(double, Dictionary<MethodType, (double, object?)>)> GetPerformActions(Dictionary<MethodType, object[]> functions)
        {
            if (driver == null) return (0, new());
            double totaltime = 0;
            Dictionary<MethodType, (double, object?)> times = new();
            foreach (MethodType type in functions.Keys)
            {
                object[] parameters = functions[type];
                object? result = null;
                Stopwatch stopwatch = new();
                stopwatch.Start();
                
                switch(type)
                {
                    case MethodType.Navigate:
                        Navigate((string)parameters[0]);
                        break;
                    case MethodType.Click:
                        result = Click((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.CheckIfDownloadisFinished:
                        await CheckIfDownloadisFinished();
                        break;
                    case MethodType.DownloadFile:
                        await DownloadFile((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.CheckIfLoaded:
                        result = CheckIfLoaded((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.CheckIfEnabled:
                        result = CheckIfEnabled((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.CheckIfSelected:
                        result = CheckIfSelected((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.SendKeys:
                        SendKeys((FindType)parameters[0], (string)parameters[1], (string)parameters[2]);
                        break;
                    case MethodType.Clear:
                        Clear((FindType)parameters[0], (string)parameters[1]);
                    break;
                    case MethodType.Select:
                        if (!int.TryParse((string)parameters[2], out int selectid))
                        {
                            Select((FindType)parameters[0], (string)parameters[1], (string)parameters[2]);
                        }
                        else
                        {
                            Select((FindType)parameters[0], (string)parameters[1], selectid);
                        }
                        break;
                    case MethodType.Deselect:
                        if (!int.TryParse((string)parameters[2], out int deselectid))
                        {
                            Deselect((FindType)parameters[0], (string)parameters[1], (string)parameters[2]);
                        }
                        else
                        {
                            Deselect((FindType)parameters[0], (string)parameters[1], deselectid);
                        }
                        break;
                    case MethodType.DeselectAll:
                        DeselectAll((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.Check:
                        Check((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.Uncheck:
                        Uncheck((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.InputText:
                        result = InputText((FindType)parameters[0], (string)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.GetOptions:
                        result = GetOptions((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.GetText:
                        result = GetText((FindType)parameters[0], (string)parameters[1]);
                        break;
                    case MethodType.GetAttribute:
                        result = GetAttribute((FindType)parameters[0], (string)parameters[1], (string)parameters[2]);
                        break;
                    case MethodType.GetCssValue:
                        result = GetAttribute((FindType)parameters[0], (string)parameters[1], (string)parameters[2]);
                        break;
                    case MethodType.GetTitle:
                        result = GetTitle();
                        break;
                    case MethodType.GetUrl:
                        result = GetUrl();
                        break;
                    case MethodType.Close:
                        Close();
                        break;
                }
                
                stopwatch.Stop();
                
                totaltime += stopwatch.Elapsed.TotalMilliseconds;
                times.Add(type, (stopwatch.Elapsed.TotalMilliseconds, result));
            }
            return (totaltime, times);
        }
    }
}
