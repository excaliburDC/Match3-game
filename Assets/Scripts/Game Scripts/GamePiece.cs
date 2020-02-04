using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class GamePiece : MonoBehaviour
    {
        [SerializeField] private int _xIndex;
        [SerializeField] private int _yIndex;
        [SerializeField] private Board _board;

        private enum InterpolationType
        {
            LINEAR,
            EASYOUT,
            EASYIN,
            SMOOTHSTEP,
            SMOOTHERSTEP
        };

        public enum MatchType
        {
            MILK,
            APPLE,
            LEMON,
            BREAD,
            VEGGIE,
            COCONUT,  
            STAR
        };

        public MatchType currentMatchType; 
        [SerializeField]
        private InterpolationType _currentInterpolationType = InterpolationType.SMOOTHERSTEP;
        private bool _isMoving = false;

        public int XIndex { get { return _xIndex; }  private set { _xIndex = value; } }  
        public int YIndex { get { return _yIndex; }  private set { _yIndex = value; } }

        public void SetCoord(int x, int y)
        {
            XIndex = x;
            YIndex = y;
        }

        public void Init(Board board)
        {
            _board = board;
        }

        private void Update()
        {
            //if (!_isMoving)
            //{
            //    if (Input.GetKeyDown(KeyCode.LeftArrow))//wt is this used for
            //    {
            //        Move(XIndex - 1, YIndex, 0.5f);
            //    }
            //    if (Input.GetKeyDown(KeyCode.RightArrow))
            //    {
            //        Move(XIndex + 1, YIndex, 0.5f);
            //    }
            //}
        }

        public void Move(int xPos, int yPos, float timeToMove)
        {
            StartCoroutine(MovePieces(new Vector3(xPos, yPos, 0), timeToMove));
        }

        private float GetInterpolationTime(InterpolationType interpolationType, float elaspedTime, float timeToMove)
        {
            float time = Mathf.Clamp(elaspedTime / timeToMove, 0, 1);
            switch (interpolationType)
            {
                case InterpolationType.LINEAR:
                    break;
                case InterpolationType.EASYOUT:
                    time = Mathf.Sin(time * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.EASYIN:
                    time = 1 - Mathf.Cos(time * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.SMOOTHSTEP:
                    time = time * time * (3 - 2 * time);
                    break;
                case InterpolationType.SMOOTHERSTEP:
                    time = time * time * time * (time * (time * 6 - 15) + 10);
                    break;
            }
            return time;
        }
        private IEnumerator MovePieces(Vector3 destination, float timeToMove)
        {
            Vector3 startPosition = transform.position;
            bool reachedDestination = false;
            float elaspedTime = 0.0f;
            _isMoving = true;
            while (!reachedDestination)
            {
                if (Vector3.Distance(transform.position, destination) < 0.01f)
                {
                    reachedDestination = true;
                    if (_board != null)
                        _board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                    break;
                }

                elaspedTime += Time.deltaTime;
                float t = GetInterpolationTime(_currentInterpolationType, elaspedTime, timeToMove);
                transform.position = Vector3.Lerp(startPosition, destination, t);
                yield return null;
            }
            _isMoving = false;
            //_board.HighlightOnMatchesAt((int)destination.x,(int)destination.y);
        }
    }
}

/*
 * Interpolation time graph curve
    //float t = Mathf.Clamp(elaspedTime / timeToMove,0,1); //Linear
    //t = Mathf.Sin(t * Mathf.PI * 0.5f);// Easy out
    //t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);// Easy in
    //t = t * t * (3 - 2 * t); // Smooth step, make a s curve
    //t = t * t * t * (t * (t * 6 - 15) + 10); // Smoother Step
*/
