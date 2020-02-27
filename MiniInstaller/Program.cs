﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using Mod.Courier.Helpers;

namespace MiniInstaller {
    // Adapted from the Everest MiniInstaller
    // https://github.com/EverestAPI/Everest
    public class Program {

        public static string PathUpdate;
        public static string PathManaged;
        public static string PathAssemblyDll;
        public static string PathOrig;
        public static string PathLog;

        public static Assembly AsmMonoMod;
        public static Assembly AsmHookGen;

        // This can be set from the in-game installer via reflection.
        public static Action<string> LineLogger;
        public static void LogLine(string line) {
            LineLogger?.Invoke(line);
            Console.WriteLine(line);
        }

        public static int Main(string[] args) {
            Console.WriteLine("Courier MiniInstaller");

            SetupPaths();

            if (File.Exists(PathLog))
                File.Delete(PathLog);
            using (Stream fileStream = File.OpenWrite(PathLog))
            using (StreamWriter fileWriter = new StreamWriter(fileStream, Console.OutputEncoding))
            using (LogWriter logWriter = new LogWriter {
                STDOUT = Console.Out,
                File = fileWriter
            }) {
                Console.SetOut(logWriter);

                try {

                    WaitForGameExit();

                    Backup();

                    MoveFilesFromUpdate();

                    if (AsmMonoMod == null) {
                        LoadMonoMod();
                    }
                    RunMonoMod(Path.Combine(PathOrig, "Assembly-CSharp.dll"), PathAssemblyDll);
                    RunHookGen(PathAssemblyDll, PathAssemblyDll);
                    MakeLargeAddressAware(PathAssemblyDll);

                    // If we're updating, start the game. Otherwise, close the window. 
                    if (PathUpdate != null) {
                        StartGame();
                    }

                } catch (Exception e) {
                    LogLine("");
                    LogLine(e.ToString());
                    LogLine("");
                    LogLine("Installing Courier failed.");
                    LogLine("Please create a new issue on GitHub @ https://github.com/Brokemia/Courier");
                    //LogLine("or join the #game_modding channel on Discord (invite in the repo).");
                    LogLine("Make sure to upload your miniinstaller-log.txt");
                    return 1;
                }

                Console.SetOut(logWriter.STDOUT);
                logWriter.STDOUT = null;
            }

            return 0;
        }

        public static void SetupPaths() {
            PathManaged = Path.Combine(Directory.GetCurrentDirectory(), "TheMessenger_Data", "Managed");

            if (Path.GetFileName(PathManaged) == "Courier-update" &&
                File.Exists(Path.Combine(Path.GetDirectoryName(PathManaged), "Assembly-CSharp.dll"))) {
                // We're updating Courier via a hypothetical future in-game installler.
                PathUpdate = PathManaged;
                PathManaged = Path.GetDirectoryName(PathUpdate);
            }

            PathAssemblyDll = Path.Combine(PathManaged, "Assembly-CSharp.dll");
            if (!File.Exists(PathAssemblyDll)) {
                LogLine("Assembly-CSharp.dll not found!");
                LogLine("Did you extract the .zip into the Managed folder?");
                return;
            }

            PathOrig = Path.Combine(PathManaged, "orig");
            PathLog = Path.Combine(PathManaged, "miniinstaller-log.txt");

            if (!Directory.Exists(Path.Combine(PathManaged, "../../Mods"))) {
                LogLine("Creating Mods directory");
                Directory.CreateDirectory(Path.Combine(PathManaged, "../../Mods"));
            }
        }

        public static void WaitForGameExit() {
            if (!CanReadWrite(PathAssemblyDll)) {
                LogLine("Assembly-CSharp.dll not read-writeable - waiting");
                while (!CanReadWrite(PathAssemblyDll))
                    Thread.Sleep(5000);
            }
        }

        public static void Backup() {
            // Backup / restore the original game files we're going to modify.
            // TODO: Maybe invalidate the orig dir when the backed up version < installed version?
            if (!Directory.Exists(PathOrig)) {
                LogLine("Creating backup orig directory");
                Directory.CreateDirectory(PathOrig);
                File.Copy(PathAssemblyDll, Path.Combine(PathOrig, "Assembly-CSharp.dll"));
                if (File.Exists(PathAssemblyDll + ".pdb"))
                    File.Copy(PathAssemblyDll + ".pdb", Path.Combine(PathOrig, "Assembly-CSharp.dll.pdb"));
                if (File.Exists(Path.ChangeExtension(PathAssemblyDll, "mdb")))
                    File.Copy(Path.ChangeExtension(PathAssemblyDll, "mdb"), Path.Combine(PathOrig, "Assembly-CSharp.mdb"));
            }
        }

        public static void MoveFilesFromUpdate() {
            if (PathUpdate != null) {
                LogLine("Moving files from update directory");
                foreach (string fileUpdate in Directory.GetFiles(PathUpdate)) {
                    string fileRelative = fileUpdate.Substring(PathUpdate.Length + 1);
                    string fileGame = Path.Combine(PathManaged, fileRelative);
                    LogLine($"Copying {fileUpdate} +> {fileGame}");
                    File.Copy(fileUpdate, fileGame, true);
                }
            }
        }

