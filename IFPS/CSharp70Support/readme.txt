Current C# version - 7.2

### How to install ###

1. Copy this CSharp70Support folder to your Unity project. It should be placed
   in the project's root, next to the Assets folder (not inside it!).

2. Import CSharp70Support/CSharpVNextSupport.unitypackage into your Unity project.

3. If you see any compilation errors at this point, restart the editor or run `Reimport All`.

4. Delete all the .csproj files in the project folder to force Unity recreate them.

5. [Optional] On Windows, run CSharp70Support/ngen install.cmd once with administrator
   privileges. It will precompile csc.exe and pdb2mdb.exe using Ngen, that will make compilation
   process in Unity a bit faster.


### History ###

CSharpVNextSupport 4.0.4

    - Switched from regular PDB to Portable PDB on all platforms for Unity 2018.1.0b12 or newer

CSharpVNextSupport 4.0.3

    - Unity 2018.1.0b10 support
    - Updated Roslyn to v2.7.0.62620

CSharpVNextSupport 4.0.2

    - Fixed macOS support

CSharpVNextSupport 4.0.1

    - Added missing reference to System.Xml.Linq.dll

CSharpVNextSupport 4.0.0

    - Unity 2018.1 support

    - The integration mechanism was changed. These's no longer need to mess with
      files in Unityinstallation folder. UnityEditor.dll is patched in memory
      using Harmony (https://github.com/pardeike/Harmony).