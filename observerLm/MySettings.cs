using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using observerLm.controls.dialogs;

namespace observerLm;

public class MySettings 
{
    /// <summary>
    /// Путь к API локального модуля. Например: http://localhost:8080/api/v1/
    /// </summary>
    public string Url { get; set; }=null! ;

    /// <summary>
    /// Строка в формате Base64 для авторизации Basic по умолчанию стоит admin:admin
    /// </summary>
    public string Auth { get; set; }=null!;
    /// <summary>
    /// Токен для авторизации по API
    /// </summary>
    public string Token { get; set; }=null! ;

    /// <summary>
    /// Путь к папке логов
    /// </summary>
    public string FolderLog { get; set; } = "/var/log/regime";
    /// <summary>
    /// Количество строк в логах, которые будут отображаться при открытии приложения, по умолчанию 100 0 не допускается.
    /// </summary>
    public int Tail { get; set; } = 100;

    private static async Task<MySettings?> GetSettings()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "observerLm","settings.json");
            
            if (File.Exists(path))
            {
                var str = File.ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<MySettings>(str);
                return settings;
            }
            throw new FileNotFoundException("Критическая ошибка: файл настроек не найден!", path);

        }
        catch (Exception e)
        {
            await MessageDialog.Show("Ошибка", e.Message);
            
           return null;
        }

    }
    private static MySettings? _instance;

    public static MySettings Settings
    {
        get
        {
            _instance ??= GetSettings().ConfigureAwait(false).GetAwaiter().GetResult();
            return _instance ??throw new NullReferenceException();
        }
    }

    public static async void Save()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "observerLm","settings.json");
            await File.WriteAllTextAsync(path,JsonConvert.SerializeObject(_instance, Formatting.Indented));
        }
        catch (Exception e)
        {
           await MessageDialog.Show("Error save setting",e.Message);
        }
    }
}