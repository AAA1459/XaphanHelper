using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Data
{
    public class InGameMapRoomData
    {
        public Vector2 Position;

        public Vector2 Size;

        public InGameMapRoomData(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }
    }
}
