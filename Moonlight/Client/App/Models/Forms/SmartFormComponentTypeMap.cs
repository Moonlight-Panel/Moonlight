using Moonlight.Client.App.UI.Components.Forms;

namespace Moonlight.Client.App.Models.Forms;

public static class SmartFormComponentTypeMap
{
    private static Dictionary<Type, Type> Data = new();

    public static Type? Get(Type dataType)
    {
        return Data.GetValueOrDefault(dataType);
    }

    public static void Set(Type dataType, Type componentType) => Data[dataType] = componentType;
    public static void Set<TDataType, TComponentType>() where TComponentType : BaseSmartFormComponent<TDataType> => Set(typeof(TDataType), typeof(TComponentType));
}