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
        /// <summary>
        /// New role name
        /// </summary>
		public string Name { internal get; set; }
        /// <summary>
        /// New role permissions
        /// </summary>
		public Permissions? Permissions { internal get; set; }
        /// <summary>
        /// New role color
        /// </summary>
		public DiscordColor? Color { internal get; set; }
        /// <summary>
        /// Whether new role should be hoisted
        /// </summary>
		public bool? Hoist { internal get; set; } //tbh what is hoist
        /// <summary>
        /// Whether new role should be mentionable
        /// </summary>
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
