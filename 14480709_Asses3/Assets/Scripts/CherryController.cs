using UnityEngine;

public class CherryController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private SpriteMask levelVisibilityMask;

    [Header("Cherry Settings")]
    [SerializeField] private float spawnDelay = 5f;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float outsideOffset = 1f;

    private GameObject activeCherry;

    private Vector3 cherryStartPosition;
    private Vector3 cherryTargetPosition;

    private float moveElapsedTime;
    private float moveDuration;

    private float respawnTimer;

    private bool maskConfigured = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        respawnTimer = spawnDelay;
    }

    // Update is called once per frame
    private void Update()
    {
      if (levelGenerator == null || !levelGenerator.IsReady)
      {
          return;
      }

      if (!maskConfigured)
      {
          ConfigureVisibilityMask();
          maskConfigured = true;
      }

      if (activeCherry == null)
      {
         UpdateRespawnTimer();
         return;
      }

        UpdateCherryMovement();
    }

    private void UpdateRespawnTimer()
    {
        respawnTimer -= Time.deltaTime;

        if (respawnTimer <= 0f)
        {
            SpawnCherry();
        }
    }

    private void SpawnCherry()
    {
        Vector3 levelCenter = levelGenerator.LevelCenter;

        float tileSize = levelGenerator.TileSize;

        float halfWidth = levelGenerator.LevelWidth * 0.5f;

        float halfHeight = levelGenerator.LevelHeight * 0.5f;

        float minX = levelCenter.x - halfWidth;

        float maxX = levelCenter.x + halfWidth;

        float minY = levelCenter.y - halfHeight;

        float maxY = levelCenter.y + halfHeight;

        float offset = outsideOffset * tileSize;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0:
                cherryStartPosition = new Vector3(minX - offset, Random.Range(minY, maxY), 0f);
                break;

            case 1:
                cherryStartPosition = new Vector3(maxX + offset, Random.Range(minY, maxY), 0f);
                break;

            case 2:
                cherryStartPosition = new Vector3(Random.Range(minX, maxX), minY + offset, 0f);
                break;

            default:
                cherryStartPosition = new Vector3(Random.Range(minX, maxX), minY - offset, 0f);
                break;
        }

        Vector3 fromCenter = cherryStartPosition - levelCenter;

        cherryTargetPosition = levelCenter - fromCenter;

        activeCherry = Instantiate(cherryPrefab, cherryStartPosition, Quaternion.identity);

        float distance = Vector3.Distance(cherryStartPosition, cherryTargetPosition);

        moveDuration = distance / moveSpeed;
        moveElapsedTime = 0f;

        respawnTimer = spawnDelay;
    }

    private void UpdateCherryMovement()
    {
        moveElapsedTime += Time.deltaTime;

        float t = Mathf.Clamp01(moveElapsedTime / moveDuration);

        activeCherry.transform.position = Vector3.Lerp(cherryStartPosition, cherryTargetPosition, t);

        if (t >= 1f)
        {
            Destroy(activeCherry);
            activeCherry = null;

            respawnTimer = spawnDelay;
        }
    }

    private void ConfigureVisibilityMask()
    {
        if (levelVisibilityMask == null || levelVisibilityMask.sprite == null)
        {
            return;
        }

        Vector3 center = levelGenerator.LevelCenter;

        levelVisibilityMask.transform.position = center;

        Vector2 spriteSize = levelVisibilityMask.sprite.bounds.size;

        float width = levelGenerator.LevelWidth;

        float height = levelGenerator.LevelHeight;

        levelVisibilityMask.transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);
    }

    public void DestroyCurrentCherry()
    {
        if (activeCherry != null)
        {
            Destroy(activeCherry);
            activeCherry = null;
        }

        respawnTimer = spawnDelay;
    }
}
