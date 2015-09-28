using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TastyDomainDriven.Azure
{
	internal class BlobLease : IDisposable
    {
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(BlobLease));

        private readonly CloudBlobContainer container;
        private readonly string instanceId = Guid.NewGuid().ToString("N");

        private string lease;

        public BlobLease(CloudBlobContainer container)
        {
            this.container = container;
        }

        public Exception Exception { get; set; }

        public void AquireLease()
        {
            //var blobReference = this.container.GetBlobReferenceFromServer("lock");

            Logger.Debug("Requesting lease....");
            this.lease = this.container.AcquireLease(TimeSpan.FromSeconds(15), this.instanceId);

            if (string.IsNullOrEmpty(lease))
            {
                this.Exception = new Exception("Could not aquire lease.");
                Logger.Info("Got exception on lease: ", this.Exception);
            }
            else
            {
                this.Exception = null;
                Logger.DebugFormat("Got lease.... {0}", this.lease);
            }
        }

        public void Dispose()
        {
            
            if (this.container != null && !string.IsNullOrEmpty(this.lease))
            {
                Logger.DebugFormat("Breaking lease {0}", this.lease);
                this.container.BreakLease(TimeSpan.FromSeconds(20));
            }
        }
    }
}