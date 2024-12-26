using System.Collections;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    class Binocular : Lookout
    {
        private static FieldInfo LookoutTalk = typeof(Lookout).GetField("talk", BindingFlags.Instance | BindingFlags.NonPublic);

        private MethodInfo LookoutInteract = typeof(Lookout).GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic);

        public Binocular(EntityData data, Vector2 offset) : base(data, offset)
        {

        }

        public static void Load()
        {
            On.Celeste.Lookout.Update += onLookoutUpdate;
            On.Celeste.Lookout.LookRoutine += onLookoutLookRoutine;
        }

        public static void Unload()
        {
            On.Celeste.Lookout.Update -= onLookoutUpdate;
            On.Celeste.Lookout.LookRoutine -= onLookoutLookRoutine;
        }

        private static void onLookoutUpdate(On.Celeste.Lookout.orig_Update orig, Lookout self)
        {
            if (SaveData.Instance.LevelSetStats.Name == "Xaphan/0" && self.GetType().ToString() != "Celeste.Mod.XaphanHelper.Entities.Binocular")
            {
                self.Collider = null;
                self.Components.Remove((TalkComponent)LookoutTalk.GetValue(self));
                self.Visible = false;
                self.RemoveSelf();
            }
            orig(self);
        }

        private static IEnumerator onLookoutLookRoutine(On.Celeste.Lookout.orig_LookRoutine orig, Lookout self, Player player)
        {
            yield return new SwapImmediately(orig(self, player));
            if (self is Binocular)
            {
                self.SceneAs<Level>().Displacement.AddBurst(self.Center - Vector2.UnitY * 3.5f, 0.5f, 8f, 32f, 0.5f);
                self.RemoveSelf();
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (SceneAs<Level>().Tracker.CountEntities<Binocular>() > 1)
            {
                foreach (Entity binocular in SceneAs<Level>().Tracker.GetEntities<Binocular>())
                {
                    if (binocular != this)
                    {
                        binocular.RemoveSelf();
                    }
                }
            }
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            LookoutInteract.Invoke(this, new object[] { player });
        }
    }
}
