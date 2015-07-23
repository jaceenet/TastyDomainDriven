namespace TastyDomainDriven
{
    using System;
    using System.Collections.Generic;

    public static class AppendOnlyStoreExtensions
    {
        public static void CopyAppender(this IAppendOnlyStore source, IAppendOnlyStore to)
        {
            CopyAppender(source, to, int.MaxValue);
        }

        public static void CopyAppender(this IAppendOnlyStore source, IAppendOnlyStore to, int version)
        {
            var serializer = new BinaryFormatterSerializer();

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (to == null)
            {
                throw new ArgumentNullException("to");
            }

            var versions = new Dictionary<string, int>();

            foreach (var record in source.ReadRecords(0, version))
            {
                if (!versions.ContainsKey(record.Name))
                {
                    versions[record.Name] = 0;
                }

                var events = serializer.DeserializeEvent(record.Data);
                to.Append(record.Name, record.Data, versions[record.Name]);
                versions[record.Name]++;
            }
        }
    }
}