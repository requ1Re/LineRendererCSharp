# LineRendererCSharp
A CSharp  implementation of a line renderer in Godot 4.0, useful for rendering cylindrical volumes such as lasers, trails, etc.
Based on the GDScript Godot 4 Version by LemiSt24 (https://godotengine.org/asset-library/asset/1348) which is based on the Godot 3.0 version by dbp8890 (https://github.com/dbp8890/line-renderer).

To use, simply download and unzip the folder into your `addons`-folder (`addons/LineRendererCSharp`).

To edit the line points, simply edit the `Points` member variable of the line renderer, and add/remove points from the array. This can also be done via the editor in Godot.


## Code Example
```csharp
LineRenderer LineRenderer = GetNode<LineRenderer>("LineRenderer");
LineRenderer.Points = [vector1, vector2];
```
