## Release 5.0

### New Rules

 Rule ID | Category | Severity | Notes                                                                                                                       
---------|----------|----------|-----------------------------------------------------------------------------------------------------------------------------
 DSP0005 | Usage    | Warning  | `Permission.HasPermission` should always be prefered over bitwise operations                                                 
 DSP0006 | Design   | Warning  | Use `DiscordChannel#ModifyAsync` instead of `DiscordChannel#AddOverwriteAsnyc`                                              
 DSP0007 | Design   | Info     | Use a list request instead of fetching single entities inside of a loop                                                     
 DSP1001 | Usage    | Error    | A slash command explicitly registered to a guild should not specify DMs or user apps as installable context                 
 DSP1002 | Usage    | Warning  | Do not explicitly register nested classes of elsewhere-registered classes to DSharpPlus.Commands                            
 DSP1003 | Usage    | Error    | A command taking a specific context type should not be registerd as allowing processors whose contex type it doesn't support 