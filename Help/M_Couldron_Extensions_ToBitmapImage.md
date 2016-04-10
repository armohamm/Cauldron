# Extensions.ToBitmapImage Method 
 _**\[This is preliminary documentation and is subject to change.\]**_

Creates a new instance of BitmapImage and assigns the Stream to its StreamSource property 

 Returns null if *stream* is null.

**Namespace:**&nbsp;<a href="N_Couldron">Couldron</a><br />**Assembly:**&nbsp;Couldron (in Couldron.dll) Version: 1.0.0.0 (1.0.0.0)

## Syntax

**C#**<br />
``` C#
public static BitmapImage ToBitmapImage(
	this Stream stream
)
```


#### Parameters
&nbsp;<dl><dt>stream</dt><dd>Type: System.IO.Stream<br />The stream that contains an image</dd></dl>

#### Return Value
Type: BitmapImage<br />A new instance of BitmapImage

#### Usage Note
In Visual Basic and C#, you can call this method as an instance method on any object of type Stream. When you use instance method syntax to call this method, omit the first parameter. For more information, see <a href="http://msdn.microsoft.com/en-us/library/bb384936.aspx">Extension Methods (Visual Basic)</a> or <a href="http://msdn.microsoft.com/en-us/library/bb383977.aspx">Extension Methods (C# Programming Guide)</a>.

## See Also


#### Reference
<a href="T_Couldron_Extensions">Extensions Class</a><br /><a href="N_Couldron">Couldron Namespace</a><br />