using System.IO;

namespace TastyDomainDriven.Projections
{
    public class FileViewProvider : BaseViewProvider
    {
        private string _path;

        public FileViewProvider(string path)
        {
            _path = path;

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
        }

        protected override ITableReaderWriter<T> Create<T>(string viewname)
        {
            return null;
            //return new JsonTableReaderWriter<T>(Path.Combine(_path, viewname + ".json"), new Projections.MemoryTableWriter<T>());
        }
    }
}