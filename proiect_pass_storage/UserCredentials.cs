using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proiect_pass_storage {

    [Serializable]
   public class UserCredentials {
        public string Name { get; set; }
        public string Password { get; set; }
    }

}
