using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRM_Plugin.Module.PanelAdmin.Domain
{
    public class Component
    {
        public int Componentid { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public int Parentid { get; set; }
    }
}
