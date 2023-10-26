using System.Reflection;

namespace Moonlight.App.Helpers;

public class PropBinder<T>
{
    private PropertyInfo PropertyInfo;
    private object DataObject;

    public PropBinder(PropertyInfo propertyInfo, object dataObject)
    {
        PropertyInfo = propertyInfo;
        DataObject = dataObject;
    }

    public string StringValue
    {
        get => (string)PropertyInfo.GetValue(DataObject)!;
        set => PropertyInfo.SetValue(DataObject, value);
    }

    public int IntValue
    {
        get => (int)PropertyInfo.GetValue(DataObject)!;
        set => PropertyInfo.SetValue(DataObject, value);
    }
    
    public long LongValue
    {
        get => (long)PropertyInfo.GetValue(DataObject)!;
        set => PropertyInfo.SetValue(DataObject, value);
    }
    
    public bool BoolValue
    {
        get => (bool)PropertyInfo.GetValue(DataObject)!;
        set => PropertyInfo.SetValue(DataObject, value);
    }

    public DateTime DateTimeValue
    {
        get => (DateTime)PropertyInfo.GetValue(DataObject)!;
        set => PropertyInfo.SetValue(DataObject, value);
    }

    public T Class
    {
        get => (T)PropertyInfo.GetValue(DataObject)!;
        set => PropertyInfo.SetValue(DataObject, value);
    }
    
    public double DoubleValue
    {
        get => (double)PropertyInfo.GetValue(DataObject)!;
        set => PropertyInfo.SetValue(DataObject, value);
    }
}