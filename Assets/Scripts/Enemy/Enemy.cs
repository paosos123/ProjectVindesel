using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHp = 3;
    [SerializeField] protected int currentHpInternal; // เปลี่ยนชื่อตัวแปรภายใน
    private bool isDeadInternal = false; // เปลี่ยนชื่อตัวแปรภายใน

    public int CurrentHp
    {
        get { return currentHpInternal; }
        protected set { currentHpInternal = value; } // อนุญาตให้คลาสลูกแก้ไขได้
    }

    public bool IsDead
    {
        get { return isDeadInternal; }
        protected set { isDeadInternal = value; } // อนุญาตให้คลาสลูกแก้ไขได้
    }

    public delegate void EnemyDamaged(int currentHealth, int damageTaken);
    public event EnemyDamaged OnEnemyDamaged;

    public delegate void EnemyKilled();
    public event EnemyKilled OnEnemyKilled;

    protected virtual void Start()
    {
        CurrentHp = maxHp; // ใช้ Property ในการตั้งค่า
        IsDead = false; // ใช้ Property ในการตั้งค่า
    }

    public virtual void TakeDamage(int damageAmount)
    {
        Debug.Log(gameObject.name + " TakeDamage called with damage: " + damageAmount + ", currentHp before: " + CurrentHp + ", isDead: " + IsDead);

        if (IsDead) // ใช้ Property ในการตรวจสอบ
        {
            Debug.Log(gameObject.name + " is already dead, returning.");
            return;
        }

        CurrentHp -= damageAmount; // ใช้ Property ในการแก้ไข
        Debug.Log(gameObject.name + " currentHp after damage: " + CurrentHp);

        OnEnemyDamaged?.Invoke(CurrentHp, damageAmount);

        if (CurrentHp <= 0 && !IsDead) // ใช้ Property ในการตรวจสอบ
        {
            IsDead = true; // ใช้ Property ในการแก้ไข
            Debug.Log(gameObject.name + " HP depleted, calling Die().");
            Die();
        }
    }

  protected virtual void Die()
{
    Debug.Log(gameObject.name + " Die() called. Invoking OnEnemyKilled event.");
    OnEnemyKilled?.Invoke();
    Destroy(gameObject);
}
    void Update()
    {
        // ... (โค้ด Update อื่นๆ ถ้ามี) ...
    }
}