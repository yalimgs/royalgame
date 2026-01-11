using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject[] tilePrefabs;
    public Tile[,] allTiles;
    
    private Tile firstSelectedTile;
    private Color previousColor; // <-- YENİ: İlk rengi hafızada tutmak için değişken

    void Start()
    {
        allTiles = new Tile[width, height];
        GenerateBoard();
    }

    void GenerateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnNewTile(x, y);
            }
        }
    }

    void SpawnNewTile(int x, int y)
    {
        Vector2 spawnPos = new Vector2(x, y + height); 

        int randomIndex = Random.Range(0, tilePrefabs.Length);
        GameObject newTileObj = Instantiate(tilePrefabs[randomIndex], spawnPos, Quaternion.identity);
        
        newTileObj.name = "Tile (" + x + "," + y + ")";
        newTileObj.transform.parent = this.transform;

        Tile tileScript = newTileObj.GetComponent<Tile>();
        tileScript.Setup(x, y, this, tilePrefabs[randomIndex].name);
        
        allTiles[x, y] = tileScript;
    }

    // --- BURASI GÜNCELLENDİ ---
    public void TileClicked(Tile clickedTile)
    {
        // 1. TIKLAMA (İlk seçim)
        if (firstSelectedTile == null)
        {
            firstSelectedTile = clickedTile;
            
            // Seçilen karenin ŞU ANKİ rengini sakla (Kırmızıysa kırmızıyı saklar)
            previousColor = firstSelectedTile.GetComponent<SpriteRenderer>().color;
            
            // Seçildiğini belli etmek için rengi biraz koyulaştır (GRİ)
            firstSelectedTile.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        // 2. TIKLAMA (İkinci seçim veya iptal)
        else
        {
            // İlk seçilen kareyi SAKLADIĞIMIZ RENGE geri döndür (Color.white değil!)
            firstSelectedTile.GetComponent<SpriteRenderer>().color = previousColor;

            // Eğer aynı kareye tekrar tıkladıysan seçimi iptal et ve çık
            if (firstSelectedTile == clickedTile)
            {
                firstSelectedTile = null;
                return;
            }

            // Mesafe ve Tür kontrolü
            float distance = Vector2.Distance(new Vector2(firstSelectedTile.x, firstSelectedTile.y), new Vector2(clickedTile.x, clickedTile.y));

            // Eğer yan yanaysa VE türleri farklıysa yer değiştir
            if (distance < 1.1f && firstSelectedTile.type != clickedTile.type) 
            {
                SwapTiles(firstSelectedTile, clickedTile);
            }
            
            // Seçimi temizle
            firstSelectedTile = null;
        }
    }

    void SwapTiles(Tile tileA, Tile tileB)
    {
        Tile tempScript = allTiles[tileA.x, tileA.y];
        allTiles[tileA.x, tileA.y] = allTiles[tileB.x, tileB.y];
        allTiles[tileB.x, tileB.y] = tempScript;

        int tempX = tileA.x;
        int tempY = tileA.y;
        tileA.x = tileB.x;
        tileA.y = tileB.y;
        tileB.x = tempX;
        tileB.y = tempY;

        StartCoroutine(FindMatchesRoutine());
    }

    IEnumerator FindMatchesRoutine()
    {
        yield return new WaitForSeconds(0.4f);

        List<Tile> matchesToDestroy = FindMatches();

        if (matchesToDestroy.Count > 0)
        {
            foreach (Tile tile in matchesToDestroy)
            {
                DestroyTile(tile);
            }

            yield return new WaitForSeconds(0.4f); 
            
            RefillBoard();
            
            StartCoroutine(FindMatchesRoutine());
        }
    }

    List<Tile> FindMatches()
    {
        HashSet<Tile> matchedTiles = new HashSet<Tile>();

        // YATAY
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile currentTile = allTiles[x, y];
                if (currentTile != null)
                {
                    if (x < width - 2)
                    {
                        Tile right1 = allTiles[x + 1, y];
                        Tile right2 = allTiles[x + 2, y];

                        if (right1 != null && right2 != null)
                        {
                            if (currentTile.type == right1.type && currentTile.type == right2.type)
                            {
                                matchedTiles.Add(currentTile);
                                matchedTiles.Add(right1);
                                matchedTiles.Add(right2);
                            }
                        }
                    }
                }
            }
        }

        // DİKEY
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile currentTile = allTiles[x, y];
                if (currentTile != null)
                {
                    if (y < height - 2)
                    {
                        Tile up1 = allTiles[x, y + 1];
                        Tile up2 = allTiles[x, y + 2];

                        if (up1 != null && up2 != null)
                        {
                            if (currentTile.type == up1.type && currentTile.type == up2.type)
                            {
                                matchedTiles.Add(currentTile);
                                matchedTiles.Add(up1);
                                matchedTiles.Add(up2);
                            }
                        }
                    }
                }
            }
        }

        return new List<Tile>(matchedTiles);
    }

    void DestroyTile(Tile tile)
    {
        if (tile != null)
        {
            Destroy(tile.gameObject);
            allTiles[tile.x, tile.y] = null; 
        }
    }

    void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            List<Tile> validTiles = new List<Tile>();
            for (int y = 0; y < height; y++)
            {
                if (allTiles[x, y] != null)
                {
                    validTiles.Add(allTiles[x, y]);
                }
            }

            for (int y = 0; y < height; y++)
            {
                if (y < validTiles.Count)
                {
                    Tile tile = validTiles[y];
                    tile.x = x;
                    tile.y = y;
                    allTiles[x, y] = tile;
                }
                else
                {
                    SpawnNewTile(x, y);
                }
            }
        }
    }
}