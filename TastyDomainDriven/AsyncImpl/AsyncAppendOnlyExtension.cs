using System.Collections.Generic;
using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public static class AsyncAppendOnlyExtension
    {
        public static async Task CopyAppender(this IAppendOnlyAsync to, IAppendOnlyAsync @from)
        {
            var records = await @from.ReadRecords(0, int.MaxValue);

            var versions = new Dictionary<string, int>();

            foreach (var record in records)
            {
                if (!versions.ContainsKey(record.Name))
                {
                    versions[record.Name] = 0;
                }

                await to.Append(record.Name, record.Data, versions[record.Name]);
                versions[record.Name]++;
            }

        }
    }
}