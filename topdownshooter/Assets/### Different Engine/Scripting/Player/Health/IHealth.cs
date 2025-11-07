using System;

public interface IHealth
{
    float Max { get; }
    float Current { get; }
    event Action<float, float> OnHealthChanged; // (current, max)
}
