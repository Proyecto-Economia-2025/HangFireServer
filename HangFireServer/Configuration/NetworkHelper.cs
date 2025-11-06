using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HangFireServer.Configuration
{
    public static class NetworkHelper
    {

     
        /// <summary>
        /// Lanza cloudflared y espera hasta timeoutMs buscando la URL pública.
        /// Deja el proceso corriendo si leaveRunning == true (recomendado).
        /// Devuelve (Url, LogPath, Pid) donde Url puede ser null si no se detectó.
        /// </summary>
        public static async Task<(string? Url, string LogPath, int? Pid)> GetCloudflareTunnelUrlAsync(
            int timeoutMs = 120000, // por defecto 120s
            string cloudflaredPath = @"..\..\cloudflared.exe",
            int localPort = 7192,
            bool leaveRunning = true,
            int statusIntervalMs = 5000) 
        {
            // Preparar log
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cloudflared_logs");
            Directory.CreateDirectory(logDir);
            string logPath = Path.Combine(logDir, $"cloudflared_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            string? url = null;

            // Path del config (debe estar en el mismo dir que cloudflared.exe)
            string configPath = @"..\..\quick-tunnel-config.yml";

            var psi = new ProcessStartInfo
            {
                FileName = cloudflaredPath,
                Arguments = $"tunnel --url http://127.0.0.1:{localPort} --config \"{configPath}\" --loglevel debug",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

            void WriteLog(string prefix, string line)
            {
                var timed = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {prefix} {line}";
                try { File.AppendAllText(logPath, timed + Environment.NewLine); } catch { }
                Console.WriteLine(timed);
            }

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                WriteLog("OUT:", e.Data);

                if (url == null)
                {
                    var m = Regex.Match(e.Data, @"https://[a-z0-9\-]+\.trycloudflare\.com", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        url = m.Value;
                    }
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                WriteLog("ERR:", e.Data);

                if (url == null)
                {
                    var m = Regex.Match(e.Data, @"https://[a-z0-9\-]+\.trycloudflare\.com", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        url = m.Value;
                    }
                }
            };


            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                WriteLog("INFO:", $"cloudflared started PID={process.Id}. Waiting up to {timeoutMs / 1000.0}s for URL...");

                var sw = Stopwatch.StartNew();
                int dots = 0;
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (url != null)
                        break;

                    if (process.HasExited)
                    {
                        WriteLog("WARN:", $"cloudflared exited early with code {process.ExitCode}");
                        break;
                    }

                    dots++;
                    Console.WriteLine($"[cloudflared] esperando a que Cloudflare caliente... ({dots * 5}s)");
                    await Task.Delay(5000);
                }
                sw.Stop();


                if (url != null)
                {
                    WriteLog("INFO:", $"URL detectada: {url} (took {sw.ElapsedMilliseconds} ms)");
                }
                else
                {
                    WriteLog("WARN:", $"No se detectó URL en {timeoutMs / 1000.0} s. Si cloudflared sigue arrancando, verifica logs: {logPath}");
                }

                int? pidAlive = process.HasExited ? null : process.Id;

                // si no queremos dejar corriendo, matarlo; por defecto leaveRunning=true => lo dejamos.
                if (!leaveRunning && !process.HasExited)
                {
                    try
                    {
                        process.Kill(true);
                        WriteLog("INFO:", "cloudflared terminado por leaveRunning=false");
                        pidAlive = null;
                    }
                    catch (Exception ex)
                    {
                        WriteLog("ERR:", $"Error al terminar cloudflared: {ex.Message}");
                    }
                }

                return (url, logPath, pidAlive);
            }
            catch (Exception ex)
            {
                WriteLog("ERR:", $"Excepción al ejecutar cloudflared: {ex}");
                try { if (!process.HasExited) process.Kill(true); } catch { }
                return (null, logPath, null);
            }
        }
    }
}

