using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IMusicSyncException.Common
{
    public class NotifiablePropertyChangedArgs
    {
        public string PropertyName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public NotifiablePropertyChangedArgs(string propertyName, object newValue, object oldValue = null)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public class Notifiable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<NotifiablePropertyChangedArgs> PropertyValueChanged;

        public void NotifyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyValueChange(string propertyName, object newValue, object oldValue = null)
        {
            PropertyValueChanged?.Invoke(this, new NotifiablePropertyChangedArgs(propertyName, newValue, oldValue));
        }

        public bool SetValue<T>(ref T property, T value, Action onChanged = null, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
            {
                return false;
            }

            property = value;

            onChanged?.Invoke();
            NotifyChange(propertyName);

            return true;
        }

        public bool SetValueChange<T>(ref T property, T value, Action onChanged = null, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
            {
                return false;
            }

            NotifyValueChange(propertyName, value, property);
            property = value;

            onChanged?.Invoke();
            NotifyChange(propertyName);

            return true;
        }

        public bool SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = null, params string[] otherProperties)
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
            {
                return false;
            }

            property = value;

            NotifyChange(propertyName);
            foreach (string otherProperty in otherProperties)
            {
                NotifyChange(otherProperty);
            }

            return true;
        }

        public bool SetValueChange<T>(ref T property, T value, [CallerMemberName] string propertyName = null, params string[] otherProperties)
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
            {
                return false;
            }

            NotifyValueChange(propertyName, value, property);
            property = value;

            NotifyChange(propertyName);
            foreach (string otherProperty in otherProperties)
            {
                NotifyChange(otherProperty);
            }

            return true;
        }
    }
}
