using System;
using System.Text;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public sealed class CharReplaceNaming : IDirectoryNaming
    {
        private readonly char splitter;
        private readonly char replace;
        private readonly int count;
        private readonly int minlength;

        public CharReplaceNaming(char splitter, char replace, int count, int minlength = 3)
        {
            this.splitter = splitter;
            this.replace = replace;
            this.count = count;
            this.minlength = minlength;
        }

        public string GetPath(string name)
        {
            if (name == null)
            {
                return null;
            }

            StringBuilder path = new StringBuilder(name);
            int times = 0;
            int pos = name.IndexOf(this.splitter.ToString(), StringComparison.Ordinal);

            while (pos >= this.minlength && times < this.count && name.Length > this.minlength*2)
            {
                path[pos] = this.replace;
                times++;
                pos = path.ToString().IndexOf(this.splitter.ToString(), StringComparison.Ordinal);
            }

            return path.ToString();
        }
    }
}