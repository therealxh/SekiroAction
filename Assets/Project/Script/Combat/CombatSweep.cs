using UnityEngine;

//攻击扫掠判定：从攻击者胸前向前方做盒型扫掠(BoxCast)
//命中可受伤目标则结算伤害；玩家与敌人共用同一套判定逻辑
public static class CombatSweep
{
    private const float MaxDistance = 1.6f;//扫掠距离(米)
    private static readonly Vector3 HalfExtents = new Vector3(0.4f, 0.6f, 0.4f);//判定盒半尺寸

    //执行一次攻击判定
    //attacker:攻击者；damage:攻击力
    //返回结算结果，供攻击方做弹刀等反馈
    public static AttackResult Perform(Transform attacker, float damage)
    {
        //起点：攻击者胸前1米高；方向：攻击者正前方
        Vector3 origin = attacker.position + Vector3.up;
        if (!Physics.BoxCast(origin, HalfExtents, attacker.forward,
                out RaycastHit hit, attacker.rotation, MaxDistance))
        {
            return AttackResult.Miss;//前方没有碰撞体
        }
        //扫到攻击者自身（擦到自己碰撞体）→视为未命中
        if (hit.transform.root == attacker.root) return AttackResult.Miss;
        //目标是否可受伤（挂有实现 IDamageable 的组件）
        IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
        if (target == null) return AttackResult.Miss;
        return target.TakeDamage(damage);
    }
}
