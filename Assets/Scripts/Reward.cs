﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AngryDash.Game.RewardChecker
{
    /// <summary> For Official Levels </summary>
    public class Official
    {
        /// <summary>The maximum of throws for each star</summary>
        public List<int> starsGain
        {
            get
            {
                if (name == "1") return new List<int> { 12, 10, 8 };
                else if (name == "2") return new List<int> { 9, 7, 6 };
                else if (name == "3") return new List<int> { 6, 0, 5 };
                else if (name == "4") return new List<int> { 13, 11, 9 };
                else if (name == "5") return new List<int> { 25, 20, 15 };
                else if (name == "6") return new List<int> { 24, 20, 16 };
                else if (name == "7") return new List<int> { 24, 21, 18 };
                else if (name == "8") return new List<int> { 10, 8, 6 };
                else if (name == "9") return new List<int> { 29, 25, 21 };
                else if (name == "10") return new List<int> { 30, 25, 20 };
                else if (name == "11") return new List<int> { 25, 20, 17 };
                else if (name == "12") return new List<int> { 22, 18, 16 };
                else if (name == "13") return new List<int> { 25, 20, 17 };
                else return new List<int> { 0, 0, 0 };
            }
        }

        /// <summary> Level's name </summary>
        public string name { get; }
        /// <summary> Number of turn </summary>
        public int turn { get; set; }

        public Official(string lvlName) { name = lvlName; }

        /// <summary>The stars obtained</summary>
        public int stars { get { return starsGain.IndexOf(starsGain.LastOrDefault(s => turn <= s)) + 1; } }

        /// <summary> The money to give the player </summary>
        public int money
        {
            get
            {
                if (turn == 0)
                {
                    Debug.LogError("You are a cheater !");
                    return 0;
                }
                else
                {
                    int starsNumber = stars;
                    if (starsNumber == 3) return 25;
                    else if (starsNumber == 2) return 15;
                    else if (starsNumber == 1) return 10;
                    else return 5;
                }
            }
        }
    }
}

