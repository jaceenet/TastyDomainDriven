namespace TastyDomainDriven
{
    using System;

    public interface ICommand
    {
        DateTime Timestamp { get; set; }
    }
}