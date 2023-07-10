namespace SKD.Test;
public class StationServiceTest : TestBase {

    public StationServiceTest() {
        context = GetAppDbContext();
    }

    [Fact]
    public async Task Can_save_new_station() {
        var service = new StationService(context);
        var input = new StationInput() {
            Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
            Name = Util.RandomString(EntityFieldLen.ProductionStation_Name),
            Sequence = 4
    };

        var before_count = await context.Components.CountAsync();
        var result = await service.SaveStation(input);

        var expectedCount = before_count + 1;
        var actualCount = context.ProductionStations.Count();
        Assert.Equal(expectedCount, actualCount);

        var station = await context.ProductionStations.FirstAsync(t => t.Id == result.Payload.Id);
        Assert.Equal(input.Code, station.Code);
        Assert.Equal(input.Name, station.Name);
        Assert.Equal(input.Sequence, station.Sequence);
    }

    [Fact]
    public async Task Can_update_station() {
        var service = new StationService(context);
        var input = new StationInput() {
            Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
            Name = Util.RandomString(EntityFieldLen.ProductionStation_Name),
            Sequence = 10
        };

        var before_count = await context.Components.CountAsync();
        var result = await service.SaveStation(input);

        var expectedCount = before_count + 1;
        var firstCount = context.ProductionStations.Count();
        Assert.Equal(expectedCount, firstCount);

        // update
        var input_2 = new StationInput() {
            Id = result.Payload.Id,
            Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
            Name = Util.RandomString(EntityFieldLen.ProductionStation_Name),
            Sequence = 20
        };


        var result_2 = await service.SaveStation(input_2);

        // assert
        var secondCount = context.ProductionStations.Count();
        Assert.Equal(firstCount, secondCount);
        Assert.Equal(input_2.Code, result_2.Payload.Code);
        Assert.Equal(input_2.Name, result_2.Payload.Name);
        Assert.Equal(input_2.Sequence, result_2.Payload.Sequence);
    }


    [Fact]
    public async Task Cannot_add_duplicate_station() {
        // setup
        var service = new StationService(context);
        var count_1 = context.ProductionStations.Count();
        var result = await service.SaveStation(new StationInput {
            Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
            Name = Util.RandomString(EntityFieldLen.ProductionStation_Name)
        });

        var count_2 = context.ProductionStations.Count();
        Assert.Equal(count_1 + 1, count_2);

        // insert duplicate code & name
        var result2 = await service.SaveStation(new StationInput {
            Code = result.Payload.Code,
            Name = result.Payload.Name
        });

        var count_3 = context.ProductionStations.Count();
        Assert.Equal(count_2, count_3);

        var errorCount = result2.Errors.Count;
        Assert.Equal(2, errorCount);

        var duplicateCode = result2.Errors.Any(e => e.Message == "duplicate code");
        Assert.True(duplicateCode, "expected: 'duplicateion code`");

        var duplicateName = result2.Errors.Any(e => e.Message == "duplicate name");
        Assert.True(duplicateCode, "expected: 'duplicateion name`");
    }
}

