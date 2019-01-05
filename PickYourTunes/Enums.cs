namespace PickYourTunes
{
    /// <summary>
    /// Type of rhe added radio.
    /// </summary>
    public enum RadioType
    {
        /// <summary>
        /// An internal vanilla game radio.
        /// </summary>
        Vanilla = 0,
        /// <summary>
        /// A local large file stored on your PC.
        /// </summary>
        SingleFile = 1,
        /// <summary>
        /// An MP3 radio stream.
        /// </summary>
        Stream = 2,
        /// <summary>
        /// A complete radio with ads and more songs.
        /// </summary>
        Radio = 3
    }

    /// <summary>
    /// The current state of the stream.
    /// </summary>
    public enum StreamingState
    {
        /// <summary>
        /// The stream is completely stopped.
        /// </summary>
        Stopped = 0,
        /// <summary>
        /// The stream is playing.
        /// </summary>
        Playing = 1,
        /// <summary>
        /// The stream is paused and buffering.
        /// </summary>
        Buffering = 2
    }
}
