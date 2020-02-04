using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public enum TileType
    {
        Normal,
        Obstacle,
        Breakable
    };

    public class Tile : MonoBehaviour
    {
        [SerializeField] private int _xIndex;
        [SerializeField] private int _yIndex;
        [SerializeField] private Board _board;

        [SerializeField] private int breakaleValue = 0;
        [SerializeField] private Sprite[] _breakableSprites;

        public TileType tileType = TileType.Normal;

        public int XIndex { get { return _xIndex; } private set { _xIndex = value; } }

        public int YIndex { get { return _yIndex; } private set { _yIndex = value; } }

        public void Init(int x, int y, Board board)
        {
            _xIndex = x;
            _yIndex = y;
            _board = board;
            //breakaleValue = 0;
        }

        private void OnMouseDown()
        {
            if (_board != null && _board.currentState == GameState.MOVE)
            {
                _board.ClickTile(this);
            }
        }

        private void OnMouseEnter()
        {
            if (_board != null &&  _board.currentState == GameState.MOVE)
            {
                _board.DragToTile(this);
            }
        }

        private void OnMouseUp()
        {
            if (_board != null && _board.currentState == GameState.MOVE)
            {
                _board.ReleaseTile();
                _board.currentState = GameState.WAIT;
            }
            else
            {
                _board.currentState = GameState.MOVE;
            }
        }

        public void BreakTile()
        {
            if (tileType == TileType.Breakable)
            {
                StartCoroutine(IBreakTile());
            }
        }

        private IEnumerator IBreakTile()
        {
            breakaleValue = Mathf.Clamp(breakaleValue--, 0, breakaleValue);//wt is breakable value
            yield return new WaitForSeconds(0.5f);
            if (_breakableSprites[breakaleValue] != null)
            {
                this.GetComponent<SpriteRenderer>().sprite = _breakableSprites[breakaleValue];
            }

            if (breakaleValue == 0)
            {
                tileType = TileType.Normal;
                this.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }
    }
}
