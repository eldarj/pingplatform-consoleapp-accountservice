using System;
using System.Collections.Generic;
using System.Text;

namespace AccountMicroservice.Settings
{
    public class GatewayBaseSettings
    {
        public string Scheme { get; set; }
        public string Url { get; set; }
        public string Port { get; set; }
    }
}
