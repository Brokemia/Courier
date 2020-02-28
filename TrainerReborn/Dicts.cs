using System;
using System.Collections.Generic;

namespace TrainerReborn {
    public static class Dicts {
        public static SortedDictionary<string, string> levelDict;
        public static SortedDictionary<string, string> itemDict;
        public static SortedDictionary<string, SortedDictionary<int, float[]>> tpDict;

        public static void InitLevelDict() {
            levelDict = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
            {
                "AutumnHills",
                ELevel.Level_02_AutumnHills.ToString()
            },
            {
                "BambooCreek",
                ELevel.Level_06_A_BambooCreek.ToString()
            },
            {
                "Beach",
                ELevel.Level_16_Beach.ToString()
            },
            {
                "Catacombs",
                ELevel.Level_04_Catacombs.ToString()
            },
            {
                "CloudRuins",
                ELevel.Level_11_A_CloudRuins.ToString()
            },
            {
                "CorruptedFuture",
                ELevel.Level_14_CorruptedFuture.ToString()
            },
            {
                "DarkCave",
                ELevel.Level_04_B_DarkCave.ToString()
            },
            {
                "ElementalSkylands",
                ELevel.Level_09_B_ElementalSkylands.ToString()
            },
            {
                "ForlornTemple",
                ELevel.Level_03_ForlornTemple.ToString()
            },
            {
                "GlacialPeak",
                ELevel.Level_09_A_GlacialPeak.ToString()
            },
            {
                "HowlingGrotto",
                ELevel.Level_05_A_HowlingGrotto.ToString()
            },
            {
                "MusicBox",
                ELevel.Level_11_B_MusicBox.ToString()
            },
            {
                "NinjaVillage",
                ELevel.Level_01_NinjaVillage.ToString()
            },
            {
                "QuillshroomMarsh",
                ELevel.Level_07_QuillshroomMarsh.ToString()
            },
            {
                "RiviereTurquoise",
                ELevel.Level_04_C_RiviereTurquoise.ToString()
            },
            {
                "SearingCrags",
                ELevel.Level_08_SearingCrags.ToString()
            },
            {
                "SunkenShrine",
                ELevel.Level_05_B_SunkenShrine.ToString()
            },
            {
                "Surf",
                ELevel.Level_15_Surf.ToString()
            },
            {
                "TowerOfTime",
                ELevel.Level_10_A_TowerOfTime.ToString()
            },
            {
                "UnderWorld",
                ELevel.Level_12_UnderWorld.ToString()
            },
            {
                "Volcano",
                ELevel.Level_17_Volcano.ToString()
            },
            {
                "VolcanoChase",
                ELevel.Level_18_Volcano_Chase.ToString()
            }
        };
        }

