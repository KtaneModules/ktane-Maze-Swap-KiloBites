using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class MazeSwapScript : MonoBehaviour
{

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMColorblindMode Colorblind;

	public KMSelectable[] arrowButtons;

	public TextMesh[] grid, markers, triCB;
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

		mazeSelection[0] = Enumerable.Range(0, 14).PickRandom();
		mazeSelection[1] = Enumerable.Range(0, 14).Where(x => x != mazeSelection[0]).PickRandom();

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

	void arrowPress(KMSelectable arrow)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		arrow.AddInteractionPunch(0.4f);

		if (moduleSolved)
		{
			return;
		}

		var ix = Array.IndexOf(arrowButtons, arrow);
		var dirs = new int[] { -6, 1, 6, -1 };
		var markers = "URDL".ToCharArray();

		if (!selectedMazes[currentColor][currentPos].Contains(markers[ix]))
		{
			currentPos += dirs[ix];
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
	private readonly string TwitchHelpMessage = @"Use <!{0} foobar> to do something.";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		command = command.Trim().ToUpperInvariant();
		List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
		yield return null;
	}
}


