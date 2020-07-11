# Courier Mod Loader

A mod loader for Sabotage Studio's The Messenger. Join our [Discord Server](https://discord.gg/dm8Wbg) for support and additional discussion.

## Installation Instructions

1) Download Courier-vX.X.zip from our [releases](https://github.com/Brokemia/Courier/releases) page.
2) Move contents of the zip release file to your TheMessenger.exe folder and overwrite any files with the same name.
3) Run the MiniInstaller.exe. Use [Mono](https://www.mono-project.com/) (Preferred) or [Wine](https://www.winehq.org/) to run MiniInstaller.exe if you are on Mac OS/Linux/*nix.

*More detailed instructions can be found by clicking going to [this page](https://github.com/Brokemia/Courier/wiki/Installing-Courier/)*

### Installer not working?
* *If the files Assembly-CSharp.Postman.mm.dll and Assembly-CSharp.Postman.mm.pdb are in your \The Messenger\TheMessenger_Data\Managed folder from a previous release, you need to delete them before installing a new version.*
* *If you are using windows and your browser or OS claims the download is unsafe, you may need to "Unblock" the zip in File Explorer under file properties before unzipping.*

If you are still having issues with the installer, create an issue [here](https://github.com/Brokemia/Courier/issues).

## Creating a mod for Courier
1) Install Courier
2) Make a .NET Framework library project
3) Add Assembly-CSharp.dll and MMHOOK_Assembly-CSharp.dll to your references 
(Located in the \The Messenger\TheMessenger_Data\Managed\ folder after installing Courier.)
4) Add your own code to methods by utilizing On.ClassName.MethodName += YourMethodHook; just like if you were to register an event.

Here are some mods you can use as examples: 
[TrainerReborn](https://github.com/Brokemia/TrainerReborn), [NinjaInvis](https://github.com/Brokemia/NinjaInvis) and [NinjaAesthetic](https://github.com/Brokemia/NinjaAesthetic).

(Readme is a work in progress)
