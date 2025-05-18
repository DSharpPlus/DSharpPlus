## Release 5.0

### New Rules

 Rule ID | Category | Severity | Notes                                                                                                                         
---------|----------|----------|-------------------------------------------------------------------------------------------------------------------------------
 DSP0006 | Usage    | Warning  | `DiscordPermissions.HasPermission` should always be preferred over bitwise operations                                         
 DSP0007 | Design   | Warning  | Use `DiscordChannel#ModifyAsync` instead of `DiscordChannel#AddOverwriteAsnyc`                                                
 DSP0008 | Design   | Info     | Use a list request instead of fetching single entities inside of a loop                                                       
 DSP0009 | Usage    | Warning  | Use `DiscordPermissions` instead of operating on `DiscordPermission`
 DSP0010 | Usage    | Info     | Use `DiscordPermissions` methods and math operations instead of bitwise operations
 DSP1001 | Usage    | Error    | A slash command explicitly registered to a guild should not specify DMs or user apps as installable context                   
 DSP1002 | Usage    | Warning  | Do not explicitly register nested classes of elsewhere registered classes to DSharpPlus.Commands                              
 DSP1003 | Usage    | Error    | A command taking a specific context type should not be registered as allowing processors whose contex type it doesn't support