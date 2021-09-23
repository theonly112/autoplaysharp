namespace autoplaysharp.Contracts
{
    public interface IVideoCapture
    {
        void Start(string sessionName);
        void End();
    }
}
