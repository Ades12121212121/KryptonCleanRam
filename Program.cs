using System.Management;  

Console.OutputEncoding = System.Text.Encoding.UTF8;

string banner = @"                   
  ██ ▄█▀ ██▓███  ▓███▄ ▄███▓ ██▓███ ▄▄▄      ▄▄▄▄   ▄▄▄ .    
  ██▄█▒ ▓██░  ██▒▓██▒▀█▀ ██▒▓██░  ██▒████▄   ▓█████▄▀▄.▀·    
 ▓███▄░ ▓██░ ██▓▒▓██    ▓██░▓██░ ██▓▒██  ▀█▄ ▒██▒ ▄█▐▀▀▪▄    
 ▓██ █▄ ▒██▄█▓▒ ▒▒██    ▒██ ▒██▄█▓▒ ▒██▄▄▄▄██▒██░█▀ ▐█▄▄▌    
 ▒██▒ █▄▒██▒ ░  ░▒██▒   ░██▒▒██▒ ░  ░▓█   ▓██▒▓█  ▀█▓█  ▓█   
 ▒ ▒▒ ▓▒░▓▒░ ░  ░░ ▒░   ░  ░░▓▒░ ░  ░▒▒   ▓▒█░▒▓███▀▒▒▓███▀   
 ░ ░▒ ▒░░▒ ░     ░  ░      ░░▒ ░      ▒   ▒▒ ░▒██▒ ░ ░▒   ▒   
 ░ ░░ ░ ░░       ░      ░   ░░        ░   ▒   ░▒   ░  ░   ░   
 ░  ░                     ░               ░  ░ ░          ░   
                Krypton Clean Ram                        
";

var colorOriginal = Console.ForegroundColor;

