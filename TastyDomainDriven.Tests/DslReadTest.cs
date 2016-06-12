using System;
using TastyDomainDriven.Dsl;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class DslReadTest
    {
        [Fact]
        public void ReadSimpleType()
        {
            var content = @"#byte;0;System;{0}.Write({1});{0}.ReadByte()";

            var dsl = new EventDslReader();

            var lines = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            dsl.ParseLines(lines);

            Assert.Equal(0, dsl.GetEventObjects().Count);
        }

        [Fact]
        public void ReadSimpleEvent()
        {
            var content = @"MyEvent;199;4=Name:string";

            var dsl = new EventDslReader();
            dsl.AddCsharpDefaults();

            var lines = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            dsl.ParseLines(lines);

            Assert.Equal(1, dsl.GetEventObjects().Count);
            Assert.Equal(199, dsl.GetEventObjects()[0].ClassId);
            Assert.Equal("MyEvent", dsl.GetEventObjects()[0].ClassName);

            Assert.Equal(1, dsl.GetEventObjects()[0].PrivateMembers.Count);
            Assert.Equal("Name", dsl.GetEventObjects()[0].PrivateMembers[0].PropertyName);
            Assert.Equal("_name", dsl.GetEventObjects()[0].PrivateMembers[0].PrivateField);
            Assert.Equal(4, dsl.GetEventObjects()[0].PrivateMembers[0].Propertyid);
        }

        [Fact]
        public void ReadBasicEvent()
        {
            var content = @"
//Types defined:
#UserId;UserId.Empty;MyNamespace;binto{0}{1};binfrom{0}{1}

//My events:
MyNewEvent;199;1=Id:UserId;2=EventId:Guid;3=Timestamp:DateTime";

            var dsl = new EventDslReader();
            dsl.AddCsharpDefaults();

            var lines = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            dsl.ParseLines(lines);

            Assert.Equal(1, dsl.GetEventObjects().Count);
            Assert.Equal(199, dsl.GetEventObjects()[0].ClassId);
            Assert.Equal("MyNewEvent", dsl.GetEventObjects()[0].ClassName);

            Assert.Equal(3, dsl.GetEventObjects()[0].PrivateMembers.Count);
        }
    }
}