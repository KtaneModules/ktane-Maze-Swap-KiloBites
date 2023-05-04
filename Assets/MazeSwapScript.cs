using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using static UnityEngine.Debug;
using static UnityEngine.Random;

public class MazeSwapScript : MonoBehaviour
{
    private const string _directionLetters = "URDL";
    private readonly int[] _directionOffsets = new int[] { -6, 1, 6, -1 };

    public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMColorblindMode Colorblind;

	public KMSelectable[] arrowButtons;

	public TextMesh[] grid, markers, triCB;
	public TextMesh markerCB;
	public GameObject[] triangleObj;
	public MeshRenderer[] triangleRends;
	public Material[] triangles;
	public Material[] toggleFontMat;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	private bool cbActive;

	private int start, currentPos, goal;
	private int currentColor = 0;
	private bool flipped;

	private bool[][] mazeMarkers = new bool[2][];
	private string[][] selectedMazes = new string[2][];

	private static readonly Color[] goalColors = { Color.green, Color.cyan };
	private static readonly Color32[] posColors = { new Color32(64, 64, 64, 255), new Color32(165, 165, 165, 255) };
	private static readonly char[] cbColors = "GC".ToCharArray();

	string mazeCoords(int pos)
	{
		var col = pos % 6;
		var row = pos / 6;

		var coord = string.Format("{0}{1}", "ABCDEF"[col], (row + 1).ToString());

		return coord;
	}

	private bool rotation;
	private float offset;
	float speed = 1;

	void Awake()
	{

		moduleId = moduleIdCounter++;

		foreach (KMSelectable arrow in arrowButtons)
		{
			arrow.OnInteract += delegate () { arrowPress(arrow); return false; };
		}

		cbActive = Colorblind.ColorblindModeActive;

	}


	void Start()
	{
		rotation = Range(0, 2) == 1;
		flipped = Range(0, 2) == 1;
		currentColor = flipped ? 1 : 0;



		var mazeSelection = new int[2];

		mazeSelection[0] = Enumerable.Range(0, 15).PickRandom();
		mazeSelection[1] = Enumerable.Range(0, 15).Where(x => x != mazeSelection[0]).PickRandom();

		for (int i = 0; i < 2; i++)
		{
			selectedMazes[i] = Mazes.possibleMazes[mazeSelection[i]];
			mazeMarkers[i] = Mazes.mazeMarkers[mazeSelection[i]];
		}

		goal = Enumerable.Range(0, 36).Where(x => !mazeMarkers[0][x] && !mazeMarkers[1][x]).PickRandom();
		start = Enumerable.Range(0, 36).Where(x => x != goal).PickRandom();
		currentPos = start;

		var ix1 = new List<string>();
		var ix2 = new List<string>();

		for (int i = 0; i < 36; i++)
		{
			if (mazeMarkers[0][i])
			{
				ix1.Add(mazeCoords(i));
			}
			if (mazeMarkers[1][i])
			{
				ix2.Add(mazeCoords(i));
			}
		}

		Log($"[Maze Swap #{moduleId}] Green markers are at {ix1.Join(", ")}.");
		Log($"[Maze Swap #{moduleId}] Cyan markers are at {ix2.Join(", ")}.");
		Log($"[Maze Swap #{moduleId}] Your starting position is at {mazeCoords(start)} in the {new[] { "Green", "Cyan" }[currentColor]} maze.");
		Log($"[Maze Swap #{moduleId}] Your goal is at {mazeCoords(goal)}.");

		markerCB.text = cbActive ? $"GREEN: {ix1.Join(", ")}\nCYAN: {ix2.Join(", ")}" : "";

        mazeInformation();
	}

