using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacManAIController : MonoBehaviour
{
	public float moveDelay = 0.5f;         // Hareketler arasındaki süre
	public Tilemap pelletTilemap;         // Pellet'lerin bulunduğu Tilemap
	public TileBase pelletTile;           // Pellet için kullanılan Tile
	public LayerMask ghostLayer;          // Hayaletlerin Layer
	public LayerMask wallLayer;           // Duvarların Layer

	private Vector2 position;             // PacMan'in pozisyonu
	private Vector2 direction;            // Hareket yönü
	private Vector2 previousDirection;    // Bir önceki yön
	private Queue<Vector2> recentPositions; // Son ziyaret edilen pozisyonlar (sonsuz döngüyü engellemek için)

	void Start()
	{
		position = transform.position;    // PacMan'in başlangıç pozisyonu
		direction = Vector2.zero;         // İlk başta hareket etmiyor
		previousDirection = Vector2.zero; // Önceki yön başlangıçta boş
		recentPositions = new Queue<Vector2>(); // Hareket geçmişi için kuyruk başlat
		StartCoroutine(AIPlay());         // AI'yı başlat
	}

	IEnumerator AIPlay()
	{
		while (true)
		{
			// Hayalet çarpışması kontrolü
			if (CheckGhostCollision())
			{
				GameManager.Instance.FinishTheGame();
				yield break; // Oyun bitti, döngüyü sonlandır
			}

			// Tüm pellet'ler toplandıysa
			if (AreAllPelletsCollected())
			{
				GameManager.Instance.FinishTheGame();
				yield break; // Oyun bitti, döngüyü sonlandır
			}

			direction = GetBestDirection();
			Vector2 newPosition = position + direction;
			Turn();

			if (!IsWallAt(newPosition))
			{
				// Sonsuz döngüyü engellemek için geçmişi kontrol et
				if (IsInRecentPositions(newPosition))
				{
					direction = GetRandomSafeDirection(); // Alternatif yön seç
					newPosition = position + direction;
				}

				// Yeni pozisyona ilerle
				AddToRecentPositions(position); // Geçmişe mevcut pozisyonu ekle
				previousDirection = direction; // Önceki yönü güncelle
				position = newPosition;
				transform.position = position;

				// Pellet'i topla
				CollectPellet(position);
			}

			yield return new WaitForSeconds(moveDelay); // Hareketler arasındaki süre
		}
	}

	void Turn()
	{
		print(direction);
		if (direction.x < 0) transform.localRotation = Quaternion.Euler(0, 0, 180);
		if (direction.x > 0) transform.localRotation = Quaternion.Euler(0, 0, 0);
		if (direction.y > 0) transform.localRotation = Quaternion.Euler(0, 0, 90);
		if (direction.y < 0) transform.localRotation = Quaternion.Euler(0, 0, 270);
	}

	Vector2 GetBestDirection()
	{
		Vector3Int closestPellet = FindClosestPellet();

		if (closestPellet == Vector3Int.zero)
		{
			// Eğer pellet kalmadıysa rastgele bir yön seç
			return GetRandomSafeDirection();
		}

		return CalculatePathTo(closestPellet);
	}

	Vector2 CalculatePathTo(Vector3Int target)
	{
		Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
		Vector2 bestDirection = Vector2.zero;
		float shortestDistance = float.MaxValue;

		foreach (Vector2 dir in directions)
		{
			Vector2 newPosition = position + dir;

			// Güvenli ve önceki hareketin tersi olmayan yönler
			if (!IsWallAt(newPosition) && dir != -previousDirection && !IsGhostNear(newPosition))
			{
				float distance = Vector2.Distance(newPosition, pelletTilemap.CellToWorld(target));
				if (distance < shortestDistance)
				{
					shortestDistance = distance;
					bestDirection = dir;
				}
			}
		}

		// Eğer en iyi yön bulunamazsa, rastgele bir güvenli yön seç
		if (bestDirection == Vector2.zero)
		{
			bestDirection = GetRandomSafeDirection();
		}

		return bestDirection;
	}

	Vector3Int FindClosestPellet()
	{
		BoundsInt bounds = pelletTilemap.cellBounds;
		Vector3Int closestPellet = Vector3Int.zero;
		float closestDistance = float.MaxValue;

		foreach (Vector3Int position in GetAllPelletPositions(bounds))
		{
			Vector3 worldPosition = pelletTilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0);
			float distance = Vector2.Distance(this.position, worldPosition);

			if (distance < closestDistance && !IsGhostNear(worldPosition))
			{
				closestDistance = distance;
				closestPellet = position;
			}
		}

		return closestPellet;
	}

	List<Vector3Int> GetAllPelletPositions(BoundsInt bounds)
	{
		List<Vector3Int> positions = new List<Vector3Int>();

		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				Vector3Int cellPosition = new Vector3Int(x, y, 0);
				if (pelletTilemap.GetTile(cellPosition) == pelletTile)
				{
					positions.Add(cellPosition);
				}
			}
		}

		return positions;
	}

	void AddToRecentPositions(Vector2 pos)
	{
		if (recentPositions.Count >= 5) // Kuyruk boyutu: Son 5 pozisyon
		{
			recentPositions.Dequeue();
		}
		recentPositions.Enqueue(pos);
	}

	bool IsInRecentPositions(Vector2 pos)
	{
		foreach (Vector2 recent in recentPositions)
		{
			if (Vector2.Distance(recent, pos) < 0.1f) // Yaklaşık eşitlik kontrolü
			{
				return true;
			}
		}
		return false;
	}

	Vector2 GetRandomSafeDirection()
	{
		Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
		foreach (Vector2 dir in directions)
		{
			Vector2 newPosition = position + dir;
			if (!IsWallAt(newPosition) && !IsGhostNear(newPosition))
			{
				return dir;
			}
		}

		return Vector2.zero; // Eğer güvenli bir yön bulunamazsa sabit kal
	}

	bool IsWallAt(Vector2 position)
	{
		return Physics2D.OverlapPoint(position, wallLayer) != null;
	}

	bool IsGhostNear(Vector2 position)
	{
		Collider2D[] ghosts = Physics2D.OverlapCircleAll(position, 1.5f, ghostLayer); // 1.5 birimlik çember
		return ghosts.Length > 0;
	}

	void CollectPellet(Vector2 position)
	{
		Vector3Int cellPosition = pelletTilemap.WorldToCell(position);
		if (pelletTilemap.GetTile(cellPosition) == pelletTile)
		{
			pelletTilemap.SetTile(cellPosition, null); // Pellet'i kaldır
			GameManager.Instance.AddScore(10);
		}
	}

	bool AreAllPelletsCollected()
	{
		BoundsInt bounds = pelletTilemap.cellBounds;

		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				Vector3Int cellPosition = new Vector3Int(x, y, 0);
				if (pelletTilemap.GetTile(cellPosition) == pelletTile)
				{
					return false; // Hâlâ pellet var
				}
			}
		}

		return true; // Tüm pellet'ler toplandı
	}

	bool CheckGhostCollision()
	{
		Collider2D[] ghosts = Physics2D.OverlapCircleAll(position, 0.5f, ghostLayer);
		return ghosts.Length > 0;
	}

}
