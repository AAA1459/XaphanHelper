using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class JournalClears : JournalPage
    {
        private string pageTitle;

        private Table table;

        private List<PlayerStatData> players;

        private int firstPlayerShown;

        private int lastPlayerShown;

        public JournalClears(Journal journal, int category) : base(journal)
        {
            PageTexture = "page";
            switch (category)
            {
                case 1:
                    pageTitle = "Xaphan_0_Journal_Clears";
                    players = PlayerStat.GeneratePlayersClearsList();
                    break;
                case 2:
                    pageTitle = "Xaphan_0_Journal_Full_Clears";
                    players = PlayerStat.GeneratePlayersFullClearsList();
                    break;
                case 3:
                    pageTitle = "Xaphan_0_Journal_Golden_Clears";
                    players = PlayerStat.GeneratePlayersGoldenClearsList();
                    break;
                case 4:
                    pageTitle = "Xaphan_0_Journal_Golden_Full_Clears";
                    players = PlayerStat.GeneratePlayersGoldenFullClearsList();
                    break;
            }
            if (players.Count > 0)
            {
                int playersPerPage = 12;
                table = new Table()
                .AddColumn(new TextCell(Dialog.Clean(pageTitle).ToUpper(), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f, 60f, true)
                {
                    SpreadOverColumns = 2
                })
                .AddColumn(new EmptyCell(500f))
                .AddColumn(new IconCell("medal", 75f))
                .AddColumn(new IconCell("heartgem0", 60f))
                .AddColumn(new IconCell("heartgem2", 60f))
                .AddColumn(new IconCell("strawberry", 135f))
                .AddColumn(new IconCell("skullblue", 100f))
                .AddColumn(new IconCell("time", 220f))
                .AddColumn(new TextCell(Dialog.Clean("Xaphan_0_Journal_Version").ToUpper(), new Vector2(0.5f), 0.5f, Color.Black * 0.7f, 60f, true))
                .AddColumn(new EmptyCell(50f));
                firstPlayerShown = 0;
                lastPlayerShown = players.Count > playersPerPage ? playersPerPage : players.Count;
                for (int i = 0; i < lastPlayerShown; i++)
                {
                    Row row = table.AddRow();
                    if (i == 0)
                    {
                        row.Add(new IconCell("trophy_gold", 60f));
                    }
                    else if (i == 1)
                    {
                        row.Add(new IconCell("trophy_silver", 60f));
                    }
                    else if (i == 2)
                    {
                        row.Add(new IconCell("trophy_bronze", 60f));
                    }
                    else
                    {
                        row.Add(new EmptyCell(60f));
                    }
                    row.Add(new TextCell(players[i].Name, new Vector2(0f, 0.5f), 0.6f, TextColor))
                        .Add(new TextCell(players[i].Medals.ToString(), TextJustify, 0.5f, TextColor))
                        .Add(new TextCell(players[i].BlueCrystalHearts.ToString(), TextJustify, 0.5f, TextColor))
                        .Add(new TextCell(players[i].YellowCrystalHearts.ToString(), TextJustify, 0.5f, TextColor))
                        .Add(new TextCell(players[i].Strawberries + "/135", TextJustify, 0.5f, TextColor))
                        .Add(new TextCell(players[i].Deaths.ToString(), TextJustify, 0.5f, TextColor))
                        .Add(new TextCell(players[i].Time, TextJustify, 0.5f, TextColor))
                        .Add(new TextCell(players[i].Version, TextJustify, 0.5f, TextColor));
                    if ((i == firstPlayerShown && firstPlayerShown != 0) || (i == lastPlayerShown - 1 && lastPlayerShown != players.Count))
                    {
                        row.Add(new IconCell("pageArrow", rotation: (i == firstPlayerShown ? -(float)Math.PI / 2 : (float)Math.PI / 2)));
                    }
                }
            }
            else
            {
                table = new Table()
                    .AddColumn(new TextCell(Dialog.Clean(pageTitle).ToUpper(), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f, 400f, true));
            }
        }

        public override void Update()
        {
            base.Update();
            if (players.Count > 0)
            {
                if (Input.MenuDown.Pressed)
                {
                    if (players.Count > lastPlayerShown)
                    {
                        firstPlayerShown += 1;
                        lastPlayerShown += 1;
                        RedrawTable();
                    }
                }
                if (Input.MenuUp.Pressed)
                {
                    if (firstPlayerShown > 0)
                    {
                        firstPlayerShown -= 1;
                        lastPlayerShown -= 1;
                        RedrawTable();
                    }
                }
            }
        }

        private void RedrawTable()
        {
            table = null;
            table = new Table()
                .AddColumn(new TextCell(Dialog.Clean(pageTitle).ToUpper(), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f, 60f, true)
                {
                    SpreadOverColumns = 2
                })
                .AddColumn(new EmptyCell(500f))
                .AddColumn(new IconCell("medal", 75f))
                .AddColumn(new IconCell("heartgem0", 60f))
                .AddColumn(new IconCell("heartgem2", 60f))
                .AddColumn(new IconCell("strawberry", 135f))
                .AddColumn(new IconCell("skullblue", 100f))
                .AddColumn(new IconCell("time", 220f))
                .AddColumn(new TextCell(Dialog.Clean("Xaphan_0_Journal_Version").ToUpper(), new Vector2(0.5f), 0.5f, Color.Black * 0.7f, 60f, true))
                .AddColumn(new EmptyCell(50f));
            for (int i = firstPlayerShown; i < lastPlayerShown; i++)
            {
                Row row = table.AddRow();
                if (i == 0)
                {
                    row.Add(new IconCell("trophy_gold", 60f));
                }
                else if (i == 1)
                {
                    row.Add(new IconCell("trophy_silver", 60f));
                }
                else if (i == 2)
                {
                    row.Add(new IconCell("trophy_bronze", 60f));
                }
                else
                {
                    row.Add(new EmptyCell(60f));
                }
                row.Add(new TextCell(players[i].Name, new Vector2(0f, 0.5f), 0.6f, TextColor))
                    .Add(new TextCell(players[i].Medals.ToString(), TextJustify, 0.5f, TextColor))
                    .Add(new TextCell(players[i].BlueCrystalHearts.ToString(), TextJustify, 0.5f, TextColor))
                    .Add(new TextCell(players[i].YellowCrystalHearts.ToString(), TextJustify, 0.5f, TextColor))
                    .Add(new TextCell(players[i].Strawberries + "/135", TextJustify, 0.5f, TextColor))
                    .Add(new TextCell(players[i].Deaths.ToString(), TextJustify, 0.5f, TextColor))
                    .Add(new TextCell(players[i].Time, TextJustify, 0.5f, TextColor))
                    .Add(new TextCell(players[i].Version, TextJustify, 0.5f, TextColor));
                if ((i == firstPlayerShown && firstPlayerShown != 0) || (i == lastPlayerShown - 1 && lastPlayerShown != players.Count))
                {
                    row.Add(new IconCell("pageArrow", rotation: (i == firstPlayerShown ? -(float)Math.PI / 2 : (float)Math.PI / 2)));
                }
            }
            ForceRedraw = true;
        }

        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            table.Render(new Vector2(60f, 20f));
            if (players.Count == 0)
            {
                ActiveFont.Draw(Dialog.Clean("Xaphan_0_Journal_Empty"), new Vector2(805f, 400f), new Vector2(0.5f, 0.5f), Vector2.One, Color.Black * 0.7f);
            }
            Draw.SpriteBatch.End();
        }

        private void DrawIcon(Vector2 pos, bool obtained, string icon)
        {
            if (obtained)
            {
                MTN.Journal[icon].DrawCentered(pos);
            }
            else
            {
                MTN.Journal["dot"].DrawCentered(pos);
            }
        }
    }
}
