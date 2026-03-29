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

    public static async Task<MySettings?> GetSettings()
    {
        try
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "observerLm","settings.json");
            
            if (File.Exists(path))
            {
                string str = File.ReadAllText(path);
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
}