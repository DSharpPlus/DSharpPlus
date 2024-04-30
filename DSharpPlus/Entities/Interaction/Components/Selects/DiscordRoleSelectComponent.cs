namespace DSharpPlus.Entities;
using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class DiscordRoleSelectComponent : BaseDiscordSelectComponent
{
    [JsonProperty("default_values", NullValueHandling = NullValueHandling.Ignore)]
    private readonly List<DiscordSelectDefaultValue> _defaultValues = new();

    /// <summary>
    /// The default values for this component.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordSelectDefaultValue> DefaultValues => _defaultValues;

    /// <summary>
    /// Adds a default role to this component.
    /// </summary>
    /// <param name="role">Role to add</param>
    public DiscordRoleSelectComponent AddDefaultRole(DiscordRole role)
    {
        DiscordSelectDefaultValue defaultValue = new(role.Id, DiscordSelectDefaultValueType.Role);
        _defaultValues.Add(defaultValue);
        return this;
    }

    /// <summary>
    /// Adds a collections of DiscordRoles to this component.
    /// </summary>
    /// <param name="roles">Collection of DiscordRoles</param>
    public DiscordRoleSelectComponent AddDefaultRoles(IEnumerable<DiscordRole> roles)
    {
        foreach (DiscordRole value in roles)
        {
            DiscordSelectDefaultValue defaultValue = new(value.Id, DiscordSelectDefaultValueType.Role);
            _defaultValues.Add(defaultValue);
        }

        return this;
    }

    /// <summary>
    /// Adds a default role to this component.
    /// </summary>
    /// <param name="id">Id of a DiscordRole</param>
    public DiscordRoleSelectComponent AddDefaultRole(ulong id)
    {
        DiscordSelectDefaultValue defaultValue = new(id, DiscordSelectDefaultValueType.Role);
        _defaultValues.Add(defaultValue);
        return this;
    }

    /// <summary>
    /// Collections of role ids to add as default values.
    /// </summary>
    /// <param name="ids">Collection of DiscordRole ids</param>
    public DiscordRoleSelectComponent AddDefaultRoles(IEnumerable<ulong> ids)
    {
        foreach (ulong value in ids)
        {
            DiscordSelectDefaultValue defaultValue = new(value, DiscordSelectDefaultValueType.Role);
            _defaultValues.Add(defaultValue);
        }

        return this;
    }

    /// <summary>
    /// Enables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordRoleSelectComponent Enable()
    {
        Disabled = false;
        return this;
    }
    /// <summary>
    /// Disables this component.
    /// </summary>
    /// <returns>The current component.</returns>
    public DiscordRoleSelectComponent Disable()
    {
        Disabled = true;
        return this;
    }

    internal DiscordRoleSelectComponent() => Type = DiscordComponentType.RoleSelect;

    /// <summary>
    /// Creates a new role select component.
    /// </summary>
    /// <param name="customId">The ID of this component.</param>
    /// <param name="placeholder">Placeholder text that's shown when no options are selected.</param>
    /// <param name="disabled">Whether this component is disabled.</param>
    /// <param name="minOptions">The minimum amount of options to be selected.</param>
    /// <param name="maxOptions">The maximum amount of options to be selected, up to 25.</param>
    public DiscordRoleSelectComponent(string customId, string placeholder, bool disabled = false, int minOptions = 1, int maxOptions = 1)
        : base(DiscordComponentType.RoleSelect, customId, placeholder, disabled, minOptions, maxOptions) { }
}
