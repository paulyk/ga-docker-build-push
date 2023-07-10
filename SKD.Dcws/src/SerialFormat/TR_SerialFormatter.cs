namespace SKD.Dcws;

public record MatchVariantResult(TR_Variant Variant, Serials Serials);
public enum TR_Variant_Type {
    V_6R80,
    V_10R80,
    V_MT
}

public class TR_Variant {
    public TR_Variant_Type VariantType { get; set; }
    public string Match_Serial1_Regex { get; set; } = "";
    public string Match_Serial_2_Regex { get; set; } = "";
    public string Tokenize_Regex { get; set; } = "";
    public string Math_Output_Regex { get; set; } = "";
    public List<int> TokenSpacing { get; set; } = new List<int>();
}

public class TR_SerialFormatter {

    readonly SerialUtil serialUtil = new();

    public static readonly string NO_MATCHING_TR_VARIANT = "No matching TR variant";

    public List<TR_Variant> TR_Variants = new() {
            new TR_Variant {
                VariantType = TR_Variant_Type.V_6R80,
                Match_Serial1_Regex = @"^(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+$",
                Match_Serial_2_Regex = @"\s*",
                Math_Output_Regex = @"^\w+\s{1}\w+\s{2}\w+\s{1}\w+\s{1}\w+\s{2}\w+\s$",
                Tokenize_Regex = @"^(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+$",
                TokenSpacing = new List<int> { 1, 2, 1, 1, 2, 1 }
            },
            new TR_Variant {
                VariantType = TR_Variant_Type.V_10R80,
                Match_Serial1_Regex = @"^(\w{16})(\w{4})\s+(\w{4})\s+(\w{2})\s*$",
                Match_Serial_2_Regex = @"\s*",
                Math_Output_Regex = @"^\w{16}\s{6}\w{4}\s\w{4}\s\w{2}\s{5}",
                Tokenize_Regex = @"^(\w{16})(\w{4})\s+(\w{4})\s+(\w{2})\s*$",

                TokenSpacing = new List<int> { 6, 1, 1, 5 }
            },
            new TR_Variant {
                VariantType = TR_Variant_Type.V_MT,
                Match_Serial1_Regex = @"FFTB\w{12}",
                Match_Serial_2_Regex = @"\w{4}\s7003\s\w{2}",
                Math_Output_Regex = @"^FFTB\d{12}\w{4}\s7003\s\w{2}$",
                Tokenize_Regex = @"(\w+)\s(\w+)\s(\w+)",
                TokenSpacing = new List<int> { 1, 1 }
            }
        };

    public SerialFormatResult FormatSerial(Serials inputSerials) {
        var (Variant, Serials) = Get_TR_Variant(inputSerials);
        if (Variant == null) {
            return new SerialFormatResult(inputSerials, false, NO_MATCHING_TR_VARIANT);
        }

        var serial = Serials.Serial1 + Serials.Serial2;
        var formattedSerial = serialUtil.SpacifyString(serial, Variant.Tokenize_Regex, Variant.TokenSpacing);

        var matchesOutputFormat = serialUtil.MatchesPattern(formattedSerial, Variant.Math_Output_Regex);

        if (!matchesOutputFormat) {
            throw new Exception("Did not match output format");
        }

        return new SerialFormatResult(new Serials(formattedSerial, ""), true, "");
    }

    /// <summary>
    /// Tries to find matching TR variant by testing combinations of Serial1 and Serial2
    ///</summary>
    ///<returns>THe variant and serials in the accepted order</returns>
    public MatchVariantResult Get_TR_Variant(Serials serials) {

        // Serial / Part numbers can be scanned in any order
        // Test both to find the correct variant
        var serials_combinations = new List<Serials> {
                new Serials(serials.Serial1, serials.Serial2),
                new Serials(serials.Serial2, serials.Serial1),
            };


        foreach (var trVariant in TR_Variants) {
            foreach (var serialsCombo in serials_combinations) {
                var serial_1_match = serialUtil.MatchesPattern(serialsCombo.Serial1, trVariant.Match_Serial1_Regex);
                var serial_2_match = serialUtil.MatchesPattern(serialsCombo.Serial2, trVariant.Match_Serial_2_Regex);

                if (serial_1_match && serial_2_match) {
                    return new MatchVariantResult(trVariant, serialsCombo);
                }
            }
        }
        return new MatchVariantResult(null, serials);
    }
}


