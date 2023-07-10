namespace SKD.Test;
public class KitServiceTest : TestBase {

    public KitServiceTest() {
    }

    [Fact]
    public async Task Can_create_kit_timeline_events() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var baseDate = DateTime.Now.Date;
        // setup        
        var dealerCode = await context.Dealers.Select(t => t.Code).FirstOrDefaultAsync();
        var timelineEvents = new List<(KitTimelineCode eventType, DateTime trxDate, DateTime eventDate)>() {
                (KitTimelineCode.CUSTOM_RECEIVED, baseDate.AddDays(0), baseDate.AddDays(-6)),
                (KitTimelineCode.PLAN_BUILD,  baseDate.AddDays(0),baseDate.AddDays(2)),
                (KitTimelineCode.BUILD_START, baseDate.AddDays(1), baseDate.AddDays(2)),
                (KitTimelineCode.BUILD_COMPLETED,  baseDate.AddDays(5),baseDate.AddDays(5)),
                (KitTimelineCode.GATE_RELEASED, baseDate.AddDays(10), baseDate.AddDays(10)),
                (KitTimelineCode.WHOLE_SALE, baseDate.AddDays(12), baseDate.AddDays(12)),
            };

        // test
        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var results = new List<MutationResult<KitTimelineEvent>>();

        var before_count = context.KitTimelineEvents.Count();

        foreach (var entry in timelineEvents) {
            var service = new KitService(context, entry.trxDate);
            var dto = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventCode = entry.eventType,
                EventDate = entry.eventDate,
                DealerCode = dealerCode
            };

