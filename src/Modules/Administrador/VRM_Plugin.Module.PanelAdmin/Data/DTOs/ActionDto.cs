using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRM_Plugin.Module.PanelAdmin.Data.DTOs
{
    public class ActionDto
    {
        public ulong action_key_id { get; set; }

        public int component_id { get; set; }
        public string action_name { get; set; } = string.Empty;
    }
}