	void mazeInformation()
	{
		var ixNums = new List<int>();

		for (int i = 0; i < 36; i++)
		{
			if (mazeMarkers[0][i])
			{
				ixNums.Add(i);
			}
		}

		for (int i = 0; i < 36; i++)
		{
			triangleRends[i].material = triangles[currentColor];
			markers[i].text = mazeMarkers[0][i] || mazeMarkers[1][i] ? "O" : "";
			markers[i].GetComponentInChildren<MeshRenderer>().material = toggleFontMat[mazeMarkers[0][i] || mazeMarkers[1][i] ? 1 : 0];
			markers[i].color = goalColors[ixNums.Contains(i) ? 0 : 1];
			triangleObj[i].SetActive(goal == i ? true : false);
			grid[i].text = goal == i ? "" : "0";
			triCB[i].text = cbActive ? cbColors[currentColor].ToString() : "";
			grid[i].GetComponentInChildren<MeshRenderer>().material = toggleFontMat[currentPos == i ? 1 : 0];
			grid[i].color = posColors[currentPos == i ? 1 : 0];
		}
	}

	void tpCBUpdate()
	{
		cbActive = !cbActive;

        var ix1 = new List<string>();
        var ix2 = new List<string>();

        for (int i = 0; i < 36; i++)
        {
            if (mazeMarkers[0][i])
            {
                ix1.Add(mazeCoords(i));
            }
            if (mazeMarkers[1][i])
            {
                ix2.Add(mazeCoords(i));
            }
            triCB[i].text = cbActive ? cbColors[currentColor].ToString() : "";
        }

        markerCB.text = cbActive ? $"GREEN: {ix1.Join(", ")}\nCYAN: {ix2.Join(", ")}" : "";

    }

	void arrowPress(KMSelectable arrow)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		arrow.AddInteractionPunch(0.4f);

		if (moduleSolved)
		{
			return;
		}

		var ix = Array.IndexOf(arrowButtons, arrow);
		var markers = _directionLetters.ToCharArray();

		if (!selectedMazes[currentColor][currentPos].Contains(markers[ix]))
		{
			currentPos += _directionOffsets[ix];
			Log($"[Maze Swap #{moduleId}] Pressed {new[] { "Up", "Right", "Down", "Left" }[ix]}. New current position is: {mazeCoords(currentPos)}");
		}
		else
		{
			Log($"[Maze Swap #{moduleId}] You tried to press {new[] { "Up", "Right", "Down", "Left" }[ix]} at {mazeCoords(currentPos)}, but ended up hitting a wall. Strike!");
			Module.HandleStrike();
		}

		if (currentPos != goal)
		{
			updateMaze();
		}
		else
		{
			for (int i = 0; i < 36; i++)
			{
				grid[i].color = posColors[0];
                grid[i].GetComponentInChildren<MeshRenderer>().material = toggleFontMat[0];
            }
			Audio.PlaySoundAtTransform("Solve", transform);
			moduleSolved = true;
			Module.HandlePass();
		}
	}

	void updateMaze()
	{
		if (Range(0, 3) == 0)
		{
			flipped = !flipped;
			currentColor = flipped ? 1 : 0;
			Log($"[Maze Swap #{moduleId}] The triangle has now changed to {new[] { "Green", "Cyan" }[currentColor]}.");
		}

		for (int i = 0; i < 36; i++)
		{
			triangleRends[i].material = triangles[currentColor];
			triCB[i].text = cbActive ? cbColors[currentColor].ToString() : "";
			grid[i].GetComponentInChildren<MeshRenderer>().material = toggleFontMat[currentPos == i ? 1 : 0];
			grid[i].color = posColors[currentPos == i ? 1 : 0];
		}
	}


	void Update()
	{
		offset = Time.deltaTime * 35 * speed;

		if (!rotation)
		{
			offset *= -1;
		}

		triangleObj[goal].transform.localEulerAngles += offset * Vector3.forward;
	}

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use '!{0} UDLR' to press those arrows; "
                                                 + "command execution will be stopped if the triangle changes color. " + "|| !{0} CB to toggle colorblind mode.";
