namespace SKD.Model;

public enum PartnerStatusCode {
    SHPC = 0,   // SHIP_CONFIRMED
    FPCR = 1,   // CUSTOM_RECEIVED
    FPBP,       // PLAN_BUILD
    FPBS,       // BUILD_START
    FPBC,       // BUILD_COMPLETED
    FPGR,       // GATE_RELEASE         
    FPWS        // WHOLE_SALE
}
