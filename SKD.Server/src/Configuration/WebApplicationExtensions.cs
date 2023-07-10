using SKD.KitStatusFeed;

public static class WebApplicationExtensions {
    public static WebApplication MapRouting(this WebApplication app) {
        app.MapPost("/migrate", async (SkdContext context) => {
            await context.Database.MigrateAsync();
            return Results.Ok();
        });

        app.MapPost("ksf/kit-current-status", async (KitCurrentStatusRequest input, KitStatusFeedService apiService) => {
            var result = await apiService.GetCurrentStatusAsync(input);
            return result;
        });

        app.MapPost("ksf/kit-pvin", async (KitPVinRequest input, KitStatusFeedService apiService) => {
            var result = await apiService.GetPvinAsync(input);
            return result;
        });

        app.MapPost("ksf/kit-process-partner-status", async (KitProcessPartnerStatusRequest input, KitStatusFeedService apiService) => {
            var result = await apiService.ProcessPartnerStatusAsync(input);
            return result;
        });

        return app;
    }
}