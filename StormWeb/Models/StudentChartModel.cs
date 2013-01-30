using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;

namespace StormWeb.Models
{
    [DataContract]
    public class StudentChartModel
    {
        [DataMember]
        List<string> range { get; set; }
        [DataMember]
        List<int> values { get; set; }

        public StudentChartModel()
        {
            range = new List<string>();
            values = new List<int>();
        }

        public void add(int val, string range)
        {
            values.Add(val);
            this.range.Add(range);
        }

        public string jsonSerializer()
        {
            var serializer = new DataContractJsonSerializer(this.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, this);
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}