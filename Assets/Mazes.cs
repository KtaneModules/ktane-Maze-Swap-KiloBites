using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

    public static class Mazes
    {
        public static readonly bool[][] mazeMarkers = new string[]
        {
            "......x..........x..................",
            "........x............x..............",
            ".........x........................x.",
            ".....x.....................x........",
            "xx..................................",
            "...........x.......x................",
            "..................x................x",
            "...x...x............................",
            "...............x......x.............",
            ".........................x...x......",
            "........................x......x....",
            "....x.....................x.........",
            "............x.......x...............",
            "..............x..................x.."
        }.Select(maze => maze.Select(marker => marker == 'x').ToArray()).ToArray();

        public static readonly string[][] possibleMazes =
        {
            new string[36] { "UL", "UD", "UR", "UL", "UD", "URD", "RL", "UL", "RD", "DL", "UD", "UR", "RL", "DL", "URD", "UDL", "UD", "R", "RL", "UDL", "UD", "UD", "UD", "R", "L", "UD", "UR", "UL", "URD", "RL", "DL", "URD", "DL", "RD", "UDL", "RD" },
            new string[36] { "UL", "UD", "UD", "UR", "UL", "UR", "DL", "U", "UR", "RL", "RL", "RL", "URL", "RL", "RL", "RDL", "RDL", "RL", "RL", "RL", "L", "URD", "UL", "RD", "L", "RD", "DL", "UD", "DR", "URL", "DL", "UD", "UD", "UD", "UD", "RD" },
            new string[36] { "UL", "UD", "UR", "UL", "UD", "UR", "L", "URD", "DL", "R", "UDL", "R", "DL", "UD", "URD", "RL", "UL", "RD", "URL", "UD", "UD", "RD", "DL", "UR", "RL", "UDL", "U", "UD", "URD", "RL", "DL", "UD", "D", "UD", "UD", "RD" },
            new string[36] { "UDL", "UD", "U", "UR", "UL", "UR", "UDL", "UD", "RD", "DL", "R", "RL", "UL", "U", "UD", "UD", "", "R", "RL", "L", "U", "UD", "RD", "RL", "RL", "RL", "L", "UD", "URD", "RL", "RDL", "RDL", "RDL", "UDL", "UD", "RD" },
            new string[36] { "UDL", "UD", "UD", "UR", "UDL", "UR", "UL", "U", "UD", "D", "UD", "RD", "RDL", "L", "U", "U", "UD", "UR", "UDL", "R", "RL", "RL", "UDL", "RD", "UDL", "RD", "RDL", "DL", "UD", "UR", "UDL", "UD", "UD", "UD", "UD", "RD" },
            new string[36] { "UDL", "UD", "U", "UD", "UR", "URL", "UDL", "UD", "R", "URL", "DL", "R", "UL", "U", "D", "RD", "URL", "RL", "RL", "L", "UD", "UR", "DL", "R", "RL", "RDL", "UL", "RD", "UL", "RD", "DL", "URD", "DL", "URD", "DL", "URD" },
            new string[36] { "UDL", "UD", "UD", "UD", "U", "URD", "UDL", "U", "UD", "U", "D", "URD", "UL", "RD", "UL", "D", "UD", "URD", "L", "UR", "DL", "U", "UD", "URD", "RL", "RL", "UDL", "D", "U", "URD", "RDL", "RDL", "UDL", "UD", "D", "URD" },
            new string[36] { "UDL", "UD", "U", "U", "UD", "URD", "UDL", "UD", "RD", "DL", "UR", "URL", "UDL", "UD", "U", "UR", "RL", "RL", "UL", "UD", "R", "L", "D", "R", "RL", "UL", "R", "L", "URD", "RL", "RDL", "RDL", "RDL", "DL", "UD", "RD" },
            new string[36] { "UL", "URD", "UL", "URD", "UL", "UR", "DL", "UR", "L", "UD", "RD", "RDL", "UDL", "R", "L", "UR", "UL", "URD", "UDL", "D", "RD", "L", "D", "URD", "UDL", "U", "U", "D", "UD", "UR", "UDL", "RD", "DL", "URD", "UDL", "RD" },
            new string[36] { "UDL", "UR", "URL", "URL", "UDL", "UR", "UL", "D", "R", "L", "URD", "RL", "RDL", "UDL", "", "", "UD", "RD", "UL", "UD", "R", "L", "UR", "URL", "RL", "UDL", "RD", "RL", "RL", "RL", "DL", "UD", "URD", "RDL", "DL", "RD" },
            new string[36] { "UDL", "U", "UD", "UD", "UD", "URD", "UDL", "", "UR", "UL", "U", "UR", "URL", "RL", "L", "RD", "RDL", "RL", "DL", "R", "RL", "UDL", "UD", "R", "UDL", "RD", "RDL", "UDL", "UR", "RL", "UDL", "UD", "UD", "UD", "D", "RD" },
            new string[36] { "UDL", "UD", "UD", "UD", "UR", "URL", "UL", "U", "UD", "UR", "RL", "RL", "RL", "L", "URD", "DL", "R", "RL", "RL", "DL", "URD", "URL", "RL", "RL", "RL", "UL", "UD", "D", "RD", "RL", "RDL", "DL", "UD", "UD", "UD", "RD" },
            new string[36] { "URL", "URL", "URL", "URL", "URL", "URL", "RL", "RL", "L", "R", "RL", "RL", "RL", "RL", "RL", "RL", "RL", "RL", "RL", "L", "", "", "R", "RL", "RL", "RL", "RL", "RDL", "RL", "RL", "DL", "RD", "DL", "URD", "DL", "RD" },
            new string[36] { "UDL", "UD", "UR", "UL", "UD", "UR", "UL", "UD", "RD", "DL", "URD", "RL", "DL", "UD", "UD", "U", "UD", "RD", "UDL", "UD", "U", "", "UD", "URD", "UL", "U", "R", "RL", "URL", "URL", "RDL", "RDL", "RDL", "DL", "D", "RD" },
            new string[36] { "URL", "UDL", "U", "U", "UD", "UR", "RL", "UDL", "RD", "L", "UD", "R", "L", "UD", "UD", "", "U", "R", "L", "UD", "UR", "RL", "RDL", "RL", "L", "UD", "R", "DL", "UD", "R", "DL", "UD", "D", "UD", "URD", "RDL" }
        };
    }