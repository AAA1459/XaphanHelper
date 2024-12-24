using System;
using Celeste.Mod.XaphanHelper.Enemies;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Colliders
{
    [Tracked(false)]
    public class EnemyCollider : Component
    {
        public Action<Enemy> OnCollide;

        public Collider Collider;

        public EnemyCollider(Action<Enemy> onCollide, Collider collider = null) : base(active: false, visible: false)
        {
            OnCollide = onCollide;
            Collider = collider;
        }

        public bool Check(Enemy enemy)
        {
            Collider collider = Collider;
            if (collider == null)
            {
                if (enemy.CollideCheck(Entity))
                {
                    OnCollide(enemy);
                    return true;
                }
                return false;
            }
            Collider collider2 = Entity.Collider;
            Entity.Collider = collider;
            bool flag = enemy.CollideCheck(Entity);
            Entity.Collider = collider2;
            if (flag)
            {
                OnCollide(enemy);
                return true;
            }
            return false;
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                Collider collider = Entity.Collider;
                Entity.Collider = Collider;
                Collider.Render(camera, Color.HotPink);
                Entity.Collider = collider;
            }
        }
    }
}
