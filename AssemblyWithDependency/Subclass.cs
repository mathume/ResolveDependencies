using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dependency;

namespace AssemblyWithDependency
{
    public class Subclass : Newtonsoft.Json.JsonConverter, Interface
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get
            {
                return base.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return base.CanWrite;
            }
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public string Implements { get; private set; }
    }
}
