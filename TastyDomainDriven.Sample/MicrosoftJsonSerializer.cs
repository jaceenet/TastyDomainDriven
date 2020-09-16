using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using TastyDomainDriven.Serialization;

namespace TastyDomainDriven.Sample
{
    public class MicrosoftJsonSerializer<T> : IDtoSerializer<T>
    {
        private readonly Stream stream;

        public MicrosoftJsonSerializer(Stream stream)
        {
            this.stream = stream;
        }

        public async Task Save(T[] items)
        {
            await JsonSerializer.SerializeAsync(stream, items);
            await stream.FlushAsync();
        }

        public async Task<T[]> Load()
        {
            stream.Seek(0, SeekOrigin.Begin);
            return await JsonSerializer.DeserializeAsync<T[]>(stream);
        }
    }
}