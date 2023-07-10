using SKD.Model;
namespace SKD.Seed {
    internal class SeedDataGenerator {
        private readonly SkdContext ctx;

        public SeedDataGenerator(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task Seed_VehicleTimelineVentType() {

            // in order by when they should occur
            var eventTypes = new List<KitTimelineEventType> {
                new KitTimelineEventType {
                    Code = KitTimelineCode.CUSTOM_RECEIVED,
                },
                new KitTimelineEventType {
                    Code = KitTimelineCode.PLAN_BUILD,
                },
                new KitTimelineEventType {
                    Code = KitTimelineCode.BUILD_COMPLETED,
                },
                new KitTimelineEventType {
                    Code = KitTimelineCode.GATE_RELEASED,
                },
                new KitTimelineEventType {
                    Code = KitTimelineCode.WHOLE_SALE,
                },
            };

            var sequence = 1;
            eventTypes.ForEach(eventType => {
                eventType.Description = UnderscoreToPascalCase(eventType.Code.ToString());
                eventType.Sequence = sequence++;
            });

            ctx.KitTimelineEventTypes.AddRange(eventTypes);
            await ctx.SaveChangesAsync();
        }

        public async Task Seed_ProductionStations(ICollection<ProductionStation_Mock_DTO> data) {
            var stations = data.ToList().Select(x => new ProductionStation() {
                Code = x.Code,
                Name = x.Name,
                Sequence = x.SortOrder,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.ProductionStations.AddRange(stations);
            await ctx.SaveChangesAsync();
        }
        public async Task Seed_Components(ICollection<Component_MockData_DTO> componentData) {
            var components = componentData.ToList().Select(x => new Component() {
                Code = x.Code,
                Name = x.Name,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.Components.AddRange(components);
            await ctx.SaveChangesAsync();
        }

        private static string UnderscoreToPascalCase(string input) {
            var str = input.Split("_").Aggregate((x, y) => x + "  " + y);
            return str.Substring(0, 1).ToUpper() + str[1..].ToLower();
        }

    }
}