using System;
using TastyDomainDriven.Sample.Properties;

namespace TastyDomainDriven.Sample.Commands
{
    public class SayCommand : ICommand
    {
        public PersonId PersonId { get; set; }

        public string Say { get; set; }

        public DateTime Timestamp { get; set; }
    }
}