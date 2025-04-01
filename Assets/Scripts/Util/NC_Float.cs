using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NC_Float
{
    public float baseValue;
    public float additionalValue;
    public float percentageValue;

    public NC_Float(float baseValue = 0, float additionalValue = 0, float percentageValue = 0)
    {
        this.baseValue = baseValue;
        this.additionalValue = additionalValue;
        this.percentageValue = percentageValue;
    }

    // copy constructor
    public NC_Float(NC_Float ncFloat)
    {
        baseValue = ncFloat.baseValue;
        additionalValue = ncFloat.additionalValue;
        percentageValue = ncFloat.percentageValue;
    }

    public float finalValue
    {
        get
        {
            return baseValue + additionalValue + (baseValue * percentageValue);
        }
    }

    public static implicit operator float(NC_Float ncFloat)
    {
        return ncFloat.finalValue;
    }

    public static implicit operator NC_Float(float value)
    {
        return new NC_Float(value, 0, 0);
    }

    
    // overload comparison operators
    public static bool operator ==(NC_Float a, NC_Float b){ return a.finalValue == b.finalValue; }
    public static bool operator !=(NC_Float a, NC_Float b){ return a.finalValue != b.finalValue; }
    public static bool operator >(NC_Float a, NC_Float b){ return a.finalValue > b.finalValue; }
    public static bool operator <(NC_Float a, NC_Float b){ return a.finalValue < b.finalValue; }
    public static bool operator >=(NC_Float a, NC_Float b){ return a.finalValue >= b.finalValue; }
    public static bool operator <=(NC_Float a, NC_Float b){ return a.finalValue <= b.finalValue; }

    public override bool Equals(object obj) => obj is NC_Float ncFloat && finalValue == ncFloat.finalValue;
    public override int GetHashCode() => finalValue.GetHashCode();

    public override string ToString() =>
        $"Base: {baseValue}, Additional: {additionalValue}, Percentage: {percentageValue * 100}%, Final: {finalValue}";


    public void CopyFrom(NC_Float other)
    {
        baseValue = other.baseValue;
        additionalValue = other.additionalValue;
        percentageValue = other.percentageValue;
    }

    // overload math operators for NC_Float, only the base value is used
    public static NC_Float operator +(NC_Float a, NC_Float b) {return new NC_Float(a.baseValue + b.baseValue, 0, 0);}
    public static NC_Float operator -(NC_Float a, NC_Float b) {return new NC_Float(a.baseValue - b.baseValue, 0, 0);}
    public static NC_Float operator *(NC_Float a, NC_Float b) {return new NC_Float(a.baseValue * b.baseValue, 0, 0);}
    public static NC_Float operator /(NC_Float a, NC_Float b) {return new NC_Float(a.baseValue / b.baseValue, 0, 0);}
    public static NC_Float operator %(NC_Float a, NC_Float b) {return new NC_Float(a.baseValue % b.baseValue, 0, 0);}

    // operation between NC-Float and float will devolve to a float
    public static float operator +(NC_Float a, float b) {return a.baseValue + b;}
    public static float operator -(NC_Float a, float b) {return a.baseValue - b;}
    public static float operator *(NC_Float a, float b) {return a.baseValue * b;}
    public static float operator /(NC_Float a, float b) {return a.baseValue / b;}
    public static float operator %(NC_Float a, float b) {return a.baseValue % b;}
    public static float operator +(float a, NC_Float b) {return a + b.baseValue;}
    public static float operator -(float a, NC_Float b) {return a - b.baseValue;}
    public static float operator *(float a, NC_Float b) {return a * b.baseValue;}
    public static float operator /(float a, NC_Float b) {return a / b.baseValue;}
    public static float operator %(float a, NC_Float b) {return a % b.baseValue;}

    public void addBaseValue(float value) { baseValue += value; }
    public void addAdditionalValue(float value) { additionalValue += value; }
    public void addPercentageValue(float value) { percentageValue += value; }
    public void setBaseValue(float value) { baseValue = value; }
    public void setAdditionalValue(float value) { additionalValue = value; }
    public void setPercentageValue(float value) { percentageValue = value; }
    public void reset() { baseValue = 0; additionalValue = 0; percentageValue = 0; }

    public void addEffect(NC_Float effect)
    {
        baseValue += effect.baseValue;
        additionalValue += effect.additionalValue;
        percentageValue += effect.percentageValue;
    }
}
