using DataTypes;
using DataTypes.SetText;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Utils
{
    public class AutoUpdate
    {
        ///<returns>whether the app updated or not</returns>
        public static async Task<string> Update(string currentVersion) {
            var spinner = new Cli.Animatables.Spinner();

            // get and log latest version
            var latestRelease = await GetLatestRelease();
            var latestVersion = latestRelease.Substring(latestRelease.IndexOf("/v") + 2);
            FS.FileSystem.Log($"Latest version: {latestVersion}");

            // return if latest version failed to fetch
            if (latestRelease == "0") {
                spinner.Cancel();
                return "Failed to fetch latest version";
            }

            spinner.Cancel();
            // if never auto update isnt on, continue
            if (!DataTypes.Config.v.NeverAutoUpdate) {

                // ask to update if always auto update is off
                if (!DataTypes.Config.v.AlwaysAutoUpdate) {

                    // give selection prompt
                    var selectionResult = new Cli.Animatables.SelectionPrompt(
                        $"{SetText.Blue}snipesharp v{latestVersion}{SetText.ResetAll} is available. Do you want to update?",
                        new string[]{ "Yes", "No", "Always", "Never" }
                    ).result;
                    
                    // handle selection prompt result
                    if (selectionResult == "Always") Config.v.AlwaysAutoUpdate = true;
                    if (selectionResult == "Never") {
                        Config.v.NeverAutoUpdate = true;
                        Cli.Output.Inform("If you change your mind, you can disable NeverAutoUpdate in config.json");
                        FS.FileSystem.UpdateConfig();
                        return "User chose to never update, not updating";
                    }
                    if (selectionResult == "No") return "User chose not to update, not updating";
                    FS.FileSystem.UpdateConfig();
                }

                var spinnerTwo = new Cli.Animatables.Spinner();

                // update
                // get appropriate latest version download
                var downloadLink = GetDownloadLink(latestRelease, latestVersion);
                if (downloadLink == "0") { 
                    spinnerTwo.Cancel();
                    return "Failed to construct download link";
                }

                // create download path
                string fileName = downloadLink.Contains(".exe") ? $"snipesharp_v{latestVersion}.exe" : $"snipesharp_v{latestVersion}";
                string currentFilePath = Process.GetCurrentProcess().MainModule!.FileName!;
                string path = String.Join
                (Path.DirectorySeparatorChar, currentFilePath.Split(Path.DirectorySeparatorChar).Reverse().Skip(1).Reverse())
                + Path.DirectorySeparatorChar + fileName;

                // download file, return if download fails
                if (!await FS.FileSystem.Download(downloadLink, path)) {
                    spinnerTwo.Cancel();
                    return $"Failed to download {latestVersion} from {downloadLink}";
                }
                FS.FileSystem.Log($"Successfully downloaded {latestVersion} from {downloadLink}");
                
                spinnerTwo.Cancel();
                // ask whether to restart snipesharp under the new version
                var restartPromptResult = new Cli.Animatables.SelectionPrompt
                ("Restart snipesharp to run the latest version?", new string[]{"Yes", "No"}).result;

                // handle restart prompt
                await HandleRestartPrompt(restartPromptResult, path);

                return $"Succesfully updated to the latest version ({latestVersion})";
            }
            
            // if never auto update is on, return
            return "Never auto update is on, not updating";
        }
        private static async Task HandleRestartPrompt(string result, string filePath) {
            
            // make app runnable for unix systems
            if (Cli.Core.pid == PlatformID.Unix) {
                try {
                    await Process.Start(new ProcessStartInfo{
                        FileName = "/bin/bash",
                        Arguments = "-c \" " + $"chmod +x {filePath}" + " \"",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    })!.WaitForExitAsync();

                    Cli.Output.Inform($"Running: -c \"sudo {filePath}\" | without --auto-update=\"{Process.GetCurrentProcess().MainModule!.FileName}\"");

                    // start the new file with --auto-update arg as sudo
                    var psi = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = string.Format("-c \"sudo {0} {1}\"", filePath, $"--auto-update=\"{Process.GetCurrentProcess().MainModule!.FileName}\"")
                    };

                    using (var p = Process.Start(psi))
                    {
                        if (p != null)
                        {
                            var strOutput = p.StandardOutput.ReadToEnd();
                            p.WaitForExit();
                        }
                    }
                }
                catch { FS.FileSystem.Log($"Failed to run \"chmod +x {filePath}\""); }
            }
            else { // windows
                // start the new file with --auto-update arg
                Process.Start(new ProcessStartInfo{
                    FileName = filePath,
                    Arguments = $"--auto-update=\"{Process.GetCurrentProcess().MainModule!.FileName}\""
                });
            }

            // kill current process
            Process.GetCurrentProcess().Kill();
        }
        private static string GetDownloadLink(string release, string version) {
            try {
                // get cpu architecture
                string cpuArch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x86-64";

                // set os string or default to 
                string os = "linux-redhat";
                if (System.OperatingSystem.IsMacOS()) os = "mac-os";
                if (System.OperatingSystem.IsWindows()) os = "win";
                if (System.OperatingSystem.IsLinux()) os = "linux";

                FS.FileSystem.Log("CPU Architecture: " + RuntimeInformation.OSArchitecture + ", OS: " + os);

                return $"{release.Replace("/tag/", "/download/")}/snipesharp_{os}-{cpuArch}-v{version}" + (os == "win" ? ".exe" : "");
            }
            catch (Exception e) { FS.FileSystem.Log(e.ToString()); return "0"; }
        }
        public static async Task<string> GetLatestRelease() {
            try {
                // prepare client
                var handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                };
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", Cli.Templates.TWeb.UserAgent);
                
                // get latest version
                var latestPage = await client.GetAsync("http://github.com/snipesharp/snipesharp/releases/latest");
                var latestReleaseLink = latestPage.RequestMessage!.RequestUri!.AbsoluteUri;
                
                // return latest version
                return latestReleaseLink;
            }
            // return 0 if latest version fetch failed
            catch (Exception e) { FS.FileSystem.Log($"Failed to check for update: {e.Message}"); return "0"; }
        }
    }
}