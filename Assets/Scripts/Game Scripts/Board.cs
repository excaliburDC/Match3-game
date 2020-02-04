using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

namespace Match3

{
    [System.Serializable]
    public class ExtraTiles
    {
        public int x;
        public int y;
        public int z;
       
    }

    public enum GameState
    {
        WAIT,
        MOVE
    }

    public class Board : MonoBehaviour
    {
        public GameState currentState=GameState.MOVE;

        [SerializeField]
        private int _boardWidth;
        [SerializeField]
        private int _boardHeight;
        [SerializeField]
        private float _borderSize;
        [SerializeField]
        private int offset;
        [SerializeField]
        private GameObject _tileNormalPrefab;
        [SerializeField]
        private GameObject _tileObstaclePrefab;
        [SerializeField]
        private GameObject[] _gamePiecePrefabs;
        [SerializeField]
        private GamePiece[,] _gamePieceArray;
        [SerializeField]
        private ExtraTiles[] _extraTiles;

        private Tile[,] _tileArray;

        private Tile _clickedTile;
        private Tile _targetTile;

        private bool _isPlayerInputActive = true;
        private bool boardInitialized;

       public int noOfExtratile;
      

        private void Start()
        {
            _tileArray = new Tile[_boardWidth, _boardHeight];
            _gamePieceArray = new GamePiece[_boardWidth, _boardHeight];
            TileSetup();
            CameraSetup();
            FillBoardGamePiece();

        }
        public void TileSetup()
        {
            foreach (ExtraTiles tiles in _extraTiles)
            {
                if (tiles != null)
                {
                    MakeTile(_tileObstaclePrefab, tiles.x, tiles.y, tiles.z);
                }
            }

            for (int i = 0; i < _boardWidth; i++)
            {
                for (int j = 0; j < _boardHeight; j++)
                {
                    if (_tileNormalPrefab != null && _tileArray[i, j] == null)
                    {
                        MakeTile(_tileNormalPrefab, i, j);
                    }

                }
            }
        }

        private void MakeTile(GameObject prefab, int x, int y, int z = 0)
        {
            if (prefab != null)
            {
                GameObject tileInstance = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                tileInstance.name = "Tile [" + x + " , " + y + "]";
                _tileArray[x, y] = tileInstance.GetComponent<Tile>();
                tileInstance.transform.parent = this.transform;
                _tileArray[x, y].Init(x, y, this);
            }
        }

        public void CameraSetup()
        {
            Camera.main.transform.position = new Vector3((float)(_boardWidth - 1) / 2, (float)(_boardHeight - 1) / 2, -10);
            float aspectRatio = (float)Screen.width / (float)Screen.height;
            float verticalOrthoSize = (float)_boardHeight / 2 + _borderSize;
            float horizontalOrthoSize = ((float)_boardWidth / 2 + _borderSize) / aspectRatio;
            Camera.main.orthographicSize = (verticalOrthoSize > horizontalOrthoSize) ? verticalOrthoSize : horizontalOrthoSize;
        }

        private void FillBoardGamePiece(int yOffset = 0, float timeToMove = 0.1f)
        {
            int loopCount = 0;
            for (int i = 0; i < _boardWidth; i++)
            {
                for (int j = 0; j < _boardHeight; j++)
                {
                    if (_gamePieceArray[i, j] == null && _tileArray[i, j].tileType != TileType.Obstacle)
                    {
                        FillRandomAt(i, j, yOffset, timeToMove);
                        
                        loopCount = 0;
                        while (HasMatchOnFill(i, j))
                        {
                            ClearPiecesAt(i, j);
                            ScoreManager.sInstance.IncrementScore(30);
                            Debug.Log(30);
                            FillRandomAt(i, j, yOffset, timeToMove);
                            loopCount++;
                            if (loopCount > 50)
                                break;
                        }
                    }
                }
            }
        }
        private void FillRandomAt(int x, int y, int yOffset = 0, float timeToMove = 0.1f)
        {
            GameObject randGamePiece = Instantiate(GetRandomGamePiece(),Vector3.zero, Quaternion.identity) as GameObject;
            if (randGamePiece != null)
            {
                randGamePiece.transform.parent = this.transform;
                randGamePiece.GetComponent<GamePiece>().Init(this);
                //PlaceGamePiece(randGamePiece.GetComponent<GamePiece>(), x, y);
                if (yOffset != 0)
                {
                    PlaceGamePiece(randGamePiece.GetComponent<GamePiece>(), x, y + yOffset);
                    randGamePiece.GetComponent<GamePiece>().Move(x, y, timeToMove);
                }
                else
                {
                    boardInitialized = true;
                    PlaceGamePiece(randGamePiece.GetComponent<GamePiece>(), x, y);
                }
                   
            }
            
        }
        private GameObject GetRandomGamePiece()
        {
            int index = Random.Range(0, _gamePiecePrefabs.Length);
            if (_gamePiecePrefabs[index] == null)
                Debug.Log("BOARD[GetRandomGamePiece]: " + index + " does not contain a valid game piece");

            return (_gamePiecePrefabs[index]);
        }