#pragma warning restore 414

    private readonly Dictionary<char, char> _reversedDirections = new Dictionary<char, char> {
        {'U', 'D'},
        {'D', 'U'},
        {'R', 'L'},
        {'L', 'R'}
    };

    private string[][] _directionMaps;
    private int[][] _depthMaps;

	private string getModuleCode()
	{
		Transform closest = null;
		float closestDistance = float.MaxValue;

		foreach (Transform children in transform.parent)
		{
			var distance = (transform.position - children.position).magnitude;
			if (children.gameObject.name == "TwitchModule(Clone)" && (closest == null || distance < closestDistance))
			{
				closest = children;
				closestDistance = distance;
			}
		}

		return closest != null ? closest.Find("MultiDeckerUI").Find("IDText").GetComponent<UnityEngine.UI.Text>().text : null;
	}

    private IEnumerator ProcessTwitchCommand(string command) {
        command = command.Trim().ToUpper();

		if ("CB".Contains(command))
		{
			tpCBUpdate();
			yield break;
		}

        if (command.Length == 0) {
            yield return "sendtochaterror That is an empty command!";
			yield break;
        }

        if (command.Any(c => !_directionLetters.Contains(c))) {
            yield return "sendtochaterror '" + command.First(c => !"UDLR".Contains(c)) + "' is not a valid direction!";
			yield break;
        }

        yield return null;
        int startMaze = currentColor;

        foreach (char direction in command) {
            if (startMaze == currentColor) {
                arrowButtons[_directionLetters.IndexOf(direction)].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
			else
			{
				yield return $"sendtochat Input has been interupted due to the triangle changing to {new[] { "Green", "Cyan" }[currentColor]} on Module {getModuleCode()} (Maze Swap).";
				yield break;
			}
        }
    }

    private IEnumerator TwitchHandleForcedSolve() {
        // Ideally would have used a 2D array; however, intial conditions imply that this not ideal.
        _directionMaps = GetDirectionMaps(selectedMazes, out _depthMaps);
        yield return null;

        while (!moduleSolved) {
            arrowButtons[_directionLetters.IndexOf(GetNextMove())].OnInteract();
            yield return new WaitForSeconds(0.2f);
        }
    }

    private string[][] GetDirectionMaps(string[][] mazes, out int[][] depths) {
        // Use DFS algorithm to find fastest route to goal from any square.
        string[][] directionMaps = new string[mazes.Length][];
        depths = new int[mazes.Length][];

        for (int i = 0; i < mazes.Length; i++) {
            directionMaps[i] = new string[36];
            depths[i] = new int[36];
            var currentDepth = new List<int>();
            var previousDepth = new List<int>();
            int depth = 1;

            directionMaps[i][goal] = "";
            depths[i][goal] = 0;
            previousDepth.Add(goal);

            while (previousDepth.Count() > 0) {
                foreach (int position in previousDepth) {
                    foreach (char direction in _directionLetters.Where(d => !mazes[i][position].Contains(d))) {
                        int newPosition = position + _directionOffsets[_directionLetters.IndexOf(direction)];

                        if (directionMaps[i][newPosition] == null) {
                            currentDepth.Add(newPosition);
                            directionMaps[i][newPosition] = _reversedDirections[direction].ToString();
                            depths[i][newPosition] = depth;
                        }
                    }
                }

                previousDepth = currentDepth.ToList();
                currentDepth.Clear();
                depth += 1;
            }
        }

        return directionMaps;
    }

    private string GetNextMove() {
        int current = flipped ? 1 : 0;
        int other = 1 - current;
        int currentDepth = _depthMaps[current][currentPos];
        int otherDepth = _depthMaps[other][currentPos];
        string currentDirection = _directionMaps[current][currentPos];
        string otherDirection = _directionMaps[other][currentPos];
        string currentWalls = selectedMazes[current][currentPos];
        string otherWalls = selectedMazes[other][currentPos];

        // Handle direction map not covering current square.
        if (currentDepth == 0) {
            if (!currentWalls.Contains(otherDirection)) {
                return otherDirection;
            }
            var sharedDirections = _directionLetters.Where(d => !(currentWalls + otherWalls).Contains(d));
            if (sharedDirections.Count() != 0) {
                return sharedDirections.First().ToString();
            }
            return _directionLetters.Where(d => !currentWalls.Contains(d)).PickRandom().ToString();
        }

        // Pick the best direction if possible.
        if (currentDepth < otherDepth || otherDepth == 0) {
            return currentDirection;
        }

        if (!currentWalls.Contains(otherDirection)) {
            return otherDirection;
        }

        // Pick random direction.
        return _directionLetters.Where(d => !currentWalls.Contains(d)).PickRandom().ToString();
    }
}