        public static void InitItemDict() {
            itemDict = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
            {
                "TimeShard",
                "0"
            },
            {
                "TimeTravel",
                "3"
            },
            {
                "RopeDart",
                "4"
            },
            {
                "Wingsuit",
                "6"
            },
            {
                "ClimbingClaws",
                "7"
            },
            {
                "KeyOfCourage",
                "11"
            },
            {
                "KeyOfHope",
                "12"
            },
            {
                "KeyOfLove",
                "13"
            },
            {
                "KeyOfStrength",
                "14"
            },
            {
                "KeyOfChaos",
                "15"
            },
            {
                "KeyOfSymbiosis",
                "16"
            },
            {
                "KeysAll",
                "11-16"
            },
            {
                "MagicSeashell",
                "19"
            },
            {
                "PhobekinNecro",
                "20"
            },
            {
                "PhobekinAcro",
                "21"
            },
            {
                "PhobekinClaustro",
                "22"
            },
            {
                "PhobekinPyro",
                "23"
            },
            {
                "PhobekinsAll",
                "20-23"
            },
            {
                "PowerThistle",
                "25"
            },
            {
                "Candle",
                "29"
            },
            {
                "LigthfootTabi",
                "40"
            },
            {
                "Scroll",
                "50"
            },
            {
                "DemonKingCrown",
                "51"
            },
            {
                "Map",
                "52"
            },
            {
                "RuxxtinAmulet",
                "55"
            },
            {
                "TeaSeed",
                "56"
            },
            {
                "TeaLeaves",
                "57"
            },
            {
                "CrestSun",
                "58"
            },
            {
                "CrestMoon",
                "59"
            },
            {
                "MagicFirefly",
                "60"
            }
        };
        }

        public static void InitTpDict() {
            tpDict = new SortedDictionary<string, SortedDictionary<int, float[]>>(StringComparer.InvariantCultureIgnoreCase);
            tpDict.Add("NinjaVillage", new SortedDictionary<int, float[]>
            {
            {
                -1,
                new float[2]
                {
                    -434.5f,
                    -51f
                }
            },
            {
                -2,
                new float[2]
                {
                    -153.3f,
                    -56.5f
                }
            }
        });
            tpDict.Add("AutumnHills", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -45.5f,
                    -89f
                }
            },
            {
                1,
                new float[2]
                {
                    238.5f,
                    -74f
                }
            },
            {
                2,
                new float[2]
                {
                    407.5f,
                    -74f
                }
            },
            {
                3,
                new float[2]
                {
                    607.48f,
                    -35f
                }
            },
            {
                4,
                new float[2]
                {
                    892.5f,
                    -27f
                }
            },
            {
                5,
                new float[2]
                {
                    68.5f,
                    -111f
                }
            },
            {
                6,
                new float[2]
                {
                    175.5f,
                    -148f
                }
            },
            {
                7,
                new float[2]
                {
                    91.5f,
                    -87f
                }
            },
            {
                8,
                new float[2]
                {
                    718.5f,
                    -73f
                }
            },
            {
                -4,
                new float[2]
                {
                    485f,
                    -101.5f
                }
            },
            {
                -3,
                new float[2]
                {
                    679.7f,
                    -138.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    968.5f,
                    -26.5f
                }
            },
            {
                -1,
                new float[2]
                {
                    -304.8f,
                    -72.5f
                }
            }
        });
            tpDict.Add("ForlornTemple", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    0.5f,
                    -11f
                }
            },
            {
                1,
                new float[2]
                {
                    88.5f,
                    -10f
                }
            },
            {
                2,
                new float[2]
                {
                    124.5f,
                    47f
                }
            },
            {
                3,
                new float[2]
                {
                    260.5f,
                    24f
                }
            },
            {
                4,
                new float[2]
                {
                    251.5f,
                    53f
                }
            },
            {
                5,
                new float[2]
                {
                    156f,
                    85f
                }
            },
            {
                6,
                new float[2]
                {
                    271.5f,
                    61f
                }
            },
            {
                7,
                new float[2]
                {
                    347.5f,
                    31f
                }
            },
            {
                8,
                new float[2]
                {
                    354.5f,
                    -11f
                }
            },
            {
                -1,
                new float[2]
                {
                    -17.1f,
                    -10.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    31.4f,
                    -19.5f
                }
            },
            {
                -3,
                new float[2]
                {
                    454.4f,
                    -10.5f
                }
            }
        });
            tpDict.Add("Catacombs", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    241.5f,
                    -25f
                }
            },
            {
                1,
                new float[2]
                {
                    379.5f,
                    -23f
                }
            },
            {
                2,
                new float[2]
                {
                    529.5f,
                    -75f
                }
            },
            {
                3,
                new float[2]
                {
                    731.5f,
                    -75f
                }
            },
            {
                4,
                new float[2]
                {
                    499.5f,
                    -43f
                }
            },
            {
                -4,
                new float[2]
                {
                    144.5f,
                    -58.5f
                }
            },
            {
                -3,
                new float[2]
                {
                    511f,
                    -122.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    809f,
                    -74.5f
                }
            },
            {
                -1,
                new float[2]
                {
                    47.2f,
                    -10.5f
                }
            }
        });
            tpDict.Add("BambooCreek", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -28.5f,
                    -19f
                }
            },
            {
                1,
                new float[2]
                {
                    92.5f,
                    25f
                }
            },
            {
                2,
                new float[2]
                {
                    210.5f,
                    29f
                }
            },
            {
                3,
                new float[2]
                {
                    379.5f,
                    25f
                }
            },
            {
                4,
                new float[2]
                {
                    227.5f,
                    -41f
                }
            },
            {
                -3,
                new float[2]
                {
                    488.4f,
                    -90.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    -50.1f,
                    45.5f
                }
            },
            {
                -1,
                new float[2]
                {
                    -177.1f,
                    -10.5f
                }
            }
        });
            tpDict.Add("HowlingGrotto", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    26.5f,
                    -27f
                }
            },
            {
                1,
                new float[2]
                {
                    138.5f,
                    -90f
                }
            },
            {
                2,
                new float[2]
                {
                    310.5f,
                    -115f
                }
            },
            {
                3,
                new float[2]
                {
                    541.5f,
                    -123f
                }
            },
            {
                4,
                new float[2]
                {
                    439f,
                    -170f
                }
            },
            {
                -5,
                new float[2]
                {
                    241f,
                    -195.5f
                }
            },
            {
                -4,
                new float[2]
                {
                    194.9f,
                    -150.5f
                }
            },
            {
                -3,
                new float[2]
                {
                    417.9f,
                    -57.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    584.2f,
                    -122.5f
                }
            },
            {
                -1,
                new float[2]
                {
                    -46.8f,
                    -10.5f
                }
            }
        });
            tpDict.Add("QuillshroomMarsh", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    193.5f,
                    -37f
                }
            },
            {
                1,
                new float[2]
                {
                    409.5f,
                    -42f
                }
            },
            {
                2,
                new float[2]
                {
                    663.5f,
                    -27f
                }
            },
            {
                3,
                new float[2]
                {
                    916.5f,
                    -26f
                }
            },
            {
                4,
                new float[2]
                {
                    1085.5f,
                    -43f
                }
            },
            {
                5,
                new float[2]
                {
                    161.5f,
                    -54f
                }
            },
            {
                -1,
                new float[2]
                {
                    -16.8f,
                    -10.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    1160.8f,
                    -42.5f
                }
            },
            {
                -3,
                new float[2]
                {
                    590.8f,
                    -74.5f
                }
            },
            {
                -4,
                new float[2]
                {
                    841.3f,
                    -120.5f
                }
            }
        });
            tpDict.Add("SearingCrags", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    61f,
                    -27f
                }
            },
            {
                1,
                new float[2]
                {
                    147.5f,
                    69f
                }
            },
            {
                2,
                new float[2]
                {
                    226.5f,
                    151f
                }
            },
            {
                3,
                new float[2]
                {
                    282f,
                    237f
                }
            },
            {
                4,
                new float[2]
                {
                    380.5f,
                    309f
                }
            },
            {
                5,
                new float[2]
                {
                    119.5f,
                    248f
                }
            },
            {
                6,
                new float[2]
                {
                    109.5f,
                    63f
                }
            },
            {
                7,
                new float[2]
                {
                    296.5f,
                    190.5f
                }
            },
            {
                -5,
                new float[2]
                {
                    384.5f,
                    135f
                }
            },
            {
                -4,
                new float[2]
                {
                    521.8f,
                    71.5f
                }
            },
            {
                -3,
                new float[2]
                {
                    303.3f,
                    39.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    350.6f,
                    309.5f
                }
            },
            {
                -1,
                new float[2]
                {
                    17.2f,
                    -26.5f
                }
            }
        });
            tpDict.Add("GlacialPeak", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    216.5f,
                    -456f
                }
            },
            {
                1,
                new float[2]
                {
                    227.5f,
                    -405f
                }
            },
            {
                2,
                new float[2]
                {
                    259.5f,
                    -297f
                }
            },
            {
                3,
                new float[2]
                {
                    251.5f,
                    -235f
                }
            },
            {
                4,
                new float[2]
                {
                    195.5f,
                    -131f
                }
            },
            {
                5,
                new float[2]
                {
                    156.5f,
                    -27f
                }
            },
            {
                -1,
                new float[2]
                {
                    201f,
                    -518.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    181.1f,
                    -306.5f
                }
            },
            {
                -3,
                new float[2]
                {
                    220.6f,
                    66.5f
                }
            }
        });
            tpDict.Add("TowerOfTime", new SortedDictionary<int, float[]>
            {
            {
                -1,
                new float[2]
                {
                    -17.7f,
                    -10.5f
                }
            },
            {
                0,
                new float[2]
                {
                    71.5f,
                    -11f
                }
            },
            {
                1,
                new float[2]
                {
                    38.5f,
                    21.5f
                }
            },
            {
                2,
                new float[2]
                {
                    5.5f,
                    37f
                }
            },
            {
                3,
                new float[2]
                {
                    50.5f,
                    77f
                }
            },
            {
                4,
                new float[2]
                {
                    57.5f,
                    85f
                }
            },
            {
                5,
                new float[2]
                {
                    31.5f,
                    133f
                }
            },
            {
                6,
                new float[2]
                {
                    58.5f,
                    165f
                }
            },
            {
                7,
                new float[2]
                {
                    84.5f,
                    237f
                }
            }
        });
            tpDict.Add("CloudRuins", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -368.5f,
                    -26f
                }
            },
            {
                1,
                new float[2]
                {
                    -140.5f,
                    -26f
                }
            },
            {
                2,
                new float[2]
                {
                    116.5f,
                    -25f
                }
            },
            {
                3,
                new float[2]
                {
                    366.5f,
                    -27f
                }
            },
            {
                4,
                new float[2]
                {
                    721.5f,
                    -22f
                }
            },
            {
                5,
                new float[2]
                {
                    816.5f,
                    -26f
                }
            },
            {
                6,
                new float[2]
                {
                    1148.5f,
                    -27f
                }
            },
            {
                7,
                new float[2]
                {
                    -146f,
                    24f
                }
            },
            {
                8,
                new float[2]
                {
                    164f,
                    -21f
                }
            },
            {
                9,
                new float[2]
                {
                    769.5f,
                    -26f
                }
            },
            {
                10,
                new float[2]
                {
                    -251f,
                    13f
                }
            },
            {
                -1,
                new float[2]
                {
                    -486f,
                    -56.5f
                }
            }
        });
            tpDict.Add("Underworld", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -305.5f,
                    -51f
                }
            },
            {
                1,
                new float[2]
                {
                    -226.5f,
                    -89f
                }
            },
            {
                2,
                new float[2]
                {
                    -186.5f,
                    -24f
                }
            },
            {
                3,
                new float[2]
                {
                    -125.5f,
                    -91f
                }
            },
            {
                4,
                new float[2]
                {
                    0.5f,
                    72f
                }
            },
            {
                5,
                new float[2]
                {
                    124.5f,
                    -43f
                }
            },
            {
                6,
                new float[2]
                {
                    132.5f,
                    -130f
                }
            },
            {
                7,
                new float[2]
                {
                    -110.5f,
                    -98f
                }
            },
            {
                -2,
                new float[2]
                {
                    -337.5f,
                    -24.5f
                }
            },
            {
                -1,
                new float[2]
                {
                    -431.8f,
                    -56.5f
                }
            }
        });
            tpDict.Add("DarkCave", new SortedDictionary<int, float[]>
            {
            {
                -1,
                new float[2]
                {
                    -4.1f,
                    -3.5f
                }
            }
        });
            tpDict.Add("RiviereTurquoise", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    804.5f,
                    -40f
                }
            },
            {
                1,
                new float[2]
                {
                    499f,
                    -132f
                }
            },
            {
                2,
                new float[2]
                {
                    149.5f,
                    -89f
                }
            },
            {
                3,
                new float[2]
                {
                    -8.5f,
                    13f
                }
            },
            {
                4,
                new float[2]
                {
                    -259f,
                    7f
                }
            },
            {
                5,
                new float[2]
                {
                    648.5f,
                    -83f
                }
            },
            {
                6,
                new float[2]
                {
                    337.5f,
                    -131f
                }
            },
            {
                -1,
                new float[2]
                {
                    997.7f,
                    6.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    837f,
                    13.5f
                }
            }
        });
            tpDict.Add("ElementalSkylands", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -35f,
                    359f
                }
            },
            {
                1,
                new float[2]
                {
                    87.5f,
                    407f
                }
            },
            {
                2,
                new float[2]
                {
                    864.5f,
                    381f
                }
            },
            {
                3,
                new float[2]
                {
                    966.3f,
                    409.4f
                }
            },
            {
                4,
                new float[2]
                {
                    1763.5f,
                    381f
                }
            },
            {
                5,
                new float[2]
                {
                    1909f,
                    411f
                }
            },
            {
                6,
                new float[2]
                {
                    2755.5f,
                    376f
                }
            },
            {
                7,
                new float[2]
                {
                    2926.5f,
                    406f
                }
            },
            {
                8,
                new float[2]
                {
                    -22.5f,
                    417f
                }
            },
            {
                -1,
                new float[2]
                {
                    -37.4f,
                    359.5f
                }
            }
        });
            tpDict.Add("SunkenShrine", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    36.5f,
                    -41f
                }
            },
            {
                1,
                new float[2]
                {
                    186.5f,
                    -9f
                }
            },
            {
                2,
                new float[2]
                {
                    100f,
                    -81f
                }
            },
            {
                3,
                new float[2]
                {
                    166f,
                    -178f
                }
            },
            {
                4,
                new float[2]
                {
                    8f,
                    -65f
                }
            },
            {
                5,
                new float[2]
                {
                    -102.5f,
                    -121f
                }
            },
            {
                6,
                new float[2]
                {
                    53.5f,
                    -25f
                }
            },
            {
                7,
                new float[2]
                {
                    92.5f,
                    -100f
                }
            },
            {
                8,
                new float[2]
                {
                    29.5f,
                    -87f
                }
            },
            {
                -1,
                new float[2]
                {
                    -46.4f,
                    -57.5f
                }
            },
            {
                -2,
                new float[2]
                {
                    29f,
                    -54.5f
                }
            }
        });
            tpDict.Add("CorruptedFuture", new SortedDictionary<int, float[]>
            {
            {
                -1,
                new float[2]
                {
                    -100f,
                    -9.5f
                }
            }
        });
            tpDict.Add("MusicBox", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -334.5f,
                    -69f
                }
            },
            {
                1,
                new float[2]
                {
                    -333f,
                    -35f
                }
            },
            {
                2,
                new float[2]
                {
                    -266.5f,
                    -3f
                }
            },
            {
                3,
                new float[2]
                {
                    -283.5f,
                    -39f
                }
            },
            {
                4,
                new float[2]
                {
                    -135f,
                    -51f
                }
            },
            {
                5,
                new float[2]
                {
                    -29.5f,
                    -118f
                }
            },
            {
                6,
                new float[2]
                {
                    -68.5f,
                    -49f
                }
            },
            {
                7,
                new float[2]
                {
                    34.5f,
                    -26f
                }
            },
            {
                8,
                new float[2]
                {
                    -45.5f,
                    -2f
                }
            },
            {
                9,
                new float[2]
                {
                    125f,
                    40f
                }
            },
            {
                -1,
                new float[2]
                {
                    -428f,
                    -54.5f
                }
            }
        });
            tpDict.Add("Surf", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    870.75f,
                    472f
                }
            },
            {
                1,
                new float[2]
                {
                    1390.95f,
                    472f
                }
            },
            {
                2,
                new float[2]
                {
                    2465.9f,
                    472f
                }
            },
            {
                3,
                new float[2]
                {
                    4450.5f,
                    472f
                }
            },
            {
                -1,
                new float[2]
                {
                    -490.5f,
                    472.5f
                }
            }
        });
            tpDict.Add("Beach", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -353f,
                    -11f
                }
            },
            {
                1,
                new float[2]
                {
                    -228f,
                    -11f
                }
            },
            {
                2,
                new float[2]
                {
                    -173f,
                    13f
                }
            },
            {
                3,
                new float[2]
                {
                    -45f,
                    -10f
                }
            },
            {
                4,
                new float[2]
                {
                    17f,
                    13f
                }
            },
            {
                5,
                new float[2]
                {
                    257.5f,
                    29f
                }
            },
            {
                6,
                new float[2]
                {
                    422f,
                    -11f
                }
            },
            {
                7,
                new float[2]
                {
                    550f,
                    45f
                }
            },
            {
                8,
                new float[2]
                {
                    732f,
                    71f
                }
            },
            {
                9,
                new float[2]
                {
                    560.5f,
                    57f
                }
            },
            {
                -1,
                new float[2]
                {
                    -431.7f,
                    -8.5f
                }
            }
        });
            tpDict.Add("Volcano", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -419.5f,
                    9f
                }
            },
            {
                1,
                new float[2]
                {
                    -354.5f,
                    89f
                }
            },
            {
                2,
                new float[2]
                {
                    -236.5f,
                    150f
                }
            },
            {
                3,
                new float[2]
                {
                    -278.5f,
                    192f
                }
            },
            {
                4,
                new float[2]
                {
                    -319.5f,
                    265f
                }
            },
            {
                5,
                new float[2]
                {
                    -406.5f,
                    285f
                }
            },
            {
                6,
                new float[2]
                {
                    -92.5f,
                    363f
                }
            },
            {
                7,
                new float[2]
                {
                    -35.5f,
                    411f
                }
            },
            {
                8,
                new float[2]
                {
                    -193.5f,
                    413f
                }
            },
            {
                9,
                new float[2]
                {
                    -109f,
                    228f
                }
            },
            {
                -1,
                new float[2]
                {
                    -443.5f,
                    -33.5f
                }
            }
        });
            tpDict.Add("VolcanoChase", new SortedDictionary<int, float[]>
            {
            {
                0,
                new float[2]
                {
                    -67.5f,
                    22f
                }
            },
            {
                -1,
                new float[2]
                {
                    -165.5f,
                    70f
                }
            }
        });
        }
    }
}
