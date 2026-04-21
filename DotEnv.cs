using System.Reflection;

namespace main
{

    public static class DotEnv
    {
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;

                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
        public static void LoadFromAssembly(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string[] resources = assembly.GetManifestResourceNames();
            var stream = assembly.GetManifestResourceStream((from r in resources where r.Contains(filename) select r).First());

            if (stream != null)
            {
                StreamReader reader = new StreamReader(stream);
                string[] lines = reader.ReadToEnd().Split('\n');

                foreach (var line in lines)
                {
                    var parts = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != 2)
                        continue;

                    Environment.SetEnvironmentVariable(parts[0], parts[1].Replace("\"", ""));
                }
            }
        }
    }
}
