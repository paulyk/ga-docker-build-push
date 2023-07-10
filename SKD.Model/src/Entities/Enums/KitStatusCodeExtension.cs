namespace SKD.Model;

public static class KitStatusCodeExtensions {


    public static KitTimelineCode ToPartnerStatusCode(this PartnerStatusCode code) {
        return (KitTimelineCode)(int)code;
    }

    public static PartnerStatusCode ToKitTimelineCode(this KitTimelineCode code) {
        return (PartnerStatusCode)(int)code;
    }

}
