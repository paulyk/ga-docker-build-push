using System.Runtime.Serialization;

namespace SKD.KitStatusFeed;

public class KitStatusFeedException : Exception {
    public KitStatusFeedException() {
    }

    public KitStatusFeedException(string message)
        : base(message) {
    }

    public KitStatusFeedException(string message, Exception inner)
        : base(message, inner) {
    }

    // If you want to customize serialization, you can add this constructor.
    protected KitStatusFeedException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
    }
}
