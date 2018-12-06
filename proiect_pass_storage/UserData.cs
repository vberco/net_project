using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace proiect_pass_storage {
    [Serializable]
    [XmlRoot("UserData")]
    public class UserData {

        public UserCredentials Credentials {get; set; }
        [XmlArray("ResourseList"), XmlArrayItem(typeof(UserResourse), ElementName = "Resourse")]
        public List<UserResourse> Resourses { get; set; }
    }
}
