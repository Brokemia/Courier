## Courier Mod Loader

A mod loader for Sabotage Studio's The Messenger.

#### Installation Instructions

##### Windows:

1. Dump the contents of a [release ZIP](https://github.com/Brokemia/Courier/releases/download/v0.2-alpha/Courier-v0.2-alpha.zip) into your TheMessenger folder.
2. MiniInstaller.exe needs to be in the TheMessenger folder. Run it.
3. Add Mod zip files to the Mods folder after installing the loader.

*Windows users may need to "unblock" the downloaded zip file in file explorer if your browser/os flags the download as "unsafe".*
##### Mac OS/Linux Based/*nix

Follow the Windows instructions but use [Mono](https://www.mono-project.com/) (Preferred) or [Wine](https://www.winehq.org/) to run the installer.

#### Creating a Mod for Courier
1) Install Courier
2) Make a .NET Framework library project
3) Add Assembly-CSharp.dll and MMHOOK_Assembly-CSharp.dll to your refrences 
(Located in the \The Messenger\TheMessenger_Data\Managed\ folder after installing Courier.)
4) Add your own code to methods by utilizing On.ClassName.MethodName += YourMethodHook; just like if you were to regester an event.

Here are some mods you can use as examples: 
[TrainerReborn](https://github.com/Brokemia/TrainerReborn), [NinjaInvis](https://github.com/Brokemia/NinjaInvis) and [NinjaAesthetic](https://github.com/Brokemia/NinjaAesthetic).

(Readme is a work in progress)
