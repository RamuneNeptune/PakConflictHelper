
using System.IO.Compression;

namespace PakConflictHelper
{
    class Program()
    {
        private static Dictionary<string, List<string>> FileMap = [];

        static void Main(string[] args)
        {
            var directory = Directory.GetCurrentDirectory();

            var paks = Directory.GetFiles(directory, "*.pak")
                .Where(p => !p.EndsWith("data0.pak") && !p.EndsWith("data1.pak"))
                .ToList();

            var pakNames = new List<string>();
            
            foreach(var pak in paks)
            {
                var filename = Path.GetFileName(pak);

                pakNames.Add(filename);
                
                try
                {
                    using ZipArchive archive = ZipFile.OpenRead(pak);
                    foreach(var entry in archive.Entries)
                    {
                        if(!entry.FullName.EndsWith('/'))
                        {
                            if(FileMap.TryGetValue(entry.FullName, out List<string>? value))
                            {
                                value.Add(filename);
                            }
                            else
                            {
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

            if(paks.Count < 1)
            {
                Log("> No .pak files found in current folder");

                Log("> Press ENTER to exit");

                Console.Read();

                return;
            }

            Log($"Files to check: {string.Join(", ", pakNames)}");

            var conflicts = FileMap.Where(kvp => kvp.Value.Count > 1).ToList();

            if(conflicts.Count != 0)
            {
                Log("");
                foreach (var conflict in conflicts)
                {
                    Log($"<!> {conflict.Key} @ {string.Join(" + ", conflict.Value)}");
                }
            }

            Log("");

            Log($"Finished! {conflicts.Count} conflicts found. Press ENTER to exit");

            Console.ReadLine();
        }


        static void Log(string text)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Console.WriteLine($"[{timestamp}] {text}");
        }
    }
}