using System;
using System.IO;
using Microsoft.Win32;

/// <summary>
/// Everything for dealing with KSP itself.
/// </summary>

namespace CKAN {
    public class KSP {

        // Where to find KSP relative to Steam's root.
        // TODO: How do we make this variable immutable?
        static string steamKSP = Path.Combine( "SteamApps", "common", "Kerbal Space Program" );

        /// <summary>
        /// Finds Steam on the current machine.
        /// </summary>
        /// <returns>The path to steam, or null if not found</returns>
        static string SteamPath() {
            // First check the registry.

            string steam = (string) Microsoft.Win32.Registry.GetValue (@"HKEY_CURRENT_USER\Software\Valve\SteamPath", "", null);

            // If that directory exists, we've found steam!
            if (steam != null && FileSystem.IsDirectory(steam)) {
                return steam;
            }

            // Not in the registry, or missing file, but that's cool. This should find it on Linux/OSX

            steam = Path.Combine (
                Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                ".steam", "steam"
            );

            if (FileSystem.IsDirectory (steam)) {
                return steam;
            }

            // Nope, can't find steam.
            return null;
        }

        public static string gameDir() {

            // TODO: Cache the result of this.

            // TODO: See if KSP was specified on the command line.

            // TODO: See if KSP is in the same dir as we're installed (GH #23)

            // TODO: See if we've got it cached in the registry.

            // See if we can find KSP as part of a Steam install.

            string steam = SteamPath ();
            if (steam != null) {
                return Path.Combine (steam, steamKSP);
            }

            // Oh noes! We can't find KSP!

            throw new DirectoryNotFoundException ();

        }
    
        public static string gameData() {
            return Path.Combine (gameDir (), "GameData");
        }

        public static string ckanDir() {
            return Path.Combine (gameDir (), "CKAN");
        }

        public static string downloadCacheDir() {
            return Path.Combine (ckanDir (), "downloads");
        }

        public static string ships() {
            return Path.Combine (gameDir (), "Ships");
        }

        /// <summary>
        /// Create the CKAN directory and any supporting files.
        /// </summary>
        public static void init() {
            if (! Directory.Exists (ckanDir ())) {
                Console.WriteLine ("Setting up CKAN for the first time...");
                Console.WriteLine ("Creating {0}", ckanDir ());
                Directory.CreateDirectory (ckanDir ());

                Console.WriteLine ("Scanning for installed mods...");
                scanGameData ();
            }

            if (! Directory.Exists( downloadCacheDir() )) {
                Console.WriteLine ("Creating {0}", downloadCacheDir ());
                Directory.CreateDirectory (downloadCacheDir ());
            }
        }

        public static void scanGameData() {

            // TODO: Get rid of magic paths!
            RegistryManager registry_manager = RegistryManager.Instance();
            Registry registry = registry_manager.registry;

            // Forget that we've seen any DLLs, as we're going to refresh them all.
            registry.clear_dlls ();

            // TODO: It would be great to optimise this to skip .git directories and the like.
            // Yes, I keep my GameData in git.

            string[] dllFiles = Directory.GetFiles (gameData(), "*.dll", SearchOption.AllDirectories);

            foreach (string file in dllFiles) {
                // register_dll does the heavy lifting of turning it into a modname
                registry.register_dll (file);
            }

            registry_manager.save();
        }
    }
}