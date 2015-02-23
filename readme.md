NRefactory Code Completion Sample
=================================
This is a sample applications that shows how to do C# code completion in a text editor with NRefactory 5. We are using AvalonEdit as the text editor.

It's a pretty full featured sample containing ctrl+space like code completion and method parameter completion. Also showing nice icons for different types in the completion window.
The project is structured so that the code completion part could be used directly as a library. Also C# scripts are supported, meaning files (.csx) that contain no class structure but just C# statements.

To get it to work properly I had to compile the latest verion of AvalonEdit and include in this project.

See:
 * Discussion: http://community.sharpdevelop.net/forums/t/16621.aspx
 * https://github.com/icsharpcode/NRefactory
 * https://github.com/icsharpcode/SharpDevelop

![Screenshot](https://raw.github.com/lukebuehler/NRefactory-Completion-Sample/master/Doc/Screenshot.png)

Known Issues
=================================
This is a work in progress and so currently there are quite a few bugs/issues with the code completion. Hopefully they will be fixed soon.

  - show InsightWindows when hovering over a type or method with the mouse.
  - event completion

Attribution & License
=================================
Most of the code is extracted from different versions of SharpDevelop especially the newNR branch and NRefactory. So all the props go the the makers of NRefactory and SharpDevelop.
The completion icons come from SharpDevelop as well. 
The rest of the code was developed by lukebuehler and is released under the MIT license.

