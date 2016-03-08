using System;
using Microsoft.WindowsAzure.Storage;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public class ConcurrentIndexException : Exception
    {
        public ConcurrentIndexException(string message, StorageException ex) : base(message, ex)
        {            
        }
    }
}