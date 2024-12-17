using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class JournalCover : JournalPage
    {
        public JournalCover(Journal journal)
            : base(journal)
        {
            PageTexture = "cover";
        }

        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            ActiveFont.Draw(Dialog.Clean("Xaphan_0_Journal_Cover"), new Vector2(805f, 400f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.Black * 0.5f);
            Draw.SpriteBatch.End();
        }
    }
}
