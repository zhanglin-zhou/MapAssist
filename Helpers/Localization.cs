using MapAssist.Structs;
using MapAssist.Types;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MapAssist.Helpers
{
    public static class Localization
    {
        public static void LoadItemLocalization()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resName = "MapAssist.Resources.items-localization.json";
            using (Stream stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new StreamReader(stream))
                {
                    var jsonString = reader.ReadToEnd();
                    Items._localizedItemList = JsonConvert.DeserializeObject<LocalizedItemList>(jsonString);
                }

                foreach (var item in Items._localizedItemList.Items)
                {
                    Items.LocalizedItems.Add(item.Key, item);
                }
            }
        }

        public static void LoadAreaLocalization()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resName = "MapAssist.Resources.items-localization.json";
            using (Stream stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new StreamReader(stream))
                {
                    var jsonString = reader.ReadToEnd();
                    AreaExtensions._localizedAreaList = JsonConvert.DeserializeObject<LocalizedAreaList>(jsonString);
                }

                foreach (var item in AreaExtensions._localizedAreaList.Areas)
                {
                    AreaExtensions.LocalizedAreas.Add(item.Key, item);
                }
            }
        }

        public static void LoadShrineLocalization()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resName = "MapAssist.Resources.items-localization.json";
            using (Stream stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new StreamReader(stream))
                {
                    var jsonString = reader.ReadToEnd();
                    Shrine._localizedShrineList = JsonConvert.DeserializeObject<LocalizedShrineList>(jsonString);
                }

                foreach (var item in Shrine._localizedShrineList.Shrines)
                {
                    Shrine.LocalizedShrines.Add(item.Key, item);
                }
            }
        }
    }

    public class LocalizedItemList
    {
        public List<LocalizedObj> Items = new List<LocalizedObj>();
    }

    public class LocalizedAreaList
    {
        public List<LocalizedObj> Areas = new List<LocalizedObj>();
    }

    public class LocalizedShrineList
    {
        public List<LocalizedObj> Shrines = new List<LocalizedObj>();
    }

    public class LocalizedObj
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string enUS { get; set; }
        public string zhTW { get; set; }
        public string deDE { get; set; }
        public string esES { get; set; }
        public string frFR { get; set; }
        public string itIT { get; set; }
        public string koKR { get; set; }
        public string plPL { get; set; }
        public string esMX { get; set; }
        public string jaJP { get; set; }
        public string ptBR { get; set; }
        public string ruRU { get; set; }
        public string zhCN { get; set; }
    }
}
