using System.Data;
using NuGet.Frameworks;
using SKD.Model;

namespace SKD.Test;

public class PcvServiceTest : TestBase {

    public PcvServiceTest() {
    }

    [Fact]
    public async Task Can_create_PCV() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var input = new SavePcvInput {
            PcvCode = Gen_Pcv_Code(),
            ModelYear = DateTime.UtcNow.Year,

            PcvModel = Gen_CategoryInput(),
            PcvSubmodel = Gen_CategoryInput(),
            PcvSeries = Gen_CategoryInput(),
            PcvEngine = Gen_CategoryInput(),
            PcvTransmission = Gen_CategoryInput(),
            PcvDrive = Gen_CategoryInput(),
            PcvPaint = Gen_CategoryInput(),
            PcvTrim = Gen_CategoryInput(),

            ComponentCodes = context.Components.Take(5).Select(t => t.Code).ToList()
        };
        var service = new PcvService(context);

        // test
        var result = await service.SavePCV(input);

        var pcvModel = await context.PcvModels.FirstOrDefaultAsync();

        // assert
        Assert.Empty(result.Errors);

        // assert result payload
        Assert.NotNull(result.Payload.PcvModel.Code);
        Assert.NotNull(result.Payload.PcvSubmodel.Code);
        Assert.NotNull(result.Payload.PcvSeries.Code);
        Assert.NotNull(result.Payload.PcvEngine.Code);
        Assert.NotNull(result.Payload.PcvTransmission.Code);
        Assert.NotNull(result.Payload.PcvDrive.Code);
        Assert.NotNull(result.Payload.PcvPaint.Code);
        Assert.NotNull(result.Payload.PcvTrim.Code);

        // assert pcv properties
        var pcv = await context.Pcvs
            .Include(t => t.PcvModel)
            .Include(t => t.PcvSubmodel)
            .Include(t => t.PcvSeries)
            .Include(t => t.PcvEngine)
            .Include(t => t.PcvTransmission)
            .Include(t => t.PcvDrive)
            .Include(t => t.PcvPaint)
            .Include(t => t.PcvTrim)
            .Include(t => t.PcvComponents).ThenInclude(t => t.Component)
            .FirstAsync(t => t.Id == result.Payload.Id);

        Assert.Equal(input.PcvCode, pcv.Code);
        Assert.Equal(input.ModelYear.ToString(), pcv.ModelYear);

        AssertCateory(input.PcvModel, pcv.PcvModel.Code);
        AssertCateory(input.PcvSubmodel, pcv.PcvSubmodel.Code);
        AssertCateory(input.PcvSeries, pcv.PcvSeries.Code);
        AssertCateory(input.PcvEngine, pcv.PcvEngine.Code);
        AssertCateory(input.PcvTransmission, pcv.PcvTransmission.Code);
        AssertCateory(input.PcvDrive, pcv.PcvDrive.Code);
        AssertCateory(input.PcvPaint, pcv.PcvPaint.Code);
        AssertCateory(input.PcvTrim, pcv.PcvTrim.Code);

        // description
        Assert.NotEmpty(pcv.Description);

        // components
        Assert.Equal(input.ComponentCodes.Count, pcv.PcvComponents.Count);

        var mappings = await context.ComponentStations.ToListAsync();

        foreach (var pcvComp in pcv.PcvComponents) {
            var mapping = mappings.FirstOrDefault(t => t.ComponentId == pcvComp.ComponentId);
            Assert.NotNull(mapping);
        }
    }

    [Fact]
    public async Task Can_Delete_PCV() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var input = new SavePcvInput {
            PcvCode = Gen_Pcv_Code(),
            ModelYear = DateTime.UtcNow.Year,

            PcvModel = Gen_CategoryInput(),
            PcvSubmodel = Gen_CategoryInput(),
            PcvSeries = Gen_CategoryInput(),
            PcvEngine = Gen_CategoryInput(),
            PcvTransmission = Gen_CategoryInput(),
            PcvDrive = Gen_CategoryInput(),
            PcvPaint = Gen_CategoryInput(),
            PcvTrim = Gen_CategoryInput(),

            ComponentCodes = context.Components.Take(5).Select(t => t.Code).ToList()
        };
        var service = new PcvService(context);
        var result = await service.SavePCV(input);
        var pcv = await context.Pcvs.FirstOrDefaultAsync(t => t.Code == input.PcvCode);

        var before_PCV_count = await context.Pcvs.CountAsync();
        var before_PcvComponent_count = await context.PcvComponents.CountAsync();
        var before_pcvModel_count = await context.PcvModels.CountAsync();
        var before_PcvSubmodel_count = await context.PcvSubmodels.CountAsync();

        // test
        var delete_result = await service.DeletePCV(new DeletePcvInput { PcvCode = pcv.Code });
        Assert.Empty(delete_result.Errors);
        pcv = await context.Pcvs.FirstOrDefaultAsync(t => t.Code == input.PcvCode);
        Assert.Null(pcv);

        var after_PCV_count = await context.Pcvs.CountAsync();
        Assert.Equal(before_PCV_count - 1, after_PCV_count);

        var after_PcvComponent_count = await context.PcvComponents.CountAsync();
        Assert.Equal(before_PcvComponent_count - input.ComponentCodes.Count, after_PcvComponent_count);

        var after_PcvModel_count = await context.PcvModels.CountAsync();
        Assert.Equal(before_pcvModel_count - 1, after_PcvModel_count);

        var after_PcvSubmodel_Count = await context.PcvSubmodels.CountAsync();
        Assert.Equal(before_PcvSubmodel_count - 1, after_PcvSubmodel_Count);
    }

    string Gen_CategoryInput() => Gen_Code(EntityFieldLen.Category_Code);

    void AssertCateory(string first, string second) {
        Assert.Equal(first, second);
        Assert.Equal(first,  second);
    }

    // skip fact
    [Fact(Skip = "Not implemented")]
    public async void Error_if_component_not_mapped() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var componentCode = "ZZZ";
        Gen_Components(componentCode);

        var input = new SavePcvInput {
            PcvCode = Gen_Pcv_Code(),
            ModelYear = DateTime.UtcNow.Year,

            PcvModel = Gen_CategoryInput(),
            PcvSubmodel = Gen_CategoryInput(),
            PcvSeries = Gen_CategoryInput(),
            PcvEngine = Gen_CategoryInput(),
            PcvTransmission = Gen_CategoryInput(),
            PcvDrive = Gen_CategoryInput(),
            PcvPaint = Gen_CategoryInput(),
            PcvTrim = Gen_CategoryInput(),

            ComponentCodes = context.Components.Take(5).Select(t => t.Code).ToList().Concat(new List<string> { componentCode }).ToList()
        };
        var service = new PcvService(context);
        var result = await service.SavePCV(input);

        Assert.NotEmpty(result.Errors);
    }

}



