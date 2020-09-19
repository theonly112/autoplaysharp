using autoplaysharp.App.Logging;
using Prism.Mvvm;
using System.Text;

namespace autoplaysharp.App.UI.Log
{
    internal class LogViewModel : BindableBase
    {
        private string _logText;
        private readonly IUiLogger _uiLogger;
        private readonly StringBuilder _builder = new StringBuilder();

        public LogViewModel(IUiLogger uiLogger)
        {
            _uiLogger = uiLogger;
            _uiLogger.NewLogEntry += _uiLogger_NewLogEntry;
        }

        private void _uiLogger_NewLogEntry(string entry)
        {
            _builder.AppendLine(entry);
            LogText = _builder.ToString();
        }

        public string LogText
        {
            get { return _logText; }
            set 
            {
                _logText = value;
                RaisePropertyChanged();
            }
        }
    }
}
