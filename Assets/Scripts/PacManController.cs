using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class PacManController : MonoBehaviour
{
	public float moveDelay = 0.5f; // Her hareket arasındaki süre
	public Vector2 gridPosition; // PacMan'in bulunduğu grid pozisyonu
	private Vector2Int direction; // Hareket yönü
	private bool canMove = true;

	public Tilemap pelletTilemap;         // Pellet'lerin bulunduğu Tilemap
	public TileBase pelletTile;           // Pellet için kullanılan Tile
	public LayerMask ghostLayer;          // Hayaletlerin Layer

	public LayerMask wallLayer;
	void Start()
	{
		// Başlangıç pozisyonunu al
		direction = Vector2Int.zero; // Hareket etmiyor
		gridPosition = transform.position;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2Int.up;
		if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2Int.down;
		if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2Int.left;
		if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2Int.right;

		if (canMove && direction != Vector2Int.zero)
		{
			StartCoroutine(Move());
		}
	}

	IEnumerator Move()
	{
		canMove = false;


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

		// Yeni pozisyonu hesapla
		Vector2 newPosition = gridPosition + direction;
		Turn();
		// Çarpışma kontrolü
		if (!IsWallAt(newPosition))
		{
			gridPosition = newPosition;
			transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
		}

		yield return new WaitForSeconds(moveDelay);
		canMove = true;
	}
	void Turn()
	{
		if (direction.x < 0) transform.localRotation = Quaternion.Euler(0, 0, 180);
		if (direction.x > 0) transform.localRotation = Quaternion.Euler(0, 0, 0);
		if (direction.y > 0) transform.localRotation = Quaternion.Euler(0, 0, 90);
		if (direction.y < 0) transform.localRotation = Quaternion.Euler(0, 0, 270);
	}

	bool IsWallAt(Vector2 position)
	{
		// Dünya pozisyonunu al
		Vector3 worldPosition = new Vector3(position.x, position.y, 0);

		// Çarpışmayı kontrol et
		return Physics2D.OverlapPoint(worldPosition, wallLayer) != null;
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
		Collider2D[] ghosts = Physics2D.OverlapCircleAll(gridPosition, 0.5f, ghostLayer);
		return ghosts.Length > 0;
	}
}

