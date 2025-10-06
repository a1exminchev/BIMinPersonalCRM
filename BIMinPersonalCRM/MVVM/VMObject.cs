using System.ComponentModel;

namespace BIMinPersonalCRM.MVVM
{
    /// <summary>
    ///     Базовая реализация <see cref="INotifyPropertyChanged"/> для вью-моделей.
    ///     Наследники вызывают <see cref="OnPropertyChanged"/> при изменении привязанных свойств.
    /// </summary>
    public abstract class VMObject : PropertyNotifier
    {
        private IDictionary<string, object> m_values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public T GetValue<T>(string key)
        {
            var value = GetValue(key);
            return (value is T) ? (T)value : default(T);
        }

        private object GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            return m_values.ContainsKey(key) ? m_values[key] : null;
        }

        public void SetValue(string key, object value)
        {
            if (!m_values.ContainsKey(key))
            {
                m_values.Add(key, value);
            }
            else
            {
                m_values[key] = value;
            }
            OnPropertyChanged(key);
        }
    }

    public abstract class PropertyNotifier : INotifyPropertyChanged
    {
        public PropertyNotifier() : base() { }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
