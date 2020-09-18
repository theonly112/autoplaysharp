namespace autoplaysharp.Overlay
{
    public abstract class OverlayElement
    {
        private int _duration;

        public OverlayElement(int duration = 3000)
        {
            _duration = duration;
        }
        public bool CanBeRemoved()
        {
            return _duration <= 0;
        }

        public virtual void Render(int delta)
        {
            _duration -= delta;
        }
    }
}
