using DG.Tweening;
using UnityEngine;

namespace Board
{
    [RequireComponent(typeof(Rigidbody))]
    public class Element : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private float peakHeight = 1.5f;
        [SerializeField] private float fullJumpTime = 0.5f;
        [SerializeField] private Vector2Int currentCoordinate = Vector2Int.zero;
        
        
        private bool _hasJumped;

        private void Start()
        {
            transform.position = grid.ToWorld(currentCoordinate);
        }

        private void Update()
        {
            if (_hasJumped) return;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var targetCoordinate = currentCoordinate + Vector2Int.right;
                JumpTo(targetCoordinate);
            }
        }

        private void JumpTo(Vector2Int targetCoordinate)
        {
            _hasJumped = true;

            Vector3 start = transform.position;
            Vector3 end   = grid.ToWorld(targetCoordinate);

            float ascendTime  = fullJumpTime * 0.35f;
            float descendTime = fullJumpTime * 0.35f;
            float settleTime  = fullJumpTime - ascendTime - descendTime;

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DOScale(new Vector3(1.1f, 0.75f, 1.1f), 0.1f).SetEase(Ease.InQuad));

            seq.Append(transform.DOScale(new Vector3(0.8f, 1.3f, 0.8f), 0.08f));
            seq.Join(transform.DOMoveY(peakHeight, ascendTime).SetEase(Ease.OutQuad));
            seq.Join(transform.DOMoveX((start.x + end.x) * 0.5f, ascendTime));

            seq.Append(transform.DOMoveY(0f, descendTime).SetEase(Ease.InQuad));
            seq.Join(transform.DOMoveX(end.x, descendTime));

            seq.Append(transform.DOScale(new Vector3(1.1f, 0.7f, 1.1f), 0.08f));
            seq.Append(transform.DOMoveY(0.25f, 0.08f).SetRelative().SetEase(Ease.OutQuad));
            seq.Append(transform.DOMoveY(-0.25f, 0.08f).SetRelative().SetEase(Ease.InQuad)); 

            seq.Append(transform.DOScale(Vector3.one, settleTime).SetEase(Ease.OutQuad));

            seq.OnComplete(() =>
            {
                currentCoordinate = targetCoordinate;          
                transform.position = grid.ToWorld(currentCoordinate);
            });
        }
    }
}