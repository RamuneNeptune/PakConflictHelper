using System.IO.Compression;

namespace PakConflictHelper
{
    class Program()
    {
        private static readonly Dictionary<string, List<string>> FileMap = [];

        static void Main(string[] args)
        {
            // Get the current folder
            var directory = Directory.GetCurrentDirectory();

            // Get all .pak files in the current folder
            var paks = Directory.GetFiles(directory, "*.pak")
                // Ignore checking the game source .pak files
                .Where(p => !p.EndsWith("data0.pak") && !p.EndsWith("data1.pak"))
                .ToList();

            // If there are no .pak files in the current folder, exit
            if(paks.Count < 1)
            {
                Log("> No .pak files found in current folder");
                Log("> Press ENTER to exit");
                Console.ReadLine();
                return;
            }

            List<string> pakNames = [];
            
            // Loop through the .pak files that were found in the current folder
            foreach(var pak in paks)
            {
                // Get the current filename (e.g. "data2.pak")
                var filename = Path.GetFileName(pak);

                // Then store it to log later
                pakNames.Add(filename);

                try
                {
                    // Open the .pak as a .zip file, as that's what they actually are, and loop through all entries (files/folders) within
                    using ZipArchive archive = ZipFile.OpenRead(pak);
                    foreach(var entry in archive.Entries)
                    {
                        // Check if the current entry is a file (not a folder)
                        if(!entry.FullName.EndsWith('/'))
                        {
                            // Check if this entry (e.g. "item_sets.loot") is already mapped to another .pak
                            if(FileMap.TryGetValue(entry.FullName, out List<string>? pakList))
                            {
                                // This entry (e.g. "item_sets.loot") already appears in another .pak
                                // Append the current .pak name (e.g. "data4.pak") to the list of .pak files that contain the entry (e.g. now ["data3.pak", "data4.pak"])
                                pakList.Add(filename);
                            }
                            else
                            {
                                // This entry (e.g. "item_sets.loot") is currently unique to this .pak
                                FileMap[entry.FullName] = [filename];
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log($"Error opening {filename}: {ex.Message}");
                }
            }

            // Log the list of .pak files that were found in the current folder
            Log($"> Files to check: {string.Join(", ", pakNames)}");

            // Select all the conflicts from FileMap
            var conflicts = FileMap.Where(kvp => kvp.Value.Count > 1).ToList();

            // Check if there were any conflicts found
            if(conflicts.Count > 0)
            {
                // Log each conflict as "<!> [conflicting file] @ [conflicting pak] + [conflicting pak] + ..."
                Log("");
                conflicts.ForEach(c => Log($"<!> {c.Key} @ {string.Join(" + ", c.Value)}"));
            }

            Log("");
            Log($"> Finished! {conflicts.Count} conflicts found. Press ENTER to exit");

            // Finished!
            Console.ReadLine();
        }

        static void Log(string text) => Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}");
    }
}