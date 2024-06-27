using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var httpClient = new HttpClient();
var downloader = new HuntReportDownloader(httpClient);
var eventLogger = new EventLogger();
var worker = new DownloadWorker(downloader, 1000, eventLogger, 100, 4);

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
    private readonly int batchSize;
    private readonly int workers;
    private int currentId;

    public DownloadWorker(HuntReportDownloader downloader, int waitMiliseconds, EventLogger eventLogger, int batchSize, int workers)
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
        this.batchSize = batchSize;
        this.workers = workers;
    }

    public void Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var currentBatch = new List<HuntReportDownloaderResult>();
            var lastId = this.batchSize + this.currentId;
            var parallel = Parallel.For(this.currentId, lastId, new ParallelOptions() { CancellationToken = token, MaxDegreeOfParallelism = this.workers }, i =>
            {
                var result = this.downloader.DownloadId(i);
                if (result != null)
                {
                    currentBatch.Add(result);
                    this.eventLogger.HuntReportDownloaded(result.Id, result.Data.Length);
                }
            });

            this.eventLogger.BatchSavingStarted();
            foreach (var item in currentBatch)
            {
                var savePath = Path.Combine(this.resultsPath, item.Id.ToString() + ".json");
                using var writer = new BinaryWriter(File.Create(savePath));
                var bytes = item.Data.AsSpan();
                writer.Write(bytes);
            }
            this.eventLogger.BatchSavingFinished();

            File.WriteAllText(this.savepointPath, lastId.ToString());

            this.eventLogger.BatchSaved(this.currentId, lastId, currentBatch.Count());
            this.currentId = lastId;

            Thread.Sleep(this.waitMiliseconds);
        }
    }
}

public record HuntReportDownloaderResult(int Id, byte[] Data);

public class EventLogger
{
    public void HuntReportDownloaded(int id, int dataLength) => Log.Information("EventName: {EventName} Id: {Id} DataLength: {DataLength}", nameof(HuntReportDownloaded), id, dataLength);

    public void BatchSaved(int fromId, int toId, int items) => Log.Information("EventName: {EventName} FromId: {FromId} ToId: {ToId} Items: {Items}", nameof(BatchSaved), fromId, toId, items);

    public void BatchSavingStarted() => Log.Information("EventName: {EventName}", nameof(BatchSavingStarted));

    public void BatchSavingFinished() => Log.Information("EventName: {EventName}", nameof(BatchSavingFinished));
}