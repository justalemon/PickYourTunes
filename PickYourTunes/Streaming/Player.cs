using NAudio.Wave;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Timers;

namespace PickYourTunes.Streaming
{
    /// <summary>
    /// Class used for handling the stream connections.
    /// </summary>
    public class StreamPlayer
    {
        /// <summary>
        /// Timer that handles the opperations over time.
        /// </summary>
        private System.Timers.Timer HandlingTimer = new System.Timers.Timer();
        /// <summary>
        /// Player of buffered media.
        /// </summary>
        private BufferedWaveProvider WaveProvider;
        /// <summary>
        /// Output Wave Player.
        /// </summary>
        private IWavePlayer Player;
        /// <summary>
        /// The current state of the playback.
        /// </summary>
        private volatile StreamingState State;
        /// <summary>
        /// If the last stream piece has been downloaded.
        /// </summary>
        private volatile bool FullyDownloaded;
        /// <summary>
        /// The class that handles the HTTP calls.
        /// </summary>
        private HttpWebRequest Request;
        /// <summary>
        /// See the main class to learn what it does.
        /// </summary>
        private VolumeWaveProvider16 VolumeProvider;

        /// <summary>
        /// Checks if the buffer is nearly full.
        /// </summary>
        private bool IsBufferNearlyFull => WaveProvider != null && WaveProvider.BufferLength - WaveProvider.BufferedBytes < WaveProvider.WaveFormat.AverageBytesPerSecond / 4;

        public StreamPlayer()
        {
            // Run every 250 MS
            HandlingTimer.Interval = 250;
            // And add the event
            HandlingTimer.Elapsed += new ElapsedEventHandler(OnTick);
        }

