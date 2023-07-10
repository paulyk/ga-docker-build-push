using System.Text;


namespace SKD.Server;

public class Mutation {


    /// <summary>
    /// Create or update a component
    /// </summary>
    public async Task<SKD.Service.MutationResult<UpdateComponentPayload>> SaveComponent(
            [Service] ComponentService service1,
            ComponentInput input
        ) {
        return await service1.SaveComponent(input);
    }

    /// <summary>
    /// Set or remove component default production station
    /// </summary>
    public async Task<SKD.Service.MutationResult<UpdateComponentPayload>> SetComponentDefaultStation(
            [Service] ComponentService service,
            SetDefaultStationInput input
        ) {
        return await service.SetDefaultStation(input);
    }

    /// <summary>
    /// Create or update a production station
    /// </summary>
    public async Task<SKD.Service.MutationResult<UpdateStationPayload>> SaveStation(
            [Service] StationService service,
            StationInput input
    ) => await service.SaveStation(input);

    public async Task<SKD.Service.MutationResult<ComponentSerialDTO>> CaptureComponentSerial(
          [Service] ComponentSerialService service,
          ComponentSerialInput input
    ) => await service.SaveComponentSerial(input);


    public async Task<SKD.Service.MutationResult<DcwsResponse>> CreateDcwsResponse(
      [Service] DCWSResponseService service,
      DcwsComponentResponseInput input
    ) {
        var dto = new DcwsComponentResponseInput {
            KitComponentId = input.KitComponentId,
            ResponseCode = input.ResponseCode,
            ErrorMessage = input.ErrorMessage
        };
        return await service.SaveDcwsComponentResponse(dto);
    }

    public async Task<SKD.Service.MutationResult<ShipmentOverviewDTO>> ImportShipment(
        [Service] ShipmentService service,
        ShipFile input
    ) => await service.ImportShipment(input);

    public async Task<SKD.Service.MutationResult<BomOverviewDTO>> ImportBom(
        [Service] BomService service,
        BomFile input
   ) => await service.ImportBom(input);

    public async Task<SKD.Service.MutationResult<PlantOverviewDTO>> CreatePlant(
        [Service] PlantService service,
        PlantInput input
     ) => await service.CreatePlant(input);

    public async Task<SKD.Service.MutationResult<LotPartDTO>> CreateLotPartQuantityReceived(
        [Service] LotPartService service,
        ReceiveLotPartInput input
    ) => await service.CreateLotPartQuantityReceived(input);

    public async Task<SKD.Service.MutationResult<DcwsResponse>> VerifyComponentSerial(
        [Service] VerifySerialService verifySerialService,
        Guid kitComponentId
    ) => await verifySerialService.VerifyComponentSerial(kitComponentId);

    public async Task<SKD.Service.MutationResult<ReceiveHandlingUnitPayload>> SetHandlingUnitReceived(
        [Service] HandlingUnitService service,
        ReceiveHandlingUnitInput input
    ) => await service.SetHandlingUnitReceived(input);

    public record ApplyComponentSerialFormatInput(Guid Id);
    public async Task<ComponentSerial> ApplyComponentSerialFormat(
        [Service] ComponentSerialService service,
        ApplyComponentSerialFormatInput input
    ) => await service.ApplyComponentSerialFormat(input.Id);

    public async Task<SKD.Service.MutationResult<Lot>> SetLotNote(
        [Service] BomService service,
        LotNoteInput input
    ) => await service.SetLotNote(input);

    public async Task<SKD.Service.MutationResult<SavePcvPayload>> SavePCV(
        [Service] PcvService service,
        SavePcvInput input
    ) => await service.SavePCV(input);

    public async Task<SKD.Service.MutationResult<DcwsResponse>> SaveDcwsComponentResponse(
        [Service] DCWSResponseService service,
        DcwsComponentResponseInput input
    ) => await service.SaveDcwsComponentResponse(input);

    public async Task<SKD.Service.MutationResult<SetComponentStationMappingsPayload>> SetComponentStationMappings(
        [Service] ComponentStationService service,
        ComponentStationMappingsInput input
    ) => await service.SetComponentStationMappings(input);

    public async Task<SKD.Service.MutationResult<RemoveAllComponentStationMappingsPayload>> RemoveAllComponentStationMappings(
        [Service] ComponentStationService service
    ) => await service.RemoveAllComponentStationMappings();



    /// <summary>
    /// Parses a PCV xlsx file and returns a list of PCV, and Component code records
    /// </summary>
    /// <param name="service"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<ParsePcvsXlxsResult>> ParsePcvsXlsx(
        [Service] PcvXlsxParserService service,
        IFile file
    ) {
        var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        return await service.ParsePcvsXlsx(stream);
    }

    /// <summary>
    /// Parse a shipment file and return a ShipFile structure
    /// </summary>
    /// <param name="service"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public async Task<ShipFile> ParseShipmentFile(
        [Service] ShipFileParser service,
        IFile file
    ) {
        var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var text = Encoding.UTF8.GetString(stream.ToArray());
        return service.ParseShipmentFile(text);
    }

    /// <summary>
    /// Set the VIN for a Kit.  
    /// This will mark the existing VIN as removed and add a new VIN.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<Kit>> SetKitVinAsync(
       [Service] KitService service,
        KitVinInput input
    ) => await service.UpdateKitVinAsync(input);

}



