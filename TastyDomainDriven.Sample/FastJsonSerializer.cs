using System.IO;
using System.Threading.Tasks;
using ServiceStack.Text;
using TastyDomainDriven.Serialization;

namespace TastyDomainDriven.Sample
{
    public class FastJsonSerializer<T> : IDtoSerializer<T>
    {
        private readonly Stream stream;

        public FastJsonSerializer(Stream stream)
        {
            this.stream = stream;
        }

        public async Task Save(T[] items)
        {
            JsonSerializer.SerializeToStream(items, stream);
            await stream.FlushAsync();            
        }

        public Task<T[]> Load()
        {
            return Task.FromResult(JsonSerializer.DeserializeFromStream<T[]>(stream));
        }
    }
}