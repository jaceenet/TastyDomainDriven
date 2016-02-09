using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyDomainDriven.Serialization
{
    public class TableWriterForSerializer<T, TDto> : ITableReaderWriter<T>
    {
        private readonly ITableReaderWriter<T> internalReaderWriter;
        private readonly IDtoConverter<TDto, T> converter;
        private readonly IDtoSerializer<TDto> serializer;
        private bool isLoaded;

        public TableWriterForSerializer(ITableReaderWriter<T> 
            internalReaderWriter, 
            IDtoConverter<TDto, T> converter, 
            IDtoSerializer<TDto> serializer)
        {
            this.internalReaderWriter = internalReaderWriter;
            this.converter = converter;
            this.serializer = serializer;
            this.isLoaded = false;
        }

        public async Task<T> Get(T match)
        {
            await this.Load();
            return await this.internalReaderWriter.Get(match);
        }

        public async Task<List<T>> GetAll()
        {
            await this.Load();
            return await this.internalReaderWriter.GetAll();
        }

        public async Task InsertOrUpdate(T add, Func<T, T> update)
        {
            await this.internalReaderWriter.InsertOrUpdate(add, update);
            await this.Save();

        }

        public async Task<T> Remove(T key)
        {
            var item = await this.internalReaderWriter.Remove(key);
            await this.Save();
            return item;
        }

        private async Task Load()
        {
            if (!isLoaded)
            {
                var items = await serializer.Load();

                if (items == null || !items.Any())
                {                    
                    return;
                }

                foreach (var dto in items)
                {
                    Action<T> update = obj => { };
                    await this.internalReaderWriter.AddOrUpdate<T>(converter.Deserialize(dto), update);
                }

                isLoaded = true;
            }
        }

        private async Task Save()
        {
            var items = await this.internalReaderWriter.GetAll();
            await this.serializer.Save(items.Select(x => converter.Serialize(x)).ToArray());
        }
    }
}