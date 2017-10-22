using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class Node
    {
        private static int COUNTER = 0;
        public enum NodeTypeEnum { Vav, Shaft, Other };

        public int Id { get; set; }
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public NodeTypeEnum NodeType { get; set; }

        public XYZ Location { get; set; }

        public String SpaceId { get; set; }

        public Node()
        {
            COUNTER++;
            Id = COUNTER;
        }
        public override string ToString()
        {
            return NodeType + ": " + Id + ": " + Name;
        }
    }


    #region CustomDeserializer
    public class XYZDeserializer: JsonCreationConverter<XYZ>
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected override XYZ Create(Type objectType, JObject jObject)
        {
            double x = Double.Parse(jObject["X"].ToString());
            double y = Double.Parse(jObject["Y"].ToString());
            double z = Double.Parse(jObject["Z"].ToString());

            return new XYZ(x, y, z);
            
        }

        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject[fieldName] != null;
        }
    }

    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">
        /// contents of JSON object that will be deserialized
        /// </param>
        /// <returns></returns>
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                         object existingValue,
                                         JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    }
    #endregion
}
