using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/AirBubbles")]
    class AirBubbles : Entity
    {
        private Sprite sprite;

        public VertexLight Light;

        public AirBubbles(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Collider = new Hitbox(6f, 6f, -3f, 2f);
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/AirBubbles/"));
            sprite.AddLoop("idle", "idle", 0.3f);
            sprite.Color = Color.White * 0.7f;
            sprite.CenterOrigin();
            sprite.Play("idle");
            Add(Light = new VertexLight(new Vector2(0f, 6f), Color.White, 1f, 16, 24));
        }
    }
}
