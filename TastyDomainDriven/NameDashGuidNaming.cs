using System;
using TastyDomainDriven.Azure.AzureBlob;

namespace TastyDomainDriven
{
    public class NameDashGuidNaming : IAppenderNamingPolicy
    {
        private string prefix;

        public NameDashGuidNaming(string prefix = "")
        {
            this.prefix = prefix.Trim('/');

            if (prefix.Length > 0)
            {
                this.prefix = prefix + "/";
            }
        }

        public string GetMasterPath()
        {
            return prefix + "master.dat";
        }

        public string GetStreamPath(string streamid)
        {
            return prefix + Fix(streamid) + "_stream.dat";
        }

        public string GetIndexPath()
        {
            return prefix + "index.txt";
        }

        public string GetIndexPath(string streamid)
        {
            return prefix + Fix(streamid) + "_index.txt";
        }

        private string Fix(string streamid)
        {
            if (streamid == null)
            {
                throw new ArgumentNullException(nameof(streamid));
            }

            if (streamid.Length > 10)
            {
                var dash = streamid.IndexOf('-');

                if (dash > 3)
                {
                    var identity = streamid.Substring(0, dash);

                    Guid g;
                    if (Guid.TryParse(streamid.Substring(dash + 1), out g))
                    {
                        if (streamid.Equals(identity + "-" + g.ToString()) || streamid.Equals(identity + "-" + g.ToString("N")))
                        {
                            return string.Concat(identity, "/", g.ToString("N").Substring(0, 2), "/",
                                g.ToString("N").Substring(2));
                        }
                    }

                    return string.Concat(identity, "/", streamid.Substring(dash + 1));
                }
            }

            return streamid;
        }
    }
}