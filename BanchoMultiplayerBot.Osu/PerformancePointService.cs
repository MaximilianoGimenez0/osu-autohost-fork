using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Osu.Data;
using BanchoMultiplayerBot.Osu.Extensions;
using osu.NET.Models.Scores;
using Serilog;

namespace BanchoMultiplayerBot.Osu;

public class PerformancePointService(string? performancePointServiceUrl, string? beatmapCacheDirectory)
{
    public bool IsAvailable => !string.IsNullOrWhiteSpace(performancePointServiceUrl);

    private readonly HttpClient _httpClient = new();

    private readonly string _cacheDirectoryPath = beatmapCacheDirectory ?? "cache";

    /// <summary>
    /// Calculates the pp values for 100%, 98% and 95% with NM for the specified beatmap.
    /// </summary>
    public async Task<BeatmapPerformanceInfo?> CalculatePerformancePoints(int beatmapId, int mods = 0, DateTimeOffset? lastUpdatedHint = null)
    {
        if (!await DownloadBeatmapFile(beatmapId, lastUpdatedHint))
        {
            return null;
        }

        try
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"{performancePointServiceUrl}/pp/map")
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    map_id = beatmapId,
                    mods
                }), 
                Encoding.UTF8,
                "application/json")
            };
            
            var response = await _httpClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error(BotMessages.Logs.Osu.PpMapRequestFailed, beatmapId, (int)response.StatusCode);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<BeatmapPerformanceInfo>();
            if (data == null)
            {
                Log.Error(BotMessages.Logs.Osu.PpMapParseFailed, beatmapId);
                return null;
            }
            
            return data;
        }
        catch (Exception e)
        {
            Log.Warning(BotMessages.Logs.Osu.PpMapCalculationError, beatmapId, e.Message);
            return null;
        }
    }

    /// <summary>
    /// Calculates the pp for an individual score.
    /// </summary>
    public async Task<ScorePerformanceInfo?> CalculateScorePerformancePoints(int beatmapId, Score scoreModel, DateTimeOffset? lastUpdated = null)
    {
        if (!await DownloadBeatmapFile(beatmapId, lastUpdated))
        {
            return null;
        }

        try
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"{performancePointServiceUrl}/pp/score")
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        map_id = beatmapId,
                        n300 = scoreModel.Statistics.Great,
                        n100 = scoreModel.Statistics.Ok,
                        n50 = scoreModel.Statistics.Meh,
                        miss = scoreModel.Statistics.Miss,
                        max_combo = scoreModel.MaxCombo,
                        mods = scoreModel.GetModsBitset()
                    }), 
                    Encoding.UTF8,
                    "application/json")
            };
            
            var response = await _httpClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error(BotMessages.Logs.Osu.PpScoreRequestFailed, scoreModel.Id, beatmapId, (int)response.StatusCode);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<ScorePerformanceInfo>();
            if (data == null)
            {
                Log.Error(BotMessages.Logs.Osu.PpScoreParseFailed, scoreModel.Id);
                return null;
            }
            
            return data;
        }
        catch (Exception e)
        {
            Log.Warning(BotMessages.Logs.Osu.PpScoreCalculationError, scoreModel.Id, beatmapId, e.Message);
            return null;
        }
    }

    private async Task<bool> DownloadBeatmapFile(int beatmapId, DateTimeOffset? lastUpdatedHint = null)
    {
        var beatmapFilePath = $"{_cacheDirectoryPath}/{beatmapId}.osu";

        // This should never be the case but better safe than sorry.
        if (!IsAvailable)
        {
            Log.Error(BotMessages.Logs.Osu.PpServiceNotAvailable);
            return false;
        }
        
        if (!Directory.Exists(_cacheDirectoryPath))
        {
            Directory.CreateDirectory(_cacheDirectoryPath);
        }

        if (File.Exists(beatmapFilePath))
        {
            var fileWrittenTime = File.GetLastWriteTimeUtc(beatmapFilePath);
            
            if (lastUpdatedHint != null &&
                lastUpdatedHint.Value.UtcDateTime > fileWrittenTime)
            {
                Log.Warning(
                    BotMessages.Logs.Osu.BeatmapCacheOutdated,
                    beatmapId, 
                    lastUpdatedHint.Value.UtcDateTime,
                    fileWrittenTime);
                
                File.Delete(beatmapFilePath);
            }
            else
            {
                return true;
            }
        }
        
        // Download the beatmap, this will only download the beatmap itself (.osu), without any additional media.
        
        try
        {
            var downloadTask = _httpClient.GetStreamAsync(new Uri($"https://osu.ppy.sh/osu/{beatmapId}"));

            await using var s = await downloadTask.WaitAsync(TimeSpan.FromSeconds(5));
            await using var fs = new FileStream(beatmapFilePath, FileMode.CreateNew);

            await s.CopyToAsync(fs);
        }
        catch (Exception e)
        {
            Log.Error(BotMessages.Logs.Osu.BeatmapDownloadFailed, beatmapId, e.Message);
            return false;
        }

        return true;
    }
}
