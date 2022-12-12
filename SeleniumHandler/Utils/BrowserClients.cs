using SeleniumHandler.Enums;

namespace SeleniumHandler.Utils
{
    public abstract class BrowserClients
    {
        public string DownloadPath = "";
        public bool Silent = false;
        public BrowserType ?Type = null;

        public BrowserClients(string downloadPath = "", bool silent = false)
        {
            DownloadPath = downloadPath;
            Silent = silent;
        }
    }
    public class Chrome : BrowserClients
    {
        public Chrome(string downloadPath = "", bool silent = false) : base(downloadPath, silent)
        {
            Type = BrowserType.Chrome;
        }
    }

    public class Firefox : BrowserClients
    {
        public Firefox(string downloadPath = "", bool silent = false) : base(downloadPath, silent)
        {
            Type = BrowserType.Firefox;
        }
    }

    public class Edge : BrowserClients
    {
        public Edge(string downloadPath = "", bool silent = false) : base(downloadPath, silent)
        {
            Type = BrowserType.Edge;
        }
    }
}
