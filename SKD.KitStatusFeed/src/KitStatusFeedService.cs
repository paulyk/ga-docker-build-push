namespace SKD.KitStatusFeed;

public class KitStatusFeedService {

    public static string ClientName = nameof(KitStatusFeedService);
    private readonly HttpClient _client;

    private readonly ILogger<KitStatusFeedService> _logger;

    public KitStatusFeedService(
        IHttpClientFactory clientFactory,
        ILogger<KitStatusFeedService> logger
    ) {
        _logger = logger;
        _client = clientFactory.CreateClient("KitStatusFeedService");
        Console.WriteLine(_client.BaseAddress);
    }

    public async Task<KitStatusFeedResponse<KitCurrentStatusResponse>> GetCurrentStatusAsync(
        KitCurrentStatusRequest input
    ) {
        var result = new KitStatusFeedResponse<KitCurrentStatusResponse>();

        try {

            var response = await _client
                .PostAsJsonAsync<KitCurrentStatusRequest>(KitStatusFeedApiEndpoints.GetCurrentStatus, input);
            result.IsSuccess = response.IsSuccessStatusCode;

            if (response.IsSuccessStatusCode) {
                result.Data = await response.Content.ReadFromJsonAsync<KitCurrentStatusResponse>();
            } else {
                var payload = await response.Content.ReadFromJsonAsync<KitStatusFeedErrorResponse>();
            }
        } catch (Exception ex) {
            throw new KitStatusFeedException("Error during operation KitStatusFeeg: get-current-status", ex);
        }

        return result;
    }

    public async Task<string> GetCurrentStatusCodeAsync(
        string kitNo
    ) {

        var response = await _client.PostAsJsonAsync<KitCurrentStatusRequest>(KitStatusFeedApiEndpoints.GetCurrentStatus, new KitCurrentStatusRequest {
            KitNumber = kitNo
        });

        if (!response.IsSuccessStatusCode) {
            throw new KitStatusFeedException("Error during operation KitStatusFeeg: get-current-status");
        }

        var data = await response.Content.ReadFromJsonAsync<KitCurrentStatusResponse>();

        return data?.Status ?? "";
    }

    public async Task<KitStatusFeedResponse<KitPVinResponse>> GetPvinAsync(
        KitPVinRequest input
    ) {
        var result = new KitStatusFeedResponse<KitPVinResponse>();

        try {

            var response = await _client.PostAsJsonAsync<KitPVinRequest>(KitStatusFeedApiEndpoints.GetPhysicalVin, input);
            result.IsSuccess = response.IsSuccessStatusCode;

            if (response.IsSuccessStatusCode) {

                result.Data = await response.Content.ReadFromJsonAsync<KitPVinResponse>();

            } else {

                var errorPayload = await response.Content.ReadFromJsonAsync<KitStatusFeedErrorResponse>();
                throw new KitStatusFeedException("Error during operation KitStatusFeed: get-pvin", new Exception(errorPayload?.Error.Messages.FirstOrDefault()));
            }

        } catch (Exception ex) {
            throw new KitStatusFeedException("Error during operation KitStatusFeed: get-pvin", ex);
        }

        return result;
    }

    public async Task<KitStatusFeedResponse<KitProcessPartnerStatusResponse>> ProcessPartnerStatusAsync(
        KitProcessPartnerStatusRequest input
    ) {
        var result = new KitStatusFeedResponse<KitProcessPartnerStatusResponse>();

        try {

            var response = await _client.PostAsJsonAsync<KitProcessPartnerStatusRequest>(KitStatusFeedApiEndpoints.ProcessPartnerStatus, input);

            result.IsSuccess = response.IsSuccessStatusCode;

            if (result.IsSuccess) {

                result.Data = await response.Content.ReadFromJsonAsync<KitProcessPartnerStatusResponse>();

            } else {

                var data = await response.Content.ReadFromJsonAsync<KitStatusFeedErrorResponse>();

            }

        } catch (Exception ex) {
            throw new KitStatusFeedException("Error during operation KitStatusFeed: process-partner-status", ex);
        }

        return result;
    }
}