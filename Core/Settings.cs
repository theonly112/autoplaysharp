using System.Runtime.CompilerServices;

// TODO: find a better place to put this.
[assembly: InternalsVisibleTo("autoplaysharp.Tests")]

namespace autoplaysharp.Core
{
    public static class Settings
    {
        public static bool SaveImages;
        public static bool SaveRawImages;
    }
}
