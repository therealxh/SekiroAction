using UnityEngine;

//攻击判定结果：一次挥刀结算的结局，返回给攻击方做反馈（如弹刀）
public enum AttackResult
{
    Miss,     //没扫到目标
    Hit,      //命中并造成伤害
    Blocked,  //被格挡（免伤）
    Ignored,  //命中但无效（无敌帧/已死亡）
}

//可受伤目标接口：玩家和敌人都实现它
//攻击判定只依赖接口，不依赖具体类型——这就是"双方共用判定框架"的基础
public interface IDamageable
{
    //受到伤害；返回本次判定的结果
    AttackResult TakeDamage(float damage);
}
