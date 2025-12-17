using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRM_Plugin.Module.PanelAdmin.Domain
{
    public class Actions
    {
        public ulong Actionid { get; set; }

        public int Componentid { get; set; }
        public string Actionname { get; set; } = string.Empty;
    }
}
