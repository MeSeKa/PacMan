using System.Collections;
using UnityEngine;

public class GhostController : MonoBehaviour
{
	public float moveDelay = 0.5f; // Hareketler aras?ndaki s�re
	public Vector2 gridPosition;  // Hayaletin pozisyonu (k�suratl? olabilir)
	private Vector2 direction;    // Hareket y�n�
	public Transform pacMan;      // PacMan'in transformu
	public LayerMask wallLayer;   // Duvarlar? temsil eden Layer

	void Start()
	{
		// Hayaletin ba?lang?� pozisyonu
		gridPosition = transform.position;
		direction = Vector2.zero;
		StartCoroutine(Move());
	}

	IEnumerator Move()
	{
		while (true)
		{
			// Hareket y�n�n� se�
			ChooseDirection();

			// Yeni pozisyonu hesapla
			Vector2 newPosition = gridPosition + direction;

			// �arp??ma kontrol�
			if (!IsWallAt(newPosition))
			{
				gridPosition = newPosition;
				transform.position = gridPosition;
			}

			yield return new WaitForSeconds(moveDelay); // Bekleme s�resi
		}
	}

	void ChooseDirection()
	{
		Vector2 targetDirection = Vector2.zero;

		// PacMan'e y�nel
		if (pacMan != null)
		{
			Vector2 pacManPosition = pacMan.position;
			Vector2 delta = pacManPosition - gridPosition;

			// En k?sa ekseni se�
			if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
			{
				targetDirection = delta.x > 0 ? Vector2.right : Vector2.left;
			}
			else
			{
				targetDirection = delta.y > 0 ? Vector2.up : Vector2.down;
			}
		}

		// E?er se�ilen y�n ge�erli de?ilse rastgele y�n se�
		if (IsWallAt(gridPosition + targetDirection))
		{
			direction = GetRandomDirection();
		}
		else
		{
			direction = targetDirection;
		}
	}

	bool IsWallAt(Vector2 position)
	{
		// Pozisyonu d�nya pozisyonuna �evir
		Vector2 worldPosition = position;

		// �arp??ma kontrol�
		return Physics2D.OverlapPoint(worldPosition, wallLayer) != null;
	}

	Vector2 GetRandomDirection()
	{
		// Rastgele bir y�n se�
		Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
		return directions[Random.Range(0, directions.Length)];
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			GameManager.Instance.FinishTheGame();
		}
	}
}
