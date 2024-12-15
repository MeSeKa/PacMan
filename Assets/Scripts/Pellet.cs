using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Pellet : MonoBehaviour
{
	public Tilemap tilemap; // Dotların bulunduğu Tilemap
	public TileBase dotTile; // Dot olarak kullanılan Tile

	void Start()
	{
		if (tilemap == null)
		{
			tilemap = GetComponent<Tilemap>();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			// PacMan'in bulunduğu pozisyonu hesapla
			Vector3 worldPosition = collision.transform.position;
			Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

			// Sadece çarpışma olan tile'ı kaldır
			if (tilemap.GetTile(cellPosition) == dotTile)
			{
				tilemap.SetTile(cellPosition, null); // Tile'ı sil
				GameManager.Instance.AddScore(10); // Puan ekle
			}
		}
	}
}
