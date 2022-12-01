using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

[Serializable]
public class Attribute {
    public float BaseValue;
    public readonly ReadOnlyCollection<Modifier> AttributeModifiers;

    protected bool _isDirty = true;
    protected float _value;
    protected float _lastBaseValue = float.MinValue;
    protected readonly List<Modifier> _attributeModifiers;

    public virtual float Value {
        get {
            if (!_isDirty && Math.Abs(_lastBaseValue - BaseValue) < 0.0001f) {
                return _value;
            }

            _lastBaseValue = BaseValue;
            _value = CalculateFinalValue();
            _isDirty = false;

            return _value;
        }
    }

    public Attribute() {
        _attributeModifiers = new List<Modifier>();
        AttributeModifiers = _attributeModifiers.AsReadOnly();
    }

    public Attribute(float baseValue) : this() {
        BaseValue = baseValue;
    }

    public virtual void AddModifier(Modifier modifier) {
        _isDirty = true;
        _attributeModifiers.Add(modifier);
        _attributeModifiers.Sort(CompareModifierOrder);
    }

    public virtual bool RemoveModifier(Modifier modifier) {
        if (!_attributeModifiers.Remove(modifier)) { 
            return false;
        }

        _isDirty = true;
        return true;
    }

    public virtual bool RemoveAllModifiersFromSource(object source) {
        var didRemove = false;

        for (var i = _attributeModifiers.Count - 1; i >= 0; --i)
        {
            if (_attributeModifiers[i].Source != source)
            {
                continue;
            }

            _isDirty = true;
            didRemove = true;

            _attributeModifiers.RemoveAt(i);
        }

        return didRemove;
    }

    protected virtual int CompareModifierOrder(Modifier a, Modifier b) {
        if (a.Order < b.Order)
        {
            return -1;
        }
        else if (a.Order > b.Order)
        {
            return 1;
        }

        return 0;
    }

    protected virtual float CalculateFinalValue() {
        var finalValue = BaseValue;
        var sumPercentAdd = 0f;

        for (var i = 0; i < _attributeModifiers.Count; ++i) {
            var modifier = _attributeModifiers[i];

            if (modifier.Type == ModifierType.Flat) {
                finalValue += modifier.Value;
            }
            else if (modifier.Type == ModifierType.PercentAdd) {
                sumPercentAdd += modifier.Value;

                if (i + 1 >= _attributeModifiers.Count || _attributeModifiers[i + 1].Type != ModifierType.PercentAdd) {
                    finalValue *= 1 + sumPercentAdd;
                    sumPercentAdd = 0f;
                }
            }
            else if (modifier.Type == ModifierType.PercentMultiply) {
                finalValue *= 1 + modifier.Value;
            }
        }

        return (float) Math.Round(finalValue, 4);
    }
    
}
