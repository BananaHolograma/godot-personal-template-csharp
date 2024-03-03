
using Godot;
using System.Collections.Generic;

namespace GameRoot;

[Icon("res://components/behaviour/health-component/suit_hearts.svg")]
public partial class HealthComponent : Node
{
    public enum TYPES
    {
        DAMAGE,
        HEALTH,
        REGEN
    }
    [Signal]
    public delegate void HealthChangedEventHandler(int amount, int type);
    [Signal]
    public delegate void InvulnerabilityChangedEventHandler(bool active);
    [Signal]
    public delegate void DiedEventHandler();

    [ExportGroup("Health parameters")]
    [Export] public int MaxHealth = 100;
    [Export] public float HealthOverflowPercentage = 0.0f;
    [Export]
    public int CurrentHealth
    {
        get => CurrentHealth;
        set { _currentHealth = Mathf.Clamp(value, 0, MaxHealthOverflow); }
    }

    [ExportGroup("Additional behaviours")]
    [Export] public int HealthRegen = 0;
    [Export] public float HealthRegenTickTime = 1.0f;
    [Export] public bool IsInvulnerable = false;
    [Export] public float InvulnerabilityTime = 1.0f;

    public int MaxHealthOverflow { get { return MaxHealth + (int)(MaxHealth * HealthOverflowPercentage / 100); } }
    public Timer InvulnerabilityTimer;
    public Timer HealthRegenTimer;
    private int _currentHealth = 100;

    public override void _Ready()
    {
        EnableHealthRegen(HealthRegen, HealthRegenTickTime);
        EnableInvulnerability(IsInvulnerable, InvulnerabilityTime);

        HealthChanged += OnHealthChanged;
        Died += OnDied;
    }

    public void Health(int amount, TYPES type = TYPES.HEALTH)
    {
        amount = Mathf.Abs(amount);
        CurrentHealth += amount;

        EmitSignal(SignalName.HealthChanged, amount, (int)type);
    }

    public void Damage(int amount, TYPES type = TYPES.DAMAGE)
    {
        if (IsInvulnerable) amount = 0;

        amount = Mathf.Abs(amount);
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        EmitSignal(SignalName.HealthChanged, amount, (int)type);
    }

    public bool CheckIsDead()
    {
        bool IsDead = CurrentHealth <= 0;

        if (IsDead) EmitSignal(SignalName.Died);

        return IsDead;
    }

    public Dictionary<string, float> GetHealthPercent()
    {
        float CurrentHealthPercentage = Mathf.Snapped(CurrentHealth / (float)MaxHealth, 0.01f);

        return new Dictionary<string, float>{
            {"CurrentHealthPercentage", Mathf.Min(CurrentHealthPercentage, 1.0f)},
            {"OverflowHealthPercentage", Mathf.Max(0.0f, CurrentHealthPercentage - 1.0f)},
            {"OverflowHealth", Mathf.Max(0, CurrentHealth - MaxHealth)}
        };
    }

    public void EnableInvulnerability(bool enable, float? time = null)
    {
        if (enable != IsInvulnerable)
        {
            EmitSignal(SignalName.InvulnerabilityChanged, enable);
        }

        CreateInvulnerabilityTimer(time ?? InvulnerabilityTime);
        IsInvulnerable = enable;
        InvulnerabilityTime = time ?? InvulnerabilityTime;

        if (IsInvulnerable)
        {
            if (InvulnerabilityTime > 0)
            {
                InvulnerabilityTimer.Start();
            }
        }
        else
        {
            InvulnerabilityTimer.Stop();
        }
    }

    public void EnableHealthRegen(int? amount = null, float? time = null)
    {
        HealthRegen = amount ?? HealthRegen;
        HealthRegenTickTime = time ?? HealthRegenTickTime;

        CreateHealthRegenTimer(HealthRegenTickTime);

        if (HealthRegenTimer is not null)
        {
            if (CurrentHealth >= MaxHealth && (HealthRegenTimer.TimeLeft > 0 || HealthRegen <= 0))
            {
                HealthRegenTimer.Stop();
                return;
            }
        }

        if (HealthRegen > 0)
        {
            if (HealthRegenTickTime != HealthRegenTimer.WaitTime)
            {
                HealthRegenTimer.Stop();
                HealthRegenTimer.WaitTime = HealthRegenTickTime;
            }

            if (HealthRegenTimer.TimeLeft == 0 || HealthRegenTimer.IsStopped())
            {
                HealthRegenTimer.Start();
            }
        }
    }

    private void CreateHealthRegenTimer(float time = 1.0f)
    {

        if (HealthRegenTimer is not null)
        {
            if (HealthRegenTimer.WaitTime != time && time > 0)
            {
                HealthRegenTimer.Stop();
                HealthRegenTimer.WaitTime = time;
            }
        }
        else
        {
            HealthRegenTimer = new()
            {
                Name = "HealthRegenTimer",
                WaitTime = Mathf.Max(0.05, time),
                OneShot = false,
                Autostart = false
            };

            AddChild(HealthRegenTimer);
            HealthRegenTimer.Timeout += OnHealthRegenTimerTimeout;
        }
    }

    private void CreateInvulnerabilityTimer(float time = 1.0f)
    {
        if (InvulnerabilityTimer is not null)
        {
            if (InvulnerabilityTimer.WaitTime != time && time > 0)
            {
                InvulnerabilityTimer.Stop();
                InvulnerabilityTimer.WaitTime = time;
            }
        }
        else
        {
            InvulnerabilityTimer = new()
            {
                Name = "InvulnerabilityTimer",
                WaitTime = Mathf.Max(0.05, time),
                OneShot = true,
                Autostart = false
            };

            AddChild(InvulnerabilityTimer);
            InvulnerabilityTimer.Timeout += OnInvulnerabilityTimerTimeout;
        }
    }

    private void OnHealthChanged(int amount, int type)
    {
        if (type == (int)TYPES.DAMAGE)
        {
            EnableHealthRegen();
            Callable.From(CheckIsDead).CallDeferred();
        }
    }

    private void OnDied()
    {
        HealthRegenTimer.Stop();
        InvulnerabilityTimer.Stop();
    }

    private void OnHealthRegenTimerTimeout()
    {
        Health(HealthRegen, TYPES.REGEN);
    }

    private void OnInvulnerabilityTimerTimeout()
    {
        EnableInvulnerability(false);
    }
}