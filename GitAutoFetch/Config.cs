using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GitAutoFetch
{
    public class Config
    {
        public int DefaultTime { get; set; }
        public int UserTime { get; set; }

        public Config(int defaultTime, int userTime)
        {
            DefaultTime = defaultTime;
            UserTime = userTime;
        }

        public int TimeValue()
        {
            return UserTime > 0 ? UserTime : DefaultTime;
        }

        public static string ReturnPath()
        {
            return $@"C:\Users\{Environment.UserName}\Documents\VsGitAutoFetch.json";
        }

        public static async Task VerifyConfigAsync(Config config)
        {
            string pathConfig = ReturnPath();

            if (File.Exists(pathConfig))
            {
                using (var r = new StreamReader(pathConfig))
                {
                    config = JsonConvert.DeserializeObject<Config>(await r.ReadToEndAsync());
                }
            }
            else
            {
                using (var w = new StreamWriter(pathConfig))
                {
                    string conf = JsonConvert.SerializeObject(config, Formatting.Indented);
                    await w.WriteAsync(conf);
                }
            }
        }

        public static void OpenConfigFile()
        {
            string pathConfig = ReturnPath();

            if (File.Exists(pathConfig))
            {
                string execute = $@"/C notepad {ReturnPath()}";

                Process.Start(new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = execute,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).Close();
            }
            else
            {
                throw new Exception("Configuration file not existing.\nClick first on Git AutoFetching");
            }
        }
    }
}
