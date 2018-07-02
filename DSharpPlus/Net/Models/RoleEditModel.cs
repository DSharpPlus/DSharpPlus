using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Models
{
    public class RoleEditModel : BaseEditModel
    {
		public string Name { internal get; set; }
		public Permissions? Permissions { internal get; set; }
		public DiscordColor? Color { internal get; set; }
		public bool? Hoist { internal get; set; }
		public bool? Mentionable { internal get; set; }

		internal RoleEditModel()
		{
			this.Name = null;
			this.Permissions = null;
			this.Color = null;
			this.Hoist = null;
			this.Mentionable = null;
		}
    }
}
