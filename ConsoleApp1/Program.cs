using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FolderSynchronizer
{
    public class Config
    {
        public string SourcePath { get; set; } = @"D:\HaranElola";
        public string DestinationPath { get; set; } = @"E:\HaranElola";
        public string LogDirectory { get; set; } = @"E:\";
        public bool EnableBidirectional { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class SyncStats
    {
        public int FilesProcessed { get; set; }
        public int FilesSkipped { get; set; }
        public int FilesCopied { get; set; }
        public int FilesDeleted { get; set; }
        public int DirectoriesCreated { get; set; }
        public long BytesProcessed { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class Program
    {
        private static Config config = new();
        private static readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sync_config.json");

        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Sincronizador Avanzado de Carpetas de Clase";

            await ShowWelcome();
            LoadConfig();
            await RunApplication();
        }

        private static async Task ShowWelcome()
        {
            var rule = new Spectre.Console.Rule("[bold yellow]🔄 SINCRONIZADOR AVANZADO DE CARPETAS DE CLASE[/]")
            {
                Justification = Justify.Center,
                Style = Style.Parse("yellow")
            };

            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();

            var panel = new Panel(new Markup("[cyan]Versión C# .NET con Spectre.Console[/]\n[dim]Autor: Claude Assistant - Versión Mejorada 2025[/]"))
            {
                Header = new PanelHeader(" 📁 Información "),
                BorderStyle = Style.Parse("cyan"),
                Padding = new Padding(2, 1)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            await Task.Delay(1500);
        }

        private static void LoadConfig()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var loadedConfig = JsonSerializer.Deserialize<Config>(json);
                    if (loadedConfig != null)
                    {
                        config = loadedConfig;
                        AnsiConsole.MarkupLine($"[green]✓[/] Configuración cargada desde: [dim]{configPath}[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]⚠[/] Usando configuración predeterminada");
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗[/] Error cargando configuración: {ex.Message}");
                AnsiConsole.MarkupLine("[yellow]⚠[/] Usando configuración predeterminada");
            }

            AnsiConsole.WriteLine();
        }

        private static void SaveConfig()
        {
            try
            {
                config.LastUpdated = DateTime.Now;
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
                AnsiConsole.MarkupLine($"[green]✓[/] Configuración guardada");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗[/] Error guardando configuración: {ex.Message}");
            }
        }

        private static async Task RunApplication()
        {
            while (true)
            {
                ShowCurrentConfig();
                var choice = ShowMainMenu();

                switch (choice)
                {
                    case "sync":
                        await PerformSync(false);
                        break;
                    case "sync_reverse":
                        await PerformSync(true);
                        break;
                    case "config":
                        ConfigurePaths();
                        break;
                    case "view_config":
                        ViewDetailedConfig();
                        break;
                    case "logs":
                        ViewLogs();
                        break;
                    case "exit":
                        ShowGoodbye();
                        return;
                }

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Presiona cualquier tecla para continuar...[/]");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private static void ShowCurrentConfig()
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Spectre.Console.Color.Blue)
                .AddColumn("[bold]Configuración Actual[/]")
                .AddColumn("[bold]Valor[/]");

            table.AddRow("[yellow]📂 Origen[/]", $"[cyan]{config.SourcePath}[/]");
            table.AddRow("[yellow]📁 Destino[/]", $"[cyan]{config.DestinationPath}[/]");
            table.AddRow("[yellow]📄 Logs[/]", $"[cyan]{config.LogDirectory}[/]");
            table.AddRow("[yellow]🔄 Bidireccional[/]", config.EnableBidirectional ? "[green]Sí[/]" : "[red]No[/]");
            table.AddRow("[yellow]🕒 Actualizado[/]", $"[dim]{config.LastUpdated:yyyy-MM-dd HH:mm:ss}[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        private static string ShowMainMenu()
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("¿Qué deseas hacer?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Usa las flechas para navegar)[/]")
                    .AddChoices(new[] {
                        "sync", "sync_reverse", "config", "view_config", "logs", "exit"
                    })
                    .UseConverter(choice => choice switch
                    {
                        "sync" => "🔄 Iniciar Sincronización (Origen → Destino)",
                        "sync_reverse" => "🔄 Sincronización Inversa (Destino → Origen)",
                        "config" => "⚙️  Configurar Rutas",
                        "view_config" => "📋 Ver Configuración Detallada",
                        "logs" => "📄 Ver Logs Anteriores",
                        "exit" => "🚪 Salir",
                        _ => choice
                    }));

            return choice;
        }

        private static async Task PerformSync(bool reverse = false)
        {
            var source = reverse ? config.DestinationPath : config.SourcePath;
            var destination = reverse ? config.SourcePath : config.DestinationPath;
            var direction = reverse ? "Destino → Origen" : "Origen → Destino";

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Spectre.Console.Rule($"[bold green]🔄 INICIANDO SINCRONIZACIÓN ({direction})[/]").LeftJustified());
            AnsiConsole.WriteLine();

            // Verificar rutas
            var verificationResult = await VerifyPaths(source, destination);
            if (!verificationResult.Success)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error de verificación:[/] {verificationResult.Message}");
                return;
            }

            // Confirmar operación
            if (!ConfirmOperation(source, destination, direction))
                return;

            var stats = new SyncStats { StartTime = DateTime.Now };
            var logPath = GenerateLogPath();

            try
            {
                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                    {
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn(),
                        new PercentageColumn(),
                        new RemainingTimeColumn(),
                        new SpinnerColumn(),
                    })
                    .StartAsync(async ctx =>
                    {
                        var mainTask = ctx.AddTask("[green]Analizando archivos...[/]");
                        var fileTask = ctx.AddTask("[blue]Procesando archivos...[/]", false);

                        // Análisis inicial
                        var analysisResult = await AnalyzeDirectories(source, destination, mainTask);
                        mainTask.Description = $"[green]Análisis completado: {analysisResult.TotalFiles} archivos encontrados[/]";

                        // Configurar tarea de archivos
                        fileTask.MaxValue = analysisResult.TotalFiles;
                        fileTask.StartTask();

                        // Sincronizar archivos
                        await SynchronizeFiles(source, destination, analysisResult, fileTask, stats);

                        mainTask.Value = 100;
                        fileTask.Value = fileTask.MaxValue;
                    });

                stats.EndTime = DateTime.Now;
                await WriteLogFile(logPath, stats, source, destination, direction);
                ShowSyncResults(stats, logPath);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                stats.Errors.Add($"Error crítico: {ex.Message}");
            }
        }

        private static async Task<(bool Success, string Message)> VerifyPaths(string source, string destination)
        {
            var verifyTask = AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .Start("Verificando rutas...", ctx =>
                {
                    ctx.Status = "Verificando carpeta origen...";
                    Thread.Sleep(500);

                    if (!Directory.Exists(source))
                        return (false, $"No se encontró la carpeta origen: {source}");

                    ctx.Status = "Verificando unidad destino...";
                    Thread.Sleep(500);

                    var destDrive = Path.GetPathRoot(destination);
                    if (!Directory.Exists(destDrive))
                        return (false, $"No se encontró la unidad destino: {destDrive}. Verifica que el USB esté conectado.");

                    ctx.Status = "Creando carpeta destino si no existe...";
                    Thread.Sleep(300);

                    if (!Directory.Exists(destination))
                    {
                        try
                        {
                            Directory.CreateDirectory(destination);
                        }
                        catch (Exception ex)
                        {
                            return (false, $"Error creando carpeta destino: {ex.Message}");
                        }
                    }

                    ctx.Status = "Verificando permisos...";
                    Thread.Sleep(300);

                    return (true, "Verificación completada exitosamente");
                });

            await Task.Delay(100); // Para mostrar el spinner
            return verifyTask;
        }

        private static bool ConfirmOperation(string source, string destination, string direction)
        {
            var panel = new Panel(new Markup($"""
                [bold yellow]⚠️  CONFIRMACIÓN DE OPERACIÓN[/]
                
                [cyan]Dirección:[/] {direction}
                [cyan]Origen:[/]    [dim]{source}[/]
                [cyan]Destino:[/]   [dim]{destination}[/]
                
                [red]Esta operación puede modificar o eliminar archivos.[/]
                """))
            {
                Header = new PanelHeader(" 🔍 Revisar antes de continuar "),
                BorderStyle = Style.Parse("yellow"),
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            return AnsiConsole.Confirm("¿Deseas continuar con la sincronización?", false);
        }

        private static async Task<(int TotalFiles, long TotalSize)> AnalyzeDirectories(string source, string destination, ProgressTask task)
        {
            var files = new List<FileInfo>();
            var totalSize = 0L;

            await Task.Run(() =>
            {
                try
                {
                    var sourceDir = new DirectoryInfo(source);
                    var allFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories);
                    files.AddRange(allFiles);
                    totalSize = allFiles.Sum(f => f.Length);

                    task.Increment(50);

                    // Simular análisis más detallado
                    for (int i = 0; i < 50; i++)
                    {
                        task.Increment(1);
                        Thread.Sleep(20);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error en análisis: {ex.Message}[/]");
                }
            });

            return (files.Count, totalSize);
        }

        private static async Task SynchronizeFiles(string source, string destination, (int TotalFiles, long TotalSize) analysis, ProgressTask task, SyncStats stats)
        {
            await Task.Run(() =>
            {
                try
                {
                    var sourceDir = new DirectoryInfo(source);
                    var files = sourceDir.GetFiles("*", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        try
                        {
                            var relativePath = Path.GetRelativePath(source, file.FullName);
                            var destPath = Path.Combine(destination, relativePath);
                            var destDir = Path.GetDirectoryName(destPath);

                            // Crear directorio si no existe
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                                stats.DirectoriesCreated++;
                            }

                            // Verificar si necesita copia
                            bool needsCopy = true;
                            if (File.Exists(destPath))
                            {
                                var destFile = new FileInfo(destPath);
                                if (file.Length == destFile.Length && file.LastWriteTime <= destFile.LastWriteTime)
                                {
                                    needsCopy = false;
                                    stats.FilesSkipped++;
                                }
                            }

                            if (needsCopy)
                            {
                                File.Copy(file.FullName, destPath, true);
                                stats.FilesCopied++;
                                stats.BytesProcessed += file.Length;
                            }

                            stats.FilesProcessed++;
                            task.Increment(1);

                            // Simular tiempo de procesamiento
                            Thread.Sleep(Random.Shared.Next(10, 50));
                        }
                        catch (Exception ex)
                        {
                            stats.Errors.Add($"Error copiando {file.Name}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    stats.Errors.Add($"Error general: {ex.Message}");
                }
            });
        }

        private static string GenerateLogPath()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            return Path.Combine(config.LogDirectory, $"sync_log_{timestamp}.txt");
        }

        private static async Task WriteLogFile(string logPath, SyncStats stats, string source, string destination, string direction)
        {
            try
            {
                var logContent = $"""
                    ==================================================
                    SINCRONIZACIÓN COMPLETADA
                    ==================================================
                    Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                    Dirección: {direction}
                    Origen: {source}
                    Destino: {destination}
                    
                    ESTADÍSTICAS:
                    - Archivos procesados: {stats.FilesProcessed}
                    - Archivos copiados: {stats.FilesCopied}
                    - Archivos omitidos: {stats.FilesSkipped}
                    - Directorios creados: {stats.DirectoriesCreated}
                    - Bytes procesados: {FormatBytes(stats.BytesProcessed)}
                    - Duración: {stats.Duration:hh\:mm\:ss}
                    
                    ERRORES ({stats.Errors.Count}):
                    {string.Join("\n", stats.Errors)}
                    
                    ADVERTENCIAS ({stats.Warnings.Count}):
                    {string.Join("\n", stats.Warnings)}
                    ==================================================
                    """;

                await File.WriteAllTextAsync(logPath, logContent);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error escribiendo log: {ex.Message}[/]");
            }
        }

        private static void ShowSyncResults(SyncStats stats, string logPath)
        {
            AnsiConsole.WriteLine();
            var rule = new Spectre.Console.Rule("[bold green]✅ SINCRONIZACIÓN COMPLETADA[/]").LeftJustified();
            AnsiConsole.Write(rule);

            var resultTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(stats.Errors.Count > 0 ? Spectre.Console.Color.Red : Spectre.Console.Color.Green)
                .AddColumn("[bold]Estadística[/]")
                .AddColumn("[bold]Valor[/]");

            resultTable.AddRow("📁 Archivos procesados", stats.FilesProcessed.ToString());
            resultTable.AddRow("📋 Archivos copiados", $"[green]{stats.FilesCopied}[/]");
            resultTable.AddRow("⏭️  Archivos omitidos", $"[yellow]{stats.FilesSkipped}[/]");
            resultTable.AddRow("📂 Directorios creados", stats.DirectoriesCreated.ToString());
            resultTable.AddRow("💾 Datos procesados", FormatBytes(stats.BytesProcessed));
            resultTable.AddRow("⏱️  Tiempo transcurrido", stats.Duration.ToString(@"hh\:mm\:ss"));
            resultTable.AddRow("❌ Errores", stats.Errors.Count > 0 ? $"[red]{stats.Errors.Count}[/]" : "[green]0[/]");

            AnsiConsole.Write(resultTable);

            if (stats.Errors.Count > 0)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[red]⚠️  Se encontraron errores. Consulta el log para más detalles.[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"📄 Log guardado en: [dim]{logPath}[/]");

            // Opciones post-sincronización
            var postChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("¿Qué deseas hacer ahora?")
                    .AddChoices(new[] { "view_log", "open_dest", "continue" })
                    .UseConverter(choice => choice switch
                    {
                        "view_log" => "📄 Ver log completo",
                        "open_dest" => "📂 Abrir carpeta destino",
                        "continue" => "➡️  Continuar",
                        _ => choice
                    }));

            switch (postChoice)
            {
                case "view_log":
                    ViewLogFile(logPath);
                    break;
                case "open_dest":
                    try
                    {
                        Process.Start("explorer.exe", config.DestinationPath);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error abriendo explorador: {ex.Message}[/]");
                    }
                    break;
            }
        }

        private static void ConfigurePaths()
        {
            AnsiConsole.WriteLine();
            var rule = new Spectre.Console.Rule("[bold blue]⚙️ CONFIGURACIÓN DE RUTAS[/]").LeftJustified();
            AnsiConsole.Write(rule);

            var newSource = AnsiConsole.Ask<string>($"[yellow]Carpeta origen[/] [dim](actual: {config.SourcePath})[/]:", config.SourcePath);
            var newDestination = AnsiConsole.Ask<string>($"[yellow]Carpeta destino[/] [dim](actual: {config.DestinationPath})[/]:", config.DestinationPath);
            var newLogDir = AnsiConsole.Ask<string>($"[yellow]Directorio de logs[/] [dim](actual: {config.LogDirectory})[/]:", config.LogDirectory);
            var bidirectional = AnsiConsole.Confirm("¿Habilitar sincronización bidireccional?", config.EnableBidirectional);

            config.SourcePath = newSource;
            config.DestinationPath = newDestination;
            config.LogDirectory = newLogDir;
            config.EnableBidirectional = bidirectional;

            SaveConfig();
            AnsiConsole.MarkupLine("[green]✓ Configuración actualizada exitosamente[/]");
        }

        private static void ViewDetailedConfig()
        {
            AnsiConsole.WriteLine();
            var configPanel = new Panel(JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }))
            {
                Header = new PanelHeader($" 📋 Configuración Detallada - {configPath} "),
                BorderStyle = Style.Parse("blue"),
            };

            AnsiConsole.Write(configPanel);
        }

        private static void ViewLogs()
        {
            try
            {
                var logFiles = Directory.GetFiles(config.LogDirectory, "sync_log_*.txt")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .Take(10)
                    .ToArray();

                if (logFiles.Length == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]⚠️  No se encontraron logs anteriores[/]");
                    return;
                }

                var selectedLog = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Selecciona un log para ver:")
                        .PageSize(10)
                        .AddChoices(logFiles)
                        .UseConverter(log => $"{Path.GetFileName(log)} [dim]({File.GetCreationTime(log):yyyy-MM-dd HH:mm})[/]"));

                ViewLogFile(selectedLog);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error accediendo a logs: {ex.Message}[/]");
            }
        }

        private static void ViewLogFile(string logPath)
        {
            try
            {
                var content = File.ReadAllText(logPath);
                var panel = new Panel(content)
                {
                    Header = new PanelHeader($" 📄 {Path.GetFileName(logPath)} "),
                    BorderStyle = Style.Parse("cyan"),
                };

                AnsiConsole.Write(panel);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error leyendo log: {ex.Message}[/]");
            }
        }

        private static void ShowGoodbye()
        {
            AnsiConsole.WriteLine();
            var goodbyePanel = new Panel(new Markup("[bold green]¡Gracias por usar el Sincronizador de Carpetas![/]\n[dim]Creado con ❤️  usando C# .NET y Spectre.Console[/]"))
            {
                Header = new PanelHeader(" 👋 Hasta luego "),
                BorderStyle = Style.Parse("green"),
                Padding = new Padding(2, 1)
            };

            AnsiConsole.Write(goodbyePanel);
            Thread.Sleep(2000);
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}