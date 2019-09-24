using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GitAutoFetch
{
    public class Config
    {
        private const int DefaultTime = 5;
        public int UserTime { get; set; }
        private static Config _CurrentInstance;

        private Config() { }

        public static Config Instance
        {
            get
            {
                if (_CurrentInstance == null)
                    _CurrentInstance = new Config();

                return _CurrentInstance;
            }
        }

        public int TimeValue()
        {
            return _CurrentInstance.UserTime > 0 ? _CurrentInstance.UserTime : DefaultTime;
        }

        public string ReturnPath()
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\VsGitAutoFetch.json";
        }

        public async Task VerifyConfigAsync()
        {
            string pathConfig = ReturnPath();

            if (File.Exists(pathConfig))
            {
                using (var r = new StreamReader(pathConfig))
                {
                    _CurrentInstance = JsonConvert.DeserializeObject<Config>(await r.ReadToEndAsync());
                }
            }
            else
            {
                using (var w = new StreamWriter(pathConfig))
                {
                    string conf = JsonConvert.SerializeObject(_CurrentInstance, Formatting.Indented);
                    await w.WriteAsync(conf);
                }
            }
        }

        public void OpenConfigFile()
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
