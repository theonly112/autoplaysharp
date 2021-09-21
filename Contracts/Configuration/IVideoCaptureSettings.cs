namespace autoplaysharp.Contracts.Configuration
{
    public interface IVideoCaptureSettings
    {
        bool Enabled { get; set; }
        string RecordingDir { get; set; }
        double FrameRate { get; set; }
    }
}
