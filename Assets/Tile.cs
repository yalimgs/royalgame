using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public BoardManager board;
    public string type; // YENİ: Benim rengim ne? (Örn: "BlueTile")

    // Setup güncellendi: Artık type (tür) bilgisini de alıyor
    public void Setup(int tempX, int tempY, BoardManager tempBoard, string tempType)
    {
        x = tempX;
        y = tempY;
        board = tempBoard;
        type = tempType;
    }

    void Update()
    {
        Vector2 targetPosition = new Vector2(x, y);
        transform.position = Vector2.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (GetComponent<Collider2D>() == Physics2D.OverlapPoint(mousePos))
            {
                board.TileClicked(this);
            }
        }
    }
}