        public static void LoadMonoMod() {
            // We can't add MonoMod as a reference to MiniInstaller, as we don't want to accidentally lock the file.
            // Instead, load it dynamically and invoke the entry point.
            // We also need to lazily load any dependencies.
            LogLine("Loading Mono.Cecil");
            LazyLoadAssembly(Path.Combine(PathManaged, "Mono.Cecil.dll"));
            LogLine("Loading Mono.Cecil.Mdb");
            LazyLoadAssembly(Path.Combine(PathManaged, "Mono.Cecil.Mdb.dll"));
            LogLine("Loading Mono.Cecil.Pdb");
            LazyLoadAssembly(Path.Combine(PathManaged, "Mono.Cecil.Pdb.dll"));
            LogLine("Loading MonoMod.Utils.dll");
            LazyLoadAssembly(Path.Combine(PathManaged, "MonoMod.Utils.dll"));
            LogLine("Loading MonoMod");
            AsmMonoMod = LazyLoadAssembly(Path.Combine(PathManaged, "MonoMod.exe"));
            LogLine("Loading MonoMod.RuntimeDetour.dll");
            LazyLoadAssembly(Path.Combine(PathManaged, "MonoMod.RuntimeDetour.dll"));
            LogLine("Loading MonoMod.RuntimeDetour.HookGen");
            AsmHookGen = LazyLoadAssembly(Path.Combine(PathManaged, "MonoMod.RuntimeDetour.HookGen.exe"));
        }

        public static void RunMonoMod(string asmFrom, string asmTo = null) {
            if (asmTo == null)
                asmTo = asmFrom;
            LogLine($"Running MonoMod for {asmFrom}");
            // We're lazy.
            Environment.SetEnvironmentVariable("MONOMOD_DEPDIRS", PathManaged);
            Environment.SetEnvironmentVariable("MONOMOD_MODS", PathManaged);
            Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "0");
            AsmMonoMod.EntryPoint.Invoke(null, new object[] { new string[] { asmFrom, asmTo + ".tmp" } });

            if (!File.Exists(asmTo + ".tmp"))
                throw new Exception("MonoMod failed creating a patched assembly!");

            if (File.Exists(asmTo))
                File.Delete(asmTo);
            File.Move(asmTo + ".tmp", asmTo);
        }

        public static void RunHookGen(string asm, string target) {
            LogLine($"Running MonoMod.RuntimeDetour.HookGen for {asm}");
            // We're lazy.
            Environment.SetEnvironmentVariable("MONOMOD_DEPDIRS", PathManaged);
            Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "0");
            AsmHookGen.EntryPoint.Invoke(null, new object[] { new string[] { "--private", asm, Path.Combine(Path.GetDirectoryName(target), "MMHOOK_" + Path.ChangeExtension(Path.GetFileName(target), "dll")) } });
        }

        // Based on https://github.com/RomSteady/RomTerraria/blob/c017139c54b82fa86c1e645be5b51656e637d449/RTRewriter/Cecil/Rewriter.cs#L37
        public static void MakeLargeAddressAware(string asm) {
            // https://docs.microsoft.com/en-us/windows/desktop/debug/pe-format#characteristics
            const ushort IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020;
            using (FileStream stream = File.Open(asm, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(stream))
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                // Check for MZ header.
                if (reader.ReadInt16() != 0x5A4D)
                    return;

                // Skip to the PE header, then check for it.
                reader.BaseStream.Position = 0x3C;
                reader.BaseStream.Position = reader.ReadInt32();
                if (reader.ReadInt32() != 0x4550)
                    return;

                // Go to the "characteristics" in the header.
                reader.BaseStream.Position += 0x12;

                // Set the IMAGE_FILE_LARGE_ADDRESS_AWARE flag if not already set.
                long pos = reader.BaseStream.Position;
                ushort flags = reader.ReadUInt16();
                if ((flags & IMAGE_FILE_LARGE_ADDRESS_AWARE) == IMAGE_FILE_LARGE_ADDRESS_AWARE)
                    return;

                flags |= IMAGE_FILE_LARGE_ADDRESS_AWARE;

                writer.Seek((int)pos, SeekOrigin.Begin);
                writer.Write(flags);
                writer.Flush();
            }
        }

        public static void StartGame() {
            LogLine("Automatic restart not yet implemented");
            //LogLine("Restarting The Messenger");
            //Process game = new Process();
            // If the game was installed via Steam, it should restart in a Steam context on its own.
            // TODO Figure out how to restart
            /*if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX) {
                // The Linux and macOS versions come with a wrapping bash script.
                game.StartInfo.FileName = PathEverestExe.Substring(0, PathEverestExe.Length - 4);
                if (!File.Exists(game.StartInfo.FileName))
                    game.StartInfo.FileName = PathAssemblyDll.Substring(0, PathAssemblyDll.Length - 4);
            } else {
                game.StartInfo.FileName = PathEverestExe;
            }
            game.StartInfo.WorkingDirectory = PathManaged;
            game.Start();*/
        }


        // AFAIK there's no "clean" way to check for any file locks in C#.
        static bool CanReadWrite(string path) {
            try {
                new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete).Dispose();
                return true;
            } catch {
                return false;
            }
        }

        static Assembly LazyLoadAssembly(string path) {
            LogLine($"Lazily loading {path}");
            ResolveEventHandler tmpResolver = (s, e) => {
                string asmPath = Path.Combine(Path.GetDirectoryName(path), new AssemblyName(e.Name).Name + ".dll");
                if (!File.Exists(asmPath))
                    return null;
                return Assembly.LoadFrom(asmPath);
            };
            AppDomain.CurrentDomain.AssemblyResolve += tmpResolver;
            Assembly asm = Assembly.Load(Path.GetFileNameWithoutExtension(path));
            AppDomain.CurrentDomain.AssemblyResolve -= tmpResolver;
            AppDomain.CurrentDomain.TypeResolve += (s, e) => {
                return asm.GetType(e.Name) != null ? asm : null;
            };
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) => {
                return e.Name == asm.FullName || e.Name == asm.GetName().Name ? asm : null;
            };
            return asm;
        }

    }
}
