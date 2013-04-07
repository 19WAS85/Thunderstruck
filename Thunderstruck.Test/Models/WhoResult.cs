using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thunderstruck.Test.Models
{
    public class WhoResult
    {
        public int spid { get; set; }

        public int ecid { get; set; }

        public string status { get; set; }

        public string loginame { get; set; }

        public string hostname { get; set; }

        public int blk { get; set; }

        public string dbname { get; set; }

        public string cmd { get; set; }

        public int request_id { get; set; }
    }
}