        public void PlaceGamePiece(GamePiece gamePiece, int xPos, int yPos)
        {
            if (gamePiece == null)
            {
                Debug.Log("BOARD[PlaceGamePiece]: does not contain a valid game piece");
                return;
            }

            if(boardInitialized)
            {
                gamePiece.transform.position = new Vector3(xPos, yPos+offset, 0);
                gamePiece.transform.rotation = Quaternion.identity;
                gamePiece.transform.DOMove(new Vector3(xPos, yPos, 0), 0.5f);
                
                boardInitialized = false;
            }
            else
            {
                gamePiece.transform.position = new Vector3(xPos, yPos, 0);
                gamePiece.transform.rotation = Quaternion.identity;
            }
           
            if (IsValidCoordinate(xPos, yPos))
            {
                gamePiece.SetCoord(xPos, yPos);

                _gamePieceArray[xPos, yPos] = gamePiece;
            }

        }

        private bool HasMatchOnFill(int x, int y, int minLength = 3)
        {
            List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);
            List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);

            if (downwardMatches == null)
                downwardMatches = new List<GamePiece>();
            if (leftMatches == null)
                leftMatches = new List<GamePiece>();

            return ((downwardMatches.Count > 0) || leftMatches.Count > 0);
        }
        private List<GamePiece> FindMatches(int xStart, int yStart, Vector2 searchDirection, int minLength = 3)
        {
            List<GamePiece> matchingPieces = new List<GamePiece>();
            GamePiece startPiece = null;

            if (IsValidCoordinate(xStart, yStart))
                startPiece = _gamePieceArray[xStart, yStart];

            if (startPiece != null)
                matchingPieces.Add(startPiece);
            else
                return null;

            int nextX;
            int nextY;

            int maxValue = (_boardWidth > _boardHeight) ? _boardWidth : _boardHeight;

            for (int i = 1; i < maxValue - 1; i++)
            {
                nextX = xStart + (Mathf.Clamp((int)searchDirection.x, -1, 1) * i);
                nextY = yStart + (Mathf.Clamp((int)searchDirection.y, -1, 1) * i);

                if (!IsValidCoordinate(nextX, nextY))
                    break;

                GamePiece nextPiece = _gamePieceArray[nextX, nextY];

                if (nextPiece == null)
                    break;
                else
                {
                    if (startPiece.currentMatchType == nextPiece.currentMatchType && !matchingPieces.Contains(nextPiece))
                        matchingPieces.Add(nextPiece);
                    else
                        break;
                }


            }

            return ((matchingPieces.Count >= minLength) ? matchingPieces : null);
        }

        private bool IsValidCoordinate(int x, int y)
        {
            return (x >= 0 && x < _boardWidth && y >= 0 && y < _boardHeight);
        }


        public void ClickTile(Tile tile)
        {
            if (_clickedTile == null)
            {
                _clickedTile = tile;
                //Debug.Log("ClickTile: " + _clickedTile.name);
            }
        }

        public void DragToTile(Tile tile)
        {
            if (_clickedTile != null)
            {
                _targetTile = tile;
                //Debug.Log("TargetTile: " + _targetTile.name);
            }
        }

        public void ReleaseTile()
        {
            if (_clickedTile != null && _targetTile != null && IsNextTo(_clickedTile, _targetTile))
            {
                SwitchTile(_clickedTile, _targetTile);
            }
            _clickedTile = null;
            _targetTile = null;
            //Debug.Log("ReleaseTile ..... !!");
        }

        private bool IsNextTo(Tile clickedTile, Tile selectedTile)
        {
            return ((Mathf.Abs(clickedTile.XIndex - selectedTile.XIndex) == 1 && clickedTile.YIndex == selectedTile.YIndex) ||
            (Mathf.Abs(clickedTile.YIndex - selectedTile.YIndex) == 1 && clickedTile.XIndex == selectedTile.XIndex));
        }
        private void SwitchTile(Tile clickedTile, Tile targetTile)
        {
            if (_isPlayerInputActive)
                StartCoroutine(ISwitchTile(clickedTile, targetTile));
        }

        private IEnumerator ISwitchTile(Tile clickedTile, Tile targetTile)
        {
            GamePiece clickedPiece = _gamePieceArray[clickedTile.XIndex, clickedTile.YIndex];
            GamePiece targetPiece = _gamePieceArray[targetTile.XIndex, targetTile.YIndex];
            clickedPiece.Move(targetPiece.XIndex, targetTile.YIndex, 0.5f);
            targetPiece.Move(clickedTile.XIndex, clickedTile.YIndex, 0.5f);
            yield return new WaitForSeconds(0.5f);
            List<GamePiece> clickedMatch = FindMatchesAt(clickedTile.XIndex, clickedTile.YIndex);
            List<GamePiece> targetMatch = FindMatchesAt(targetTile.XIndex, targetTile.YIndex);
            if (clickedMatch == null)
                clickedMatch = new List<GamePiece>();
            if (targetMatch == null)
                targetMatch = new List<GamePiece>();

            if (clickedMatch.Count == 0 && targetMatch.Count == 0)
            {
                clickedPiece.Move(clickedTile.XIndex, clickedTile.YIndex, 0.5f);
                targetPiece.Move(targetTile.XIndex, targetTile.YIndex, 0.5f);
                currentState = GameState.MOVE;
            }
            else
            {
                //HiglightClearCollapseAndRefill(clickedMatch.Union(targetMatch).ToList());
                StartCoroutine(IHiglightClearCollapseAndRefill(clickedMatch.Union(targetMatch).ToList()));
            }
        }
        private List<GamePiece> FindMatchesAt(int i, int j, int minLength = 3)
        {
            List<GamePiece> horizMatches = FindHorizontalMatches(i, j, minLength);
            List<GamePiece> vertiMatches = FindVecticalMatches(i, j, minLength);

            if (horizMatches == null)
                horizMatches = new List<GamePiece>();
            if (vertiMatches == null)
                vertiMatches = new List<GamePiece>();

            List<GamePiece> combinedMatches = horizMatches.Union(vertiMatches).ToList();
            return combinedMatches;
        }

        private List<GamePiece> FindVecticalMatches(int xStart, int yStart, int minLength = 3)
        {
            List<GamePiece> upwardMatches = FindMatches(xStart, yStart, new Vector2(0, 1), 2);
            List<GamePiece> downwardMatches = FindMatches(xStart, yStart, new Vector2(0, -1), 2);

            if (upwardMatches == null)
                upwardMatches = new List<GamePiece>();
            if (downwardMatches == null)
                downwardMatches = new List<GamePiece>();

            List<GamePiece> combinedMatches = upwardMatches.Union(downwardMatches).ToList();

            return (combinedMatches.Count >= minLength ? combinedMatches : null);
        }

        private List<GamePiece> FindHorizontalMatches(int xStart, int yStart, int minLength = 3)
        {
            List<GamePiece> rightMatches = FindMatches(xStart, yStart, new Vector2(1, 0), 2);
            List<GamePiece> leftMatches = FindMatches(xStart, yStart, new Vector2(-1, 0), 2);

            if (rightMatches == null)
                rightMatches = new List<GamePiece>();
            if (leftMatches == null)
                leftMatches = new List<GamePiece>();

            List<GamePiece> combinedMatches = rightMatches.Union(leftMatches).ToList();

            return (combinedMatches.Count >= minLength ? combinedMatches : null);
        }
        private IEnumerator IHiglightClearCollapseAndRefill(List<GamePiece> gamePieces)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            List<GamePiece> matches = new List<GamePiece>();
            _isPlayerInputActive = false;
            HighlightOnMatchesAt(gamePieces);
            yield return new WaitForSeconds(0.2f);
            bool isFinshed = false;
            while (!isFinshed)
            {
                HighlightOffMatchesAt(gamePieces);
                ClearPiecesAt(gamePieces);
               // Debug.Log(30);
                ScoreManager.sInstance.IncrementScore(30);
                BreakTileAt(gamePieces);
                movingPieces = CollapseColumn(gamePieces);
                matches = FindMatchesAt(movingPieces);
                if (matches.Count == 0)
                {
                    isFinshed = true;
                    break;
                }
                else
                    yield return StartCoroutine(IHiglightClearCollapseAndRefill(matches));
            }
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(IRefillAndCheckMatches());
            _isPlayerInputActive = true;
            currentState = GameState.MOVE;
        }
        private void HighlightOnMatchesAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                HighlightTileOn(piece.XIndex, piece.YIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
        private void HighlightTileOn(int x, int y, Color color)
        {
            if (_tileArray[x, y].tileType != TileType.Breakable)
            {
                SpriteRenderer spriteRenderer = _tileArray[x, y].GetComponent<SpriteRenderer>();
                spriteRenderer.color = color;
            }
        }
        public void HighlightOffMatchesAt(int x, int y)
        {
            List<GamePiece> combinedMatches = FindMatchesAt(x, y);

            if (combinedMatches.Count > 0)
            {
                foreach (GamePiece piece in combinedMatches)
                {
                    HighlightTileOff(piece.XIndex, piece.YIndex);

                }
            }
        }
        private void HighlightTileOff(int x, int y)
        {
            if (_tileArray[x, y].tileType != TileType.Breakable)
            {
                SpriteRenderer spriteRenderer = _tileArray[x, y].GetComponent<SpriteRenderer>();
                spriteRenderer.color = Color.black;
            }
        }
        private void ClearPiecesAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece game in gamePieces)
            {
                if (game != null)
                    ClearPiecesAt(game.XIndex, game.YIndex);
            }
        }
        private void ClearPiecesAt(int x, int y)
        {
            GamePiece gamePiece = _gamePieceArray[x, y];
           
            if (gamePiece != null)
            {
                _gamePieceArray[x, y] = null;
                Destroy(gamePiece.gameObject);
               
                
            }
        }

        public void BreakTileAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    BreakTileAt(piece.XIndex, piece.YIndex);
                }
            }
        }
        private void BreakTileAt(int x, int y)
        {
            Tile tileToBreak = _tileArray[x, y];
            if (tileToBreak != null)
            {
                tileToBreak.BreakTile();
            }
        }

        private List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            List<int> columnToCollapse = GetColumn(gamePieces);

            foreach (int column in columnToCollapse)
            {
                movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
            }
            return movingPieces;
        }
        private List<int> GetColumn(List<GamePiece> gamePieces)
        {
            List<int> column = new List<int>();
            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    if (!column.Contains(piece.XIndex))
                        column.Add(piece.XIndex);
                }

            }
            return column;
        }
        private List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            for (int i = 0; i < _boardHeight - 1; i++)
            {
                if (_gamePieceArray[column, i] == null && _tileArray[column, i].tileType != TileType.Obstacle)
                {
                    for (int j = i + 1; j < _boardHeight; j++)
                    {
                        if (_gamePieceArray[column, j] != null)
                        {
                            _gamePieceArray[column, j].Move(column, i, collapseTime);
                            _gamePieceArray[column, i] = _gamePieceArray[column, j];
                            _gamePieceArray[column, i].SetCoord(column, i);
                            if (!movingPieces.Contains(_gamePieceArray[column, j]))
                            {
                                movingPieces.Add(_gamePieceArray[column, j]);
                            }
                            _gamePieceArray[column, j] = null;
                            break;
                        }
                    }
                }
            }
            return movingPieces;
        }

        private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
        {
            List<GamePiece> matches = new List<GamePiece>();

            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                    matches = matches.Union(FindMatchesAt(piece.XIndex, piece.YIndex, minLength)).ToList();
            }
            return matches;
        }






        public void HighlightOnMatchesAt(int x, int y)
        {
            List<GamePiece> combinedMatches = FindMatchesAt(x, y);

            if (combinedMatches.Count > 0)
            {
                foreach (GamePiece piece in combinedMatches)
                {
                    HighlightTileOn(piece.XIndex, piece.YIndex, piece.GetComponent<SpriteRenderer>().color);

                }
                //ClearPiecesAt(combinedMatches);
            }

        }
        private void HighlightOffMatchesAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                HighlightTileOff(piece.XIndex, piece.YIndex);
            }
        }

        public void HighlightMatchingPieces()
        {
            for (int i = 0; i < _boardWidth; i++)
            {
                for (int j = 0; j < _boardHeight; j++)
                {
                    HighlightOnMatchesAt(i, j);

                }
            }
        }
       
       
       

        private IEnumerator IRefill()
        {
            FillBoardGamePiece(5, 0.3f);
            yield return null;
        }

        private List<GamePiece> FindAllMatches()
        {
            List<GamePiece> combinesMatches = new List<GamePiece>();
            for (int i = 0; i < _boardWidth; i++)
            {
                for (int j = 0; j < _boardHeight; j++)
                {
                    List<GamePiece> matches = FindMatchesAt(i, j);
                    combinesMatches = combinesMatches.Union(matches).ToList();

                }
            }
            return combinesMatches;
        }

        private IEnumerator IFindNewMatch()
        {
            List<GamePiece> matches = new List<GamePiece>();
            matches = FindAllMatches();
            if (matches != null)
            {
                if (matches.Count == 0)
                    yield return null;
                else
                    yield return StartCoroutine(IHiglightClearCollapseAndRefill(matches));
            }
            else
                yield return null;

        }

        private IEnumerator IRefillAndCheckMatches()
        {
            yield return StartCoroutine(IRefill());
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(IFindNewMatch());
            yield return new WaitForSeconds(0.5f);
            currentState = GameState.MOVE;
        }
     

       

       
        

       

        public int x;
        public int y;
        public void PrintArrayValue()
        {
            Debug.Log(_gamePieceArray[x, y]);
        }
    }
}
