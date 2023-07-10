
namespace SKD.Test;
public class ComponentStationServiceTest : TestBase {

    [Fact]
    public async Task Can_Set_ComponentStation_Mappings() {
        await Task.Delay(100);
        // setup
        context = GetAppDbContext();
        var stationCodes = new string[] { "ST1", "ST2" };
        var componentCodes = new string[] { "COMP1", "COMP2" };

        Gen_ProductionStations(stationCodes);
        Gen_Components(componentCodes);

        var mappings = new List<ComponentStationMapping>();
        foreach (var stationCode in stationCodes) {
            foreach (var componentCode in componentCodes) {
                mappings.Add(new ComponentStationMapping {
                    ComponentCode = componentCode,
                    StationCode = stationCode,
                    // randomize save CDC component boolean
                    SaveCDCComponent = new Random().Next(0, 2) == 0                    
                });
            }
        }

        var service = new ComponentStationService(context);
        var input = new ComponentStationMappingsInput {
            Mappings = mappings
        };
        var result = await service.SetComponentStationMappings(input);
        // assert not errors
        Assert.Empty(result.Errors);
        // get all ComponentStations
        var allComponentStations = await context.ComponentStations
            .Include(cs => cs.Component)
            .Include(cs => cs.Station)
            .ToListAsync();
        Assert.Equal(mappings.Count, allComponentStations.Count);
        // assert all mappings have the same Component.Code and Station.Code and SaveCDCComponent value
        foreach (var mapping in mappings) {
            var componentStation = allComponentStations.FirstOrDefault(cs =>
                cs.Component.Code == mapping.ComponentCode
                && cs.Station.Code == mapping.StationCode
            );
            Assert.NotNull(componentStation);
            Assert.Equal(mapping.SaveCDCComponent, componentStation.SaveCDCComponent);
        }
    }


}