            if (entry.eventType == KitTimelineCode.BUILD_START) {
                await service.UpdateKitVinAsync(new KitVinInput {
                    KitNo = kit.KitNo,
                    VIN = Gen_VIN()
                });
                await Gen_ComponnentSerialScan_ForKit(kit.KitNo);
            }
            var result = await service.CreateKitTimelineEventAsync(dto);
            results.Add(result);
        }

        // assert
        var after_count = context.KitTimelineEvents.Count();
        Assert.Equal(0, before_count);
        Assert.Equal(timelineEvents.Count, after_count);
    }

    [Fact]
    public async Task Error_if_custom_receive_date_greater_than_current_date() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var appSettings = await ApplicationSetting.GetKnownAppSettings(context);
        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var currentDate = DateTime.Now.Date;
        var service = new KitService(context, currentDate);

        var input_1 = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = currentDate
        };

        var input_2 = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = currentDate.AddDays(-1)
        };

        // test
        var result_1 = await service.CreateKitTimelineEventAsync(input_1);
        var result_2 = await service.CreateKitTimelineEventAsync(input_2);

        // assert
        var expectedError = $"Custom received date must precede current date by {appSettings.PlanBuildLeadTimeDays} days";
        var actualMessage = result_1.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expectedError, actualMessage);
        Assert.Empty(result_2.Errors);
    }

    [Fact]
    public async Task Cannot_create_kit_timeline_events_out_of_sequence() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var baseDate = DateTime.Now.Date;
        var timelineEvents = new List<(KitTimelineCode eventType, DateTime trxDate, DateTime eventDate)>() {
                (KitTimelineCode.CUSTOM_RECEIVED, baseDate.AddDays(1),  baseDate.AddDays(6)),
                (KitTimelineCode.BUILD_COMPLETED, baseDate.AddDays(2), baseDate.AddDays(2)),
            };

        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        // test
        KitService service = null;
        var results = new List<MutationResult<KitTimelineEvent>>();

        foreach (var (eventType, eventDate, trxDate) in timelineEvents) {
            var input = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventCode = eventType,
                EventDate = eventDate,
            };
            service = new KitService(context, trxDate);
            var result = await service.CreateKitTimelineEventAsync(input);
            results.Add(result);
        }

        var lastPayload = results[1];

        // assert
        var expectedMessage = "Missing timeline event";
        var actualMessage = lastPayload.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expectedMessage, actualMessage);
    }

    [Fact]
    public async Task Create_kit_timeline_event_with_note() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var appSettings = await ApplicationSetting.GetKnownAppSettings(context);
        var kit = context.Kits.First();
        var dealerCode = context.Dealers.First().Code;
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var eventNote = Util.RandomString(15);
        var baseDate = DateTime.Now.Date;
        var timelineEventItems = new List<(KitTimelineCode eventType, DateTime trxDate, DateTime eventDate, string eventNode)>() {
                (KitTimelineCode.CUSTOM_RECEIVED, baseDate.AddDays(2), baseDate.AddDays(-appSettings.PlanBuildLeadTimeDays) , eventNote),
                (KitTimelineCode.PLAN_BUILD, baseDate.AddDays(3), baseDate.AddDays(5), eventNote),
                (KitTimelineCode.BUILD_START, baseDate.AddDays(4), baseDate.AddDays(5), eventNote),
                (KitTimelineCode.BUILD_COMPLETED, baseDate.AddDays(8), baseDate.AddDays(8), eventNote),
                (KitTimelineCode.GATE_RELEASED, baseDate.AddDays(10), baseDate.AddDays(10), eventNote),
                (KitTimelineCode.WHOLE_SALE, baseDate.AddDays(11), baseDate.AddDays(11), eventNote),
            };

        // test
        KitService service = null;

        var results = new List<MutationResult<KitTimelineEvent>>();

        foreach (var entry in timelineEventItems) {
            var input = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventCode = entry.eventType,
                EventDate = entry.eventDate,
                EventNote = entry.eventNode,
                DealerCode = dealerCode
            };
            service = new KitService(context, entry.trxDate);

            if (entry.eventType == KitTimelineCode.BUILD_START) {
                await service.UpdateKitVinAsync(new KitVinInput {
                    KitNo = kit.KitNo,
                    VIN = Gen_VIN()
                });
                await Gen_ComponnentSerialScan_ForKit(kit.KitNo);
            }

            var result = await service.CreateKitTimelineEventAsync(input);
            results.Add(result);
        }

        // assert
        var timelineEvents = context.KitTimelineEvents.ToList();

        Assert.Equal(timelineEventItems.Count, timelineEvents.Count);

        timelineEvents.ForEach(entry => {
            Assert.Equal(eventNote, entry.EventNote);
        });
    }

    [Fact]
    public async Task Cannot_set_timline_event_date_to_future_date_for_build_complete_onwards() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var appSettings = await ApplicationSetting.GetKnownAppSettings(context);
        var kit = context.Kits.First();
        var dealerCode = context.Dealers.First().Code;
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var eventNote = Util.RandomString(15);
        var baseDate = DateTime.Now.Date;
        var timelineEventItems = new List<(KitTimelineCode eventType, DateTime trxDate, DateTime eventDate, string expectedError)>() {
                (KitTimelineCode.CUSTOM_RECEIVED, baseDate.AddDays(2), baseDate.AddDays(-appSettings.PlanBuildLeadTimeDays) , ""),
                (KitTimelineCode.PLAN_BUILD, baseDate.AddDays(3), baseDate.AddDays(5), ""),
                (KitTimelineCode.BUILD_START, baseDate.AddDays(4), baseDate.AddDays(5), ""),
                (KitTimelineCode.BUILD_COMPLETED, baseDate.AddDays(8), baseDate.AddDays(9), "Date cannot be in the future"),
            };

        // test
        KitService service = null;

        var results = new List<MutationResult<KitTimelineEvent>>();

        foreach (var entry in timelineEventItems) {
            var input = new KitTimelineEventInput {
                KitNo = kit.KitNo,
                EventCode = entry.eventType,
                EventDate = entry.eventDate,
                EventNote = "",
                DealerCode = dealerCode
            };
            service = new KitService(context, entry.trxDate);

            if (entry.eventType == KitTimelineCode.BUILD_START) {
                await service.UpdateKitVinAsync(new KitVinInput {
                    KitNo = kit.KitNo,
                    VIN = Gen_VIN()
                });
                await Gen_ComponnentSerialScan_ForKit(kit.KitNo);
            }

            var result = await service.CreateKitTimelineEventAsync(input);
            if (entry.expectedError == "") {
                Assert.True(result.Errors.Count == 0);
            } else {
                Assert.True(result.Errors.Count > 0);
                var actualError = result.Errors.Select(t => t.Message).FirstOrDefault();
                Assert.StartsWith(entry.expectedError, actualError);
            }
            results.Add(result);
        }
    }

    [Fact]
    public async Task Create_kit_timline_event_removes_prior_events_of_the_same_type() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var before_count = context.KitTimelineEvents.Count();

        var originalDate = new DateTime(2020, 11, 28);
        var newDate = new DateTime(2020, 11, 30);

        var dto = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = originalDate
        };
        var dto2 = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = newDate
        };

        var service = new KitService(context, DateTime.Now);
        // test
        await service.CreateKitTimelineEventAsync(dto);
        await service.CreateKitTimelineEventAsync(dto2);

        var after_count = context.KitTimelineEvents.Count();

        // assert
        Assert.Equal(0, before_count);
        Assert.Equal(2, after_count);

        var originalEntry = context.KitTimelineEvents.FirstOrDefault(t => t.Kit.VIN == kit.VIN && t.RemovedAt != null);
        var latestEntry = context.KitTimelineEvents.FirstOrDefault(t => t.Kit.VIN == kit.VIN && t.RemovedAt == null);

        Assert.Equal(originalEntry.EventDate, originalDate);
        Assert.Equal(newDate, latestEntry.EventDate);
    }

    [Fact]
    public async Task Cannot_add_duplicate_kit_timline_event_if_same_type_and_date_and_note() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var kit = context.Kits.First();
        await Gen_ShipmentLot_ForKit(kit.KitNo);

        var originalDate = new DateTime(2020, 11, 28);
        var newDate = new DateTime(2020, 11, 30);
        var eventNote = "EN 78889";

        var dto = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = originalDate,
            EventNote = eventNote
        };
        var dto2 = new KitTimelineEventInput {
            KitNo = kit.KitNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = newDate,
            EventNote = eventNote
        };

        // test
        var service = new KitService(context, DateTime.Now);
        await service.CreateKitTimelineEventAsync(dto);
        await service.CreateKitTimelineEventAsync(dto2);
        var result = await service.CreateKitTimelineEventAsync(dto2);

        // assert
        var after_count = context.KitTimelineEvents.Count();
        Assert.Equal(2, after_count);
        var errorsMessage = result.Errors.Select(t => t.Message).First();
        var expectedMessage = "duplicate kit timeline event";

        Assert.StartsWith(expectedMessage, errorsMessage);
    }

    [Fact]
    public async Task Can_create_kit_timeline_event_by_lot() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var lot = context.Lots
            .Include(t => t.Kits)
            .First();
        await Gen_ShipmentLot(lot.LotNo);

        var kitCount = lot.Kits.Count;

        var baseDate = DateTime.Now.Date;
        var eventDate = baseDate.AddDays(-10);
        var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
        var input = new LotTimelineEventInput {
            LotNo = lot.LotNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = eventDate,
            EventNote = eventNote
        };

        // test
        var service = new KitService(context, baseDate);
        var result = await service.CreateLotTimelineEventAsync(input);
        Assert.Empty(result.Errors);

        var timelineEvents = context.KitTimelineEvents.Where(t => t.Kit.Lot.LotNo == input.LotNo)
            .Include(t => t.Kit)
            .Include(t => t.EventType).ToList();

        var timelineEventCount = timelineEvents.Count;
        Assert.Equal(kitCount, timelineEventCount);

        foreach (var timelineEvent in timelineEvents) {
            Assert.Equal(eventDate, timelineEvent.EventDate);
            Assert.Equal(eventNote, timelineEvent.EventNote);
        }
    }

    [Fact]
    public async Task Cannot_create_kit_timeline_event_by_lot_with_dupliate_date() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var lot = context.Lots.First();
        await Gen_ShipmentLot(lot.LotNo);

        var baseDate = DateTime.Now.Date;
        var event_date = baseDate.AddDays(1);
        var event_date_trx = baseDate.AddDays(2);
        var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
        var input = new LotTimelineEventInput {
            LotNo = lot.LotNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = event_date,
            EventNote = eventNote
        };

        // test
        var service = new KitService(context, event_date_trx);
        var result = await service.CreateLotTimelineEventAsync(input);
        Assert.Empty(result.Errors);

        var result_2 = await service.CreateLotTimelineEventAsync(input);
        Assert.NotEmpty(result_2.Errors);

        var errorMessage = result_2.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedMessage = "duplicate kit timeline event";
        var actualMessage = errorMessage.Substring(0, expectedMessage.Length);
        Assert.Equal(expectedMessage, actualMessage);
    }

    [Fact]
    public async Task Cannot_create_lot_custom_receive_with_date_6_months_ago() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var lot = context.Lots.First();
        await Gen_ShipmentLot(lot.LotNo);

        var baseDate = DateTime.Now.Date;
        var event_date = baseDate.AddMonths(-6).AddDays(-1);
        var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
        var input = new LotTimelineEventInput {
            LotNo = lot.LotNo,
            EventCode = KitTimelineCode.CUSTOM_RECEIVED,
            EventDate = event_date,
            EventNote = eventNote
        };

        // test
        var service = new KitService(context, baseDate);
        var result = await service.CreateLotTimelineEventAsync(input);

        var expectedError = "custom received cannot be more than 6 months ago";
        var actualErrorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.Equal(expectedError, actualErrorMessage);
    }

    [Fact]
    public async Task Can_change_kit_component_production_station() {
        // setup
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();

        var kit = await context.Kits
            .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
            .FirstOrDefaultAsync();

        var productionStationCodes = await context.ProductionStations.Select(t => t.Code).ToListAsync();
        var kitComponent = kit.KitComponents.FirstOrDefault();
        var newStationCode = productionStationCodes.First(code => code != kitComponent.ProductionStation.Code);

        var service = new KitService(context, DateTime.Now.Date);

        var paylaod = service.ChangeKitComponentProductionStationAsync(new KitComponentProductionStationInput {
            KitComponentId = kitComponent.Id,
            ProductionStationCode = newStationCode
        });

        var kitComponent_2 = await context.KitComponents
            .Include(t => t.ProductionStation)
            .FirstOrDefaultAsync(t => t.Id == kitComponent.Id);

        Assert.Equal(newStationCode, kitComponent_2.ProductionStation.Code);
    }



}

