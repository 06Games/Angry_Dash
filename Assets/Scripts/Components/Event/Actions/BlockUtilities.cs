using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace AngryDash.Game.API
{
    public class BlockUtilities
    {
        public static List<int> GetBlocks(int group)
        {
            var Blocks = Player.userPlayer.LP.level.blocks;
            var blocks = new List<int>();
            for (var i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i].parameter.ContainsKey("Groups"))
                {
                    if (Blocks[i].parameter["Groups"].Split(", ").Contains(group.ToString())) blocks.Add(i);
                }
            };
            return blocks;
        }

        public static void Color(int blockGroup, byte r, byte g, byte b)
        {
            UnityThread.executeInUpdate(() =>
            {
                var color = new Color32(r, g, b, 255);
                foreach (var block in GetBlocks(blockGroup))
                {
                    var go = GameObject.Find("Items").transform.Find($"Objet n° {block}").gameObject;
                    if (go != null) go.GetComponent<SpriteRenderer>().color = color;
                }
            });
        }
        public static void Active(int blockGroup, string enable)
        {
            UnityThread.executeInUpdate(() =>
            {
                foreach (var block in GetBlocks(blockGroup))
                {
                    var go = GameObject.Find("Items").transform.Find($"Objet n° {block}").gameObject;
                    if (go != null) go.SetActive(enable == "True");
                }
            });
        }
    }
}
