# MessageDialog.ShowYesNoCancelAsync Method (String, String, Action, Action, Action)
 _**\[This is preliminary documentation and is subject to change.\]**_

Begins an asynchronous operation showing a dialog with a Yes, No and Cancel button.

**Namespace:**&nbsp;<a href="N_Couldron">Couldron</a><br />**Assembly:**&nbsp;Couldron (in Couldron.dll) Version: 1.0.0.0 (1.0.0.0)

## Syntax

**C#**<br />
``` C#
public static Task ShowYesNoCancelAsync(
	string title,
	string content,
	Action commandYes,
	Action commandNo,
	Action commandCancel
)
```


#### Parameters
&nbsp;<dl><dt>title</dt><dd>Type: System.String<br />The title to display on the dialog, if any.</dd><dt>content</dt><dd>Type: System.String<br />The message to be displayed to the user.</dd><dt>commandYes</dt><dd>Type: System.Action<br />The action associated with the Yes Command</dd><dt>commandNo</dt><dd>Type: System.Action<br />The action associated with the No Command</dd><dt>commandCancel</dt><dd>Type: System.Action<br />The action associated with the Cancel Command</dd></dl>

#### Return Value
Type: Task<br />An object that represents the asynchronous operation.

## See Also


#### Reference
<a href="T_Couldron_MessageDialog">MessageDialog Class</a><br /><a href="Overload_Couldron_MessageDialog_ShowYesNoCancelAsync">ShowYesNoCancelAsync Overload</a><br /><a href="N_Couldron">Couldron Namespace</a><br />