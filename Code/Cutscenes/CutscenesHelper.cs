using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    internal class CutscenesHelper
    {
        // Badeline movements

        public static BadelineDummy BadelineAppears(Level level, Vector2 position)
        {
            BadelineDummy badeline = new BadelineDummy(position);
            level.Add(badeline);
            badeline.Appear(level);
            return badeline;
        }

        public static BadelineDummy BadelineSplit(Level level, Player player)
        {
            BadelineDummy badeline = new BadelineDummy(player.Position);
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            level.Add(badeline);
            level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
            return badeline;
        }

        public static IEnumerator BadelineFloat(CutsceneEntity cutscene, int x, int y, BadelineDummy badeline, int? turnAtEndTo, bool faceDirection, bool fadeLight, bool quickEnd)
        {
            Vector2 badelineEndPosition = new Vector2(badeline.Position.X + x, badeline.Position.Y + y);
            yield return badeline.FloatTo(badelineEndPosition, turnAtEndTo, faceDirection, fadeLight, quickEnd);
            while (badeline.Position != badelineEndPosition)
            {
                yield return null;
            }
        }

        public static IEnumerator BadelineMerge(Level level, Player player, BadelineDummy badeline)
        {
            yield return badeline.FloatTo(player.Position, null, true, false, true);
            while (badeline.Position != player.Position)
            {
                yield return null;
            }
            Audio.Play("event:/new_content/char/badeline/maddy_join_quick", badeline.Position);
            level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
            badeline.RemoveSelf();
        }
    }
}
