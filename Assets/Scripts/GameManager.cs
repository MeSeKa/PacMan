using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{

	bool isPlayer = false;
	public static GameManager Instance { get; private set; }

	public GameObject GAME;

	[SerializeField] private PacManController pacmanPlayer;
	[SerializeField] private PacManAIController pacmanAI;
	[SerializeField] private Text gameOverText;
	[SerializeField] private Text scoreText;
	[SerializeField] private Text livesText;

	[SerializeField] private TextMeshProUGUI playerScore;
	[SerializeField] private TextMeshProUGUI AIScore;

	public int score { get; private set; } = 0;
	public int lives { get; private set; } = 3;

	private int ghostMultiplier = 1;

	private void Awake()
	{
		if (Instance != null)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			Instance = this;
		}

		playerScore.text = PlayerPrefs.GetInt("PlayerScore").ToString();
		AIScore.text = PlayerPrefs.GetInt("AIScore").ToString();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	internal void AddScore(int v)
	{
		score += v;
		scoreText.text = score + " ";
	}

	public void SetPlayer()
	{
		isPlayer = true;
		pacmanAI.enabled = false;
		pacmanPlayer.enabled = true;
		GAME.SetActive(true);
	}

	public void SetAI()
	{
		isPlayer = false;
		pacmanAI.enabled = true;
		pacmanPlayer.enabled = false;
		GAME.SetActive(true);
	}

	public void StopGame()
	{
		GAME.SetActive(false);
	}

	public void Restart()
	{
		SceneManager.LoadScene(0);
	}

	public void FinishTheGame()
	{
		if (!isPlayer) { PlayerPrefs.SetInt("AIScore", score); }
		else PlayerPrefs.SetInt("PlayerScore", score);

		Restart();
	}
}