using Unity;

public interface IDamageable
{
    void TakeDamage(int amount, DamageSource source);
}
public interface IPsycotic
{
    //later add psycotic specific methods
}
public enum DamageSource
{
    Enemy,
    Player,
    Environment,
    Boss
}