using System;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/JournalPedestal")]
    public class JournalPedestal : Entity
    {
        private TalkComponent talk;

        private MTexture image;

        private SineWave wave;

        public JournalPedestal(EntityData data, Vector2 position) : base(data.Position + position)
        {
            image = GFX.Game["objects/Xaphan/Journal/journal"];
            Add(wave = new SineWave(Calc.Random.Range(0.1f, 0.3f), Calc.Random.NextFloat() * ((float)Math.PI * 2f)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(talk = new TalkComponent(new Rectangle(0, 28, 16, 8), new Vector2(8f, -2f), Interact));
            talk.PlayerMustBeFacing = false;
            PlayerStat.GetPlayersList();
        }

        private void Interact(Player player)
        {
            if (player.Center.X > Center.X + 8f)
            {
                player.Facing = Facings.Left;
            }
            else if (player.Center.X < Center.X + 6f)
            {
                player.Facing = Facings.Right;
            }
            SceneAs<Level>().Add(new Journal(Vector2.Zero));
        }

        public override void Render()
        {
            base.Render();
            image.DrawCentered(Center + new Vector2(8f) + Vector2.UnitY * (wave.Value * 2f));
        }
    }
}