        private void StreamMp3(object state)
        {
            // Store that we don't have the stream downloaded
            FullyDownloaded = false;
            // Store the URL
            string URL = (string)state;
            // Create the web request
            Request = (HttpWebRequest)WebRequest.Create(URL);
            // Create a variable for the response
            HttpWebResponse Response;
            // Try to make the web request
            try
            {
                Response = (HttpWebResponse)Request.GetResponse();
            }
            // If there is an exception, just return
            catch (WebException)
            {
                return;
            }

            // Create a place to store the buffer data (needs to be big enough to hold a decompressed frame)
            byte[] Buffer = new byte[16384 * 4];

            // Remove the Decompressor
            IMp3FrameDecompressor Decompressor = null;
            // Start processing the data
            try
            {
                // Get the response stream
                using (Stream ResponseStream = Response.GetResponseStream())
                {
                    // Store as a ready stream
                    FullStream ReadyStream = new FullStream(ResponseStream);
                    // And start working with it
                    do
                    {
                        // If the buffer is nearly full
                        if (IsBufferNearlyFull)
                        {
                            // Sleep for half a second
                            Thread.Sleep(500);
                        }
                        else
                        {
                            // Set a place to store the frame
                            Mp3Frame Frame;
                            // Try to create the frame from the stream that is ready
                            try
                            {
                                Frame = Mp3Frame.LoadFromStream(ReadyStream);
                            }
                            // If we reach the end of the stream, break the do
                            catch (EndOfStreamException)
                            {
                                FullyDownloaded = true;
                                break;
                            }
                            // We tried to read the file prior to downloading it
                            catch (IOException)
                            {
                                break;
                            }
                            // If we get a web exception, break the do
                            // Original Message: Probably we have aborted download from the GUI thread
                            catch (WebException)
                            {
                                break;
                            }
                            // If there is no frame, break the do
                            if (Frame == null)
                            {
                                break;
                            }
                            // If there is no decompressor:
                            if (Decompressor == null)
                            {
                                // Don't think these details matter too much - just help ACM select the right codec
                                // however, the buffered provider doesn't know what sample rate it is working at
                                // until we have a frame
                                Decompressor = CreateFrameDecompressor(Frame);
                                // Create a new wave provider
                                WaveProvider = new BufferedWaveProvider(Decompressor.OutputFormat)
                                {
                                    BufferDuration = TimeSpan.FromSeconds(20) // Allow us to get well ahead of ourselves
                                };
                            }
                            // Decompress a single frame
                            int Decompressed = Decompressor.DecompressFrame(Frame, Buffer, 0);
                            // And add it to the buffer (TODO: Confirm if my explanations are correct)
                            WaveProvider.AddSamples(Buffer, 0, Decompressed);
                        }
                    } while (State != StreamingState.Stopped);
                    // If the decompressor is not empty, dispose it
                    if (Decompressor != null)
                    {
                        Decompressor.Dispose();
                    }
                }
            }
            finally
            {
                // If the decompressor is not empty, dispose it
                if (Decompressor != null)
                {
                    Decompressor.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns an MP3 Frame decompressor from a single frame.
        /// </summary>
        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame Frame)
        {
            // Create the wave format
            WaveFormat Format = new Mp3WaveFormat(Frame.SampleRate, Frame.ChannelMode == ChannelMode.Mono ? 1 : 2, Frame.FrameLength, Frame.BitRate);
            // And return it as an MP3 frame decompressor
            return new AcmMp3FrameDecompressor(Format);
        }

        /// <summary>
        /// Stops the playback of the stream
        /// </summary>
        public void Stop()
        {
            // If the stream is not already stopped
            if (State != StreamingState.Stopped)
            {
                // If the stream is not fully downloaded and there is a request being made, abort it
                if (!FullyDownloaded && Request != null)
                {
                    Request.Abort();
                }
                // Store a stopped state
                State = StreamingState.Stopped;
                // If there is an existing player
                if (Player != null)
                {
                    // Stop the player
                    Player.Stop();
                    // Dispose the player
                    Player.Dispose();
                    // And set it to null
                    Player = null;
                }
                // Disable the timer
                HandlingTimer.Enabled = false;
                // Wait half a second
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Event executed every iteration of the timer.
        /// </summary>
        private void OnTick(object sender, ElapsedEventArgs e)
        {
            // If the stream is not stopped
            if (State != StreamingState.Stopped)
            {
                // If there is no player and no wave device
                if (Player == null && WaveProvider != null)
                {
                    // Create a new wave device
                    Player = new WaveOut();
                    // Add a volume controller
                    VolumeProvider = new VolumeWaveProvider16(WaveProvider);
                    //volumeProvider.Volume = volumeSlider1.Volume;
                    Player.Init(VolumeProvider);
                    //progressBarBuffer.Maximum = (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
                }
                // If there is a player but not a wave device
                else if (WaveProvider != null)
                {
                    // Store the buffered seconds
                    double BufferedSeconds = WaveProvider.BufferedDuration.TotalSeconds;

                    // Make it stutter less by buffering up a decent amount before playing
                    if (BufferedSeconds < 0.5 && State == StreamingState.Playing && !FullyDownloaded)
                    {
                        Pause();
                    }
                    else if (BufferedSeconds > 4 && State == StreamingState.Buffering)
                    {
                        Play();
                    }
                    else if (FullyDownloaded && BufferedSeconds == 0)
                    {
                        Stop(); // We have reached the end of the stream
                    }
                }
            }
        }

        /// <summary>
        /// Plays the stored stream.
        /// </summary>
        public void Play()
        {
            // Reproduce the player
            Player.Play();
            // Store the current state
            State = StreamingState.Playing;
        }

        /// <summary>
        /// Plays a stream on the player.
        /// </summary>
        public void Play(string URL)
        {
            // Stop the existing stream
            Stop();
            // Store a buffering state
            State = StreamingState.Buffering;
            // Remove the wave provider
            WaveProvider = null;
            // Run the streaming function on a separate thread.
            ThreadPool.QueueUserWorkItem(StreamMp3, URL);
            // Enable the timer
            HandlingTimer.Enabled = true;
        }

        /// <summary>
        /// Pauses the streaming playback.
        /// </summary>
        public void Pause()
        {
            // Store a buffering state
            State = StreamingState.Buffering;
            // And pause the player
            Player.Pause();
        }
    }
}
