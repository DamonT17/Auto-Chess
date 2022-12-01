using System.Collections.Generic;

public enum ModifierType {
    Flat = 100,
    PercentAdd = 200,
    PercentMultiply = 300
}

public class Modifier {
    public readonly float Value;
    public readonly ModifierType Type;
    public readonly int Order;
    public readonly object Source;

    // "Main" constructor requiring all variables
    public Modifier(float value, ModifierType type, int order, object source) {
        Value = value;
        Type = type;
        Order = order;
        Source = source;
    }

    // Constructor requiring Value and Type. Calls "Main" constructor and sets Order and Source to default values
    public Modifier(float value, ModifierType type) : this(value, type, (int) type, null) { }

    // Constructor requiring Value, Type, and Order. Sets Source to its default value
    public Modifier(float value, ModifierType type, int order) : this(value, type, order, null) { }

    // Constructor requiring Value, Type, and Source. Sets Order to its default value
    public Modifier(float value, ModifierType type, object source) : this(value, type, (int) type, source) { }
}
