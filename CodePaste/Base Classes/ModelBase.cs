using System.Collections.Generic;
using System.ComponentModel;

namespace CodePaste.Base_Classes
{
    public abstract class ModelBase : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyChangedEventArgs> _argsCache =
        new Dictionary<string, PropertyChangedEventArgs>();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (_argsCache != null)
            {
                if (!_argsCache.ContainsKey(propertyName))
                    _argsCache[propertyName] = new PropertyChangedEventArgs(propertyName);

                OnPropertyChanged(_argsCache[propertyName]);
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}