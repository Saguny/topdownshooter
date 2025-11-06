using UnityEngine;
using System;

public static class GameEvents
{
    public static Action<int> OnGearsChanged;
    public static Action<int> OnWaveStarted;
    public static Action<int> OnWaveCleared;
    public static Action<int> OnEnemyKilled;
    public static Action<float> OnRunTimeChanged;
}
