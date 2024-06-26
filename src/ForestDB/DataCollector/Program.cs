using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var httpClient = new HttpClient();
var downloader = new HuntReportDownloader(httpClient);
var eventLogger = new EventLogger();
var worker = new DownloadWorker(downloader, 500, eventLogger);

var tokenSource = new CancellationTokenSource();
worker.Run(tokenSource.Token);

public class HuntReportDownloader
{
    private readonly HttpClient client;
    private readonly string huntReportUrl = "https://www.bdl.lasy.gov.pl/portal/BULiGL.BDL.Reports/Map/HuntReportData?objectId=";
    private readonly int emptyResultLength = 18;

    public HuntReportDownloader(HttpClient client) => this.client = client;

    public HuntReportDownloaderResult? DownloadId(int id)
    {
        var currentUrl = this.huntReportUrl + id;
        var response = this.client!.GetAsync(currentUrl).Result;
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = response.Content.ReadAsByteArrayAsync().Result;
        return result.Length == this.emptyResultLength ? null : new(id, result);
    }
}

public class DownloadWorker
{
    private readonly string savepointPath = "savepoint.txt";
    private readonly string resultsPath = "/Results/";
    private readonly HuntReportDownloader downloader;
    private readonly int waitMiliseconds;
    private readonly EventLogger eventLogger;
    private int currentId;

    public DownloadWorker(HuntReportDownloader downloader, int waitMiliseconds, EventLogger eventLogger)
    {
        if (File.Exists(this.savepointPath))
        {
            if (int.TryParse(File.ReadAllText(this.savepointPath), out var result))
            {
                this.currentId = result;
            }
        }
        else
        {
            File.Create(this.savepointPath).Dispose();
        }

        this.downloader = downloader;
        this.waitMiliseconds = waitMiliseconds;
        this.eventLogger = eventLogger;
    }

    public void Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var result = this.downloader.DownloadId(this.currentId);
            if (result != null)
            {
                var savePath = Path.Combine(this.resultsPath, this.currentId.ToString() + ".json");
                using var writer = new BinaryWriter(File.Create(savePath));
                var bytes = result.Data.AsSpan();
                writer.Write(bytes);

                this.eventLogger.HuntReportDownloaded(result.Id, bytes.Length);
            }

            this.currentId++;
            File.WriteAllText(this.savepointPath, this.currentId.ToString());
            Thread.Sleep(this.waitMiliseconds);
        }
    }
}

public record HuntReportDownloaderResult(int Id, byte[] Data);

public class EventLogger
{
    public void HuntReportDownloaded(int id, int dataLength)
    {
        Log.Information("EventName: {EventName} Id: {Id} DataLength: {DataLength}", nameof(HuntReportDownloaded), id, dataLength);
    }
}