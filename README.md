# MGizmosCSharp

v1.1

Debug Gizmos for Godot written in C# for both runtime and in editor use.

# Install

place all repo files in /addons/MGizmosCSharp

# How to use

In code add the namespace with "using MGizmosCSharp" then call methods in the static class GizmoUtils. Like "GizmoUtils.DrawLine".

When plugin is loaded you can also add line and shape nodes through the add child node menu. Remember that the line node needs a start and end target.

Make use of the timing settings to save performance. Rebuilding the gizmos are expensive so if you have a lot of them, saving performance is important.


![Demo](https://github.com/MuNgLo/MGizmosCSharp/blob/main/ExamplePictures/Shapes.png)
![Demo](https://github.com/MuNgLo/MGizmosCSharp/blob/main/ExamplePictures/Line.png)
![Demo](https://github.com/MuNgLo/MGizmosCSharp/blob/main/ExamplePictures/CircleAsTriangle.png)

