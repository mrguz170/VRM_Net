using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRM_Plugin.Module.PanelAdmin.Data.DTOs
{
    public class ComponentDto
    {
        public int component_id { get; set; }
        public string name { get; set; } = string.Empty;
        public int module_id { get; set; }
        public int parent_id { get; set; }
    }
}
