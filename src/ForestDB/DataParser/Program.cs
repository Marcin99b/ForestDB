using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

//var items = GetAllItems().ToArray();

//var serialized = JsonConvert.SerializeObject(new Rootobject() { huntingData = items });

var fullPath = "C:/Results/full.data";
//using var bsonFile = File.Create(fullPath);
//var bsonWriter = new BsonDataWriter(bsonFile);

//var textReader = new StringReader(serialized);
//var jsonReader = new JsonTextReader(textReader);
//bsonWriter.WriteToken(jsonReader);

var fs = File.Open(fullPath, FileMode.Open);
var reader = new BsonDataReader(fs);

var serializer = new JsonSerializer();
var items = serializer.Deserialize<Rootobject>(reader)!.huntingData;

var groupedByRegion = items.GroupBy(x => x.hunt_dist_nr).ToArray();


Console.ReadKey();

IEnumerable<Huntingdata> GetAllItems()
{
    var files = Directory.GetFiles("C:/Results").Where(x => x.EndsWith(".json")).ToArray();

    var count = 0;
    var fullCount = files.Length;
    foreach (var item in files)
    {
        var content = File.ReadAllText(item);
        var huntingData = JsonConvert.DeserializeObject<Rootobject>(content)!.huntingData;
        Console.SetCursorPosition(0, 0);
        Console.Write($"{++count}/{fullCount} ({count * 100 / fullCount}%)");
        foreach (var huntingItem in huntingData)
        {
            yield return huntingItem;
        }
    }
}

public class Rootobject
{
    public Huntingdata[] huntingData { get; set; }
}

public class Huntingdata
{
    public string years1 { get; set; }
    public string years2 { get; set; }
    public string hunt_dist_nr { get; set; }
    public string total_area { get; set; }
    public string forest_area { get; set; }
    public string after_exclusion_area { get; set; }
    public string county_name { get; set; }
    public string district_name { get; set; }
    public string inspectorate_name { get; set; }
    public string inspectorate_adr { get; set; }
    public string region_name { get; set; }
    public string region_adr { get; set; }
    public string reg_mng_board_name { get; set; }
    public string reg_mng_board_adr { get; set; }
    public string lease_holder_name { get; set; }
    public string farming_cost { get; set; }
    public string damage_cost { get; set; }
    public string venison_value { get; set; }
    public string objectid { get; set; }
    public string hd_int_num { get; set; }
    public string a_year { get; set; }
    public string los_poz { get; set; }
    public string los_stan { get; set; }
    public string los_plan { get; set; }
    public string jel_poz { get; set; }
    public string jel_stan { get; set; }
    public string jel_plan { get; set; }
    public string jsi_poz { get; set; }
    public string jsi_stan { get; set; }
    public string jsi_plan { get; set; }
    public string dan_poz { get; set; }
    public string dan_stan { get; set; }
    public string dan_plan { get; set; }
    public string sar_poz { get; set; }
    public string sar_stan { get; set; }
    public string sar_plan { get; set; }
    public string muf_poz { get; set; }
    public string muf_stan { get; set; }
    public string muf_plan { get; set; }
    public string dzi_poz { get; set; }
    public string dzi_stan { get; set; }
    public string dzi_plan { get; set; }
    public string lis_poz { get; set; }
    public string lis_stan { get; set; }
    public string lis_plan { get; set; }
    public string jen_poz { get; set; }
    public string jen_stan { get; set; }
    public string jen_plan { get; set; }
    public string bor_poz { get; set; }
    public string bor_stan { get; set; }
    public string bor_plan { get; set; }
    public string sza_poz { get; set; }
    public string sza_stan { get; set; }
    public string sza_plan { get; set; }
    public string kun_poz { get; set; }
    public string kun_stan { get; set; }
    public string kun_plan { get; set; }
    public string nor_poz { get; set; }
    public string nor_stan { get; set; }
    public string nor_plan { get; set; }
    public string tch_poz { get; set; }
    public string tch_stan { get; set; }
    public string tch_plan { get; set; }
    public string szo_poz { get; set; }
    public string szo_stan { get; set; }
    public string szo_plan { get; set; }
    public string piz_poz { get; set; }
    public string piz_stan { get; set; }
    public string piz_plan { get; set; }
    public string zaj_poz { get; set; }
    public string zaj_stan { get; set; }
    public string zaj_plan { get; set; }
    public string dkr_poz { get; set; }
    public string dkr_stan { get; set; }
    public string dkr_plan { get; set; }
    public string jar_poz { get; set; }
    public string jar_stan { get; set; }
    public string jar_plan { get; set; }
    public string baz_poz { get; set; }
    public string baz_stan { get; set; }
    public string baz_plan { get; set; }
    public string kur_poz { get; set; }
    public string kur_stan { get; set; }
    public string kur_plan { get; set; }
    public string ges_poz { get; set; }
    public string ges_stan { get; set; }
    public string ges_plan { get; set; }
    public string kac_poz { get; set; }
    public string kac_stan { get; set; }
    public string kac_plan { get; set; }
    public string gol_poz { get; set; }
    public string gol_stan { get; set; }
    public string gol_plan { get; set; }
    public string slo_poz { get; set; }
    public string slo_stan { get; set; }
    public string slo_plan { get; set; }
    public string lys_poz { get; set; }
    public string lys_stan { get; set; }
    public string lys_plan { get; set; }
}