﻿using Monocle;

namespace Celeste.Mod.XaphanHelper.Data
{
    public class InGameMapHintControllerData
    {
        public int ChapterIndex;

        public string Room;

        public int TileCordX;

        public int TileCordY;

        public string[] DisplayFlags;

        public string HideFlag;

        public string Type;

        public string Direction;

        public bool RemoveWhenReachedByPlayer;

        public bool HideOnWorldMap;

        public Sprite Sprite;

        public InGameMapHintControllerData(int chapterIndex, string room, string tileCord, string[] displayFlags, string hideFlag, string type, string direction, bool removeWhenReachedByPlayer, bool hideOnWorldMap)
        {
            ChapterIndex = chapterIndex;
            Room = room;
            string[] str = tileCord.Split('-');
            TileCordX = int.Parse(str[0]);
            TileCordY = int.Parse(str[1]);
            DisplayFlags = displayFlags;
            HideFlag = hideFlag;
            Type = type;
            Direction = direction;
            RemoveWhenReachedByPlayer = removeWhenReachedByPlayer;
            HideOnWorldMap = hideOnWorldMap;
            Sprite = new Sprite(GFX.Gui, "maps/");
            string spriteAnim = Type == "Arrow" ? "hintArrow" : "hint";
            Sprite.AddLoop("hint", spriteAnim, 0.15f);
            Sprite.Play("hint");
            Sprite.Visible = false;
        }
    }
}
