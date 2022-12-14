namespace UmbraProjects.Utilities.Interfaces 
{
    public interface IDamageable<T> {
        void ApplyDamage(T damageAmount);
        void ApplyDamage(T damageAmount, int damageType, bool isCriticalDamage);
    }
}