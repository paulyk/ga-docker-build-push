namespace SKD.Test;
public class DcwsSerialFormatter_Test {

    record TestData(string Serial1, string Serial2, string ExpectedSerial1, string ExpectedSerial2, bool ExpectedSuccess, string ExpectedMessage);

    [Fact]
    public void Dcws_serial_formatter_transformats_TR_serial_correctly() {

        // setup
        var tests = new List<TestData> {

                new TestData(
                    Serial1:        "A4321 03092018787960  FB3P 7000 DA  A1 ",
                    Serial2:        "",
                    ExpectedSerial1: "A4321 03092018787960  FB3P 7000 DA  A1 ",
                    ExpectedSerial2: "",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(

                    Serial1:        "TC04A20275132234JB3P 7000 VE",
                    Serial2:        "",
                    ExpectedSerial1: "TC04A20275132234      JB3P 7000 VE     ",
                    ExpectedSerial2: "",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial1:        "TC04A21034221034L1MP 7000 SB ",
                    Serial2:        "",
                    ExpectedSerial1:  "TC04A21034221034      L1MP 7000 SB     ",
                    ExpectedSerial2: "",
                    ExpectedSuccess:true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial1:        "FFTB102020224524",
                    Serial2:        "JB3R 7003 SA",
                    ExpectedSerial1:  "FFTB102020224524JB3R 7003 SA",
                    ExpectedSerial2: "",
                    ExpectedSuccess:true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial1:         "JB3R 7003 SA",
                    Serial2:         "FFTB102020224524",
                    ExpectedSerial1: "FFTB102020224524JB3R 7003 SA",
                    ExpectedSerial2: "",
                    ExpectedSuccess:true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial1:        "TC04A2027JB3P 7000 VE",
                    Serial2:        "",
                    ExpectedSerial1: "",
                    ExpectedSerial2: "",
                    ExpectedSuccess: false,
                    ExpectedMessage: TR_SerialFormatter.NO_MATCHING_TR_VARIANT
                ),
                new TestData(
                    Serial1:        "JB3B-2660005-YJ3ZHE",
                    Serial2:        "S0FAF202792998L",
                    ExpectedSerial1: "",
                    ExpectedSerial2: "",
                    ExpectedSuccess: false,
                      ExpectedMessage: TR_SerialFormatter.NO_MATCHING_TR_VARIANT
                ),
            };

        // test
        foreach (var testEntry in tests) {

            var result = DcwsSerialFormatter.FormatSerialIfNeeded("TR", new Serials(testEntry.Serial1, testEntry.Serial2));

            // assert
            Assert.Equal(testEntry.ExpectedSuccess, result.Success);
            Assert.Equal(testEntry.ExpectedMessage, result.Message);

            if (testEntry.ExpectedSuccess) {
                Assert.Equal(testEntry.ExpectedSerial1, result.Serials.Serial1);
                Assert.Equal(testEntry.ExpectedSerial2, result.Serials.Serial2);
            }
        }
    }

    [Fact]
    public void Dcws_serial_formatter_transformats_EN_serial_correctly() {
        var tests = new List<TestData> {

                new TestData(
                    Serial1:        "CSEPA20276110074JB3Q 6007 KB    36304435474544003552423745444400364145374A4643003636474148454200",
                    Serial2:        "",
                    ExpectedSerial1: "CSEPA20276110074JB3Q 6007 KB",
                    ExpectedSerial2: "",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial1:         "CSEPA20276110067JB3Q 6007 KB",
                    Serial2:        "",
                    ExpectedSerial1: "CSEPA20276110067JB3Q 6007 KB",
                    ExpectedSerial2: "",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial1:         "GRBPA20318943774FB3Q 6007 CB3D",
                    Serial2:        "",
                    ExpectedSerial1: "GRBPA20318943774FB3Q 6007 CB3D",
                    ExpectedSerial2: "",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial1:         "394893834",
                    Serial2:        "",
                    ExpectedSerial1: "",
                    ExpectedSerial2: "",
                    ExpectedSuccess: false,
                    ExpectedMessage: EN_SerialFormatter.INVALID_SERIAL
                )
            };

        // test
        foreach (var testEntry in tests) {

            var result = DcwsSerialFormatter.FormatSerialIfNeeded("EN", new Serials(testEntry.Serial1, testEntry.Serial2));

            // assert
            Assert.Equal(testEntry.ExpectedSuccess, result.Success);
            Assert.Equal(testEntry.ExpectedMessage, result.Message);

            if (testEntry.ExpectedSuccess) {
                Assert.Equal(testEntry.ExpectedSerial1, result.Serials.Serial1);
                Assert.Equal(testEntry.ExpectedSerial2, result.Serials.Serial2);
            }
        }
    }
}