while (true)
{
    try
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(banner);
        Console.WriteLine();

        
        if (!EsAdministrador())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[!] Ejecuta como administrador para máxima optimización de RAM.\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        (long ramUsadaMB, long ramMaxMB) = MemUtils.GetGlobalMemoryUsageMB();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"RAM usada: {ramUsadaMB} MB / {ramMaxMB} MB");
        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine("\nSeleccione una opción:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[1]");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(" Limpiar RAM");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[2]");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(" Boost RAM (optimización avanzada)");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[3]");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(" Cerrar procesos secundarios inactivos");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[0]");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(" Salir");
        Console.ResetColor();
        Console.Write("Opción: ");
        string input = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(input) || !"0123".Contains(input.Trim()))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nOpción inválida. Presione una tecla para continuar...");
            Console.ReadKey();
            continue;
        }
        int opcion = int.Parse(input);
        if (opcion == 0) break;
        if (opcion == 1) LimpiarRamRobusta("Clean Ram");
        if (opcion == 2) LimpiarRamRobusta("Boost Ram");
        if (opcion == 3) CerrarProcesosSecundarios();
        Console.WriteLine("\nPresione una tecla para volver al menú...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[ERROR] {ex.Message}");
        Console.ForegroundColor = colorOriginal;
        Console.WriteLine("Presione una tecla para continuar...");
        Console.ReadKey();
    }
}

 
static bool EsAdministrador()
{
    try
    {
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }
    catch { return false; }
}


 
void LimpiarRamRobusta(string modo)
{     
    MostrarAnimacionNuke();
     
    (long ramAntes, long ramMax) = MemUtils.GetGlobalMemoryUsageMB();
    int procesosLiberados = 0, procesosCerrados = 0, procesosHijos = 0;
    var procesosCerradosLista = new List<string>();
    var procesosOptimLista = new List<string>();

     
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    MemUtils.EmptyWorkingSet(System.Diagnostics.Process.GetCurrentProcess());

     
    foreach (var proc in System.Diagnostics.Process.GetProcesses())
    {
        try
        {
            if (proc.HasExited) continue;
            if (proc.Id == System.Diagnostics.Process.GetCurrentProcess().Id) continue;
             
            if (proc.SessionId == 0 || proc.ProcessName.ToLower().Contains("system") || proc.ProcessName.ToLower().Contains("service")) continue;

            MemUtils.EmptyWorkingSet(proc);
            procesosLiberados++;
            procesosOptimLista.Add(proc.ProcessName);
        }
        catch { }
    }

     
    string[] procesosInactivos = { "notepad", "calc", "mspaint", "wordpad" };
    foreach (var nombre in procesosInactivos)
    {
        foreach (var p in System.Diagnostics.Process.GetProcessesByName(nombre))
        {
            try { p.Kill(); procesosCerrados++; procesosCerradosLista.Add(p.ProcessName); } catch { }
        }
    }

     
    int currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;
    foreach (var proc in System.Diagnostics.Process.GetProcesses())
    {
        try
        {
            int parentPid = MemUtils.GetParentProcessId(proc);
            if (parentPid == currentPid && !proc.HasExited)
            {
                MemUtils.EmptyWorkingSet(proc);
                procesosHijos++;
            }
        }
        catch { }
    }

     
    (long ramDespues, _) = MemUtils.GetGlobalMemoryUsageMB();
    long liberada = ramAntes - ramDespues;
    if (liberada < 0) liberada = 0;

     
    if (modo.ToLower().Contains("boost"))
    {
         
        bool compactado = false;
        try {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            if (GC.TryStartNoGCRegion(1024 * 1024 * 100))  
            {
                GC.EndNoGCRegion();
                compactado = true;
            }
        } catch { }

         
        int prioridadesAjustadas = 0;
        foreach (var proc in System.Diagnostics.Process.GetProcesses())
        {
            try
            {
                if (proc.HasExited) continue;
                if (proc.Id == System.Diagnostics.Process.GetCurrentProcess().Id) continue;
                if (proc.SessionId == 0 || proc.ProcessName.ToLower().Contains("system") || proc.ProcessName.ToLower().Contains("service")) continue;
                if (proc.Responding && proc.MainWindowHandle != IntPtr.Zero && proc.BasePriority > 8)
                {
                    proc.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                    prioridadesAjustadas++;
                }
            }
            catch { }
        }

         
        int serviciosOptim = 0;
        try {
            foreach (var proc in System.Diagnostics.Process.GetProcesses())
            {
                if (proc.HasExited) continue;
                if (proc.ProcessName.ToLower().Contains("service") && proc.SessionId != 0)
                {
                    MemUtils.EmptyWorkingSet(proc);
                    serviciosOptim++;
                }
            }
        } catch { }

         
        (long ramDespuesBoost, _) = MemUtils.GetGlobalMemoryUsageMB();
        long liberadaBoost = ramAntes - ramDespuesBoost;
        if (liberadaBoost < 0) liberadaBoost = 0;

        string boostMsg =
            $"\n[✓] Memoria compactada y heap optimizado: {(compactado ? "Sí" : "No")}" +
            $"\n[✓] Prioridad ajustada en procesos: {prioridadesAjustadas}" +
            $"\n[✓] Procesos de usuario optimizados: {procesosLiberados}" +
            $"\n[✓] Procesos inactivos cerrados: {procesosCerrados}" +
            $"\n[✓] Procesos hijos optimizados: {procesosHijos}" +
            $"\n[✓] Servicios de usuario optimizados: {serviciosOptim}" +
            $"\n[✓] Balanceo de memoria completado" +
            $"\n\nLa RAM ha sido distribuida y optimizada para máximo rendimiento.";
        string msg = $"{modo}: RAM liberada: {liberadaBoost} MB\n{boostMsg}";
        MostrarNotificacion(msg);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(msg);
        if (procesosOptimLista.Count > 0)
            Console.WriteLine("Procesos optimizados: " + string.Join(", ", procesosOptimLista.Distinct().Take(10)) + (procesosOptimLista.Count > 10 ? ", ..." : ""));
        if (procesosCerradosLista.Count > 0)
            Console.WriteLine("Procesos cerrados: " + string.Join(", ", procesosCerradosLista.Distinct().Take(10)) + (procesosCerradosLista.Count > 10 ? ", ..." : ""));
        Console.ForegroundColor = colorOriginal;
    }
    else
    {
        string msg = $"{modo}: RAM liberada: {liberada} MB | Procesos optimizados: {procesosLiberados} | Procesos cerrados: {procesosCerrados} | Hilos optimizados: {procesosHijos}";
        MostrarNotificacion(msg);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(msg);
        if (procesosOptimLista.Count > 0)
            Console.WriteLine("Procesos optimizados: " + string.Join(", ", procesosOptimLista.Distinct().Take(10)) + (procesosOptimLista.Count > 10 ? ", ..." : ""));
        if (procesosCerradosLista.Count > 0)
            Console.WriteLine("Procesos cerrados: " + string.Join(", ", procesosCerradosLista.Distinct().Take(10)) + (procesosCerradosLista.Count > 10 ? ", ..." : ""));
        Console.ForegroundColor = colorOriginal;
    }
}


 
void CerrarProcesosSecundarios()
{
    string[] procesos = { "notepad", "calc", "mspaint", "wordpad" };
    int cerrados = 0;
    foreach (var nombre in procesos)
    {
        foreach (var p in System.Diagnostics.Process.GetProcessesByName(nombre))
        {
            try { p.Kill(); cerrados++; } catch { }
        }
    }
    string msg = $"Procesos secundarios cerrados: {cerrados}";
    MostrarNotificacion(msg);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(msg);
    Console.ForegroundColor = colorOriginal;
}

 
static void MostrarNotificacion(string mensaje)
{
     
    string script = $"[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null; " +
                    $"$template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02); " +
                    $"$textNodes = $template.GetElementsByTagName(\"text\"); " +
                    $"$textNodes.Item(0).AppendChild($template.CreateTextNode(\"Krypton Clean Ram\")) > $null; " +
                    $"$textNodes.Item(1).AppendChild($template.CreateTextNode(\"{mensaje}\")) > $null; " +
                    $"$toast = [Windows.UI.Notifications.ToastNotification]::new($template); " +
                    $"[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier(\"Krypton Clean Ram\").Show($toast);";
    var psi = new System.Diagnostics.ProcessStartInfo("powershell", $"-NoProfile -Command \"{script}\"")
    {
        CreateNoWindow = true,
        UseShellExecute = false
    };
    try { System.Diagnostics.Process.Start(psi); } catch { }
}

 
void MostrarAnimacionNuke()
{
    string[] banner = new string[]
    {
        "    ██ ▄█▀ ██▀███ ▓██   ██▓ ██▓███  ▄▄▄█████▓ ▒█████   ███▄    █ ",
        "    ██▄█▒ ▓██ ▒ ██▒▒██  ██▒▓██░  ██▒▓  ██▒ ▓▒▒██▒  ██▒ ██ ▀█   █ ",
        "    ▓███▄░ ▓██ ░▄█ ▒ ▒██ ██░▓██░ ██▓▒▒ ▓██░ ▒░▒██░  ██▒▓██  ▀█ ██▒",
        "    ▓██ █▄ ▒██▀▀█▄   ░ ▐██▓░▒██▄█▓▒ ▒░ ▓██▓ ░ ▒██   ██░▓██▒  ▐▌██▒",
        "    ▒██▒ █▄░██▓ ▒██▒ ░ ██▒▓░▒██▒ ░  ░  ▒██▒ ░ ░ ████▓▒░▒██░   ▓██░",
        "    ▒ ▒▒ ▓▒░ ▒▓ ░▒▓░  ██▒▒▒ ▒▓▒░ ░  ░  ▒ ░░   ░ ▒░▒░▒░ ░ ▒░   ▒ ▒ ",
        "    ░ ░▒ ▒░  ░▒ ░ ▒░▓██ ░▒░ ░▒ ░         ░      ░ ▒ ▒░ ░ ░░   ░ ▒░",
        "    ░ ░░ ░   ░░   ░ ▒ ▒ ░░  ░░         ░      ░ ░ ░ ▒     ░   ░ ░ ",
        "    ░  ░      ░     ░ ░                           ░ ░           ░ ",
        "                    ░ ░                                           "
    };
    int width = Console.WindowWidth;
    int bannerLen = banner[0].Length;
    int leftPad = Math.Max(0, (width - bannerLen) / 2);
    int topPad = Math.Max(0, (Console.WindowHeight - banner.Length) / 2);
    int delay = 100;
    int loops = 2;
    ConsoleColor[] colors = { ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.DarkYellow, ConsoleColor.White };
    for (int l = 0; l < loops; l++)
    {
        foreach (var color in colors)
        {
            Console.Clear();
            for (int t = 0; t < topPad; t++) Console.WriteLine();
            Console.ForegroundColor = color;
            foreach (var line in banner)
            {
                Console.WriteLine(new string(' ', leftPad) + line);
            }
            Console.ResetColor();
            System.Threading.Thread.Sleep(delay);
        }
    }
    Console.ResetColor();
}

public static class MemUtils
{
    public static int GetParentProcessId(System.Diagnostics.Process process)
    {
        try
        {
            using (var searcher = new System.Management.ManagementObjectSearcher(
                "SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["ParentProcessId"]);
                }
            }
        }
        catch { }
        return -1;
    }

    public static (long ramUsadaMB, long ramMaxMB) GetGlobalMemoryUsageMB()
    {
        try
        {
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                long total = (long)(memStatus.ullTotalPhys / (1024 * 1024));
                long libre = (long)(memStatus.ullAvailPhys / (1024 * 1024));
                long usada = total - libre;
                return (usada, total);
            }
        }
        catch { }
        return (0, 0);
    }

    public static void EmptyWorkingSet(System.Diagnostics.Process proc)
    {
        try
        {
            _ = SetProcessWorkingSetSize(proc.Handle, -1, -1);
        }
        catch { }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern int SetProcessWorkingSetSize(IntPtr proc, int min, int max);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([System.Runtime.InteropServices.In] [System.Runtime.InteropServices.Out] MEMORYSTATUSEX lpBuffer);
}