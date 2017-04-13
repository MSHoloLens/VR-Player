using Windows.ApplicationModel.Core;

namespace VR_Player
{
    // The entry point for the app.
    internal class AppViewSource : IFrameworkViewSource
    {
        public IFrameworkView CreateView()
        {
            return new AppView();
        }
    }
}
