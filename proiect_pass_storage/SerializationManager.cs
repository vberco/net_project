using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace proiect_pass_storage {
    public class SerializationManager {
       // private UserData UserData { get; set; }

        //public SerializationManager(UserData data) {
        //    UserData = data;
        //}

        public string SerializeUserData(UserData data)
        {
            using (System.IO.StringWriter writer = new System.IO.StringWriter()) {
                var serializer = new XmlSerializer(typeof(UserData));
                serializer.Serialize(writer, data);
                return writer.ToString();
            }
        }

        public UserData DeserializeUserData(string xml)
        {
            using (System.IO.StringReader reader = new System.IO.StringReader(xml)) {
                Type userData = typeof(UserData);
                var serializer = new XmlSerializer(userData);
                return (UserData)serializer.Deserialize(reader);
            }
        }
    }
}
