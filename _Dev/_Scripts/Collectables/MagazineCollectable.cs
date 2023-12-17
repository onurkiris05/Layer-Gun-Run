using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Interfaces;
using Game.Player;
using Game.Projectiles;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Collectables
{
    [System.Serializable]
    public struct MagazineCollectableData
    {
        public int Health;
        public float ExperienceRewardAmount;
        public GameObject WholeModel;
        public GameObject[] FracturePieces;
    }

    public class MagazineCollectable : BaseCollectable
    {
        [Header("Settings")]
        [SerializeField] private float breathEffectTime;
        [SerializeField] private float breathEffectStrength;
        [SerializeField] private MagazineCollectableData[] collectables;

        [Space] [Header("Components")]
        [SerializeField] private Transform parentTransform;
        [SerializeField] private Transform pieceMinJumpPos;
        [SerializeField] private Transform pieceMaxJumpPos;
        [SerializeField] private TextMeshPro headerText;

        [Space] [Header("Debug")]
        [SerializeField] [ReadOnly] private int currentPieceIndex;
        [SerializeField] [ReadOnly] private float currentRewardExperience;
        [SerializeField] [ReadOnly] private float currentHealth;
        [SerializeField] [ReadOnly] private float totalHealth;
        [SerializeField] [ReadOnly] private bool isKilled;
        [SerializeField] [ReadOnly] private bool isMaxedOut;

        [Inject] private IUIManager uiManager;
        [Inject] private ICameraManager cameraManager;

        private Dictionary<GameObject, Vector3> _piecesOriginalPosDict = new();
        private List<Transform> _piecesToBeCollect = new();

        public int hitCountForNewPiece;
        public GameObject piece;
        public float breakForce;
        private Material currentPieceMaterial;
        float hitCounter;
        private List<GameObject> piecesInstd = new List<GameObject>();
        #region UNITY EVENTS

        private void Start() => Init();
        public void BreakPiece(Vector3 instPosition)
        {
            GameObject newPiece = Instantiate(piece);
            newPiece.transform.position = instPosition;
            newPiece.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1, 1), Random.Range(2, 3), Random.Range(-1, -2))* breakForce);
            piecesInstd.Add(newPiece);
        }
        protected override void OnTriggerEnter(Collider other)
        {
            if (isKilled) return;

            if (other.TryGetComponent(out BulletProjectile bullet))
            {
                Taptic.Light();
                if (isMaxedOut) return;
                hitCounter -= bullet.Power;
                if (hitCounter < 0)
                {
                    BreakPiece(other.transform.position);
                    hitCounter = hitCountForNewPiece;
                }
                VFXSpawner.Instance.PlayVFX("BulletHit", bullet.transform.position);
                currentHealth -= bullet.Power;
                totalHealth -= bullet.Power;
                UpdateHeaderText();
                ProcessFirstImpact();

                bullet.Kill();

                // Process to move to next collectable piece
                if (currentHealth <= 0)
                {
                    KillCurrentBlock();
                    currentPieceIndex++;

                    // Check if its maxed out
                    if (currentPieceIndex >= collectables.Length)
                    {
                        isMaxedOut = true;
                        headerText.enabled = false;
                    }
                    else
                        currentHealth += collectables[currentPieceIndex].Health;
                }
                else
                {
                    ProcessBreathEffect();
                }
            }
            else if (other.TryGetComponent(out PlayerController player))
            {
                StartCoroutine(GetPieces(player));
                player.OnMagazineReward(currentRewardExperience);
                StartCoroutine(ProcessCollectingPieces());
                Kill();
            }
        }
        private IEnumerator GetPieces(PlayerController _player)
        {
            for(int i = 0; i < piecesInstd.Count; i++)
            {
                piecesInstd[i].transform.parent = AlpGameManager.instance.levelBarWorldPositionObject;
                piecesInstd[i].transform.DOLocalMove(Vector3.zero, .4f).OnComplete(delegate {
                    _player.OnMagazineReward(20);
                });
                piecesInstd[i].transform.DOScale(Vector3.zero, .1f).SetDelay(.35f);
                
                yield return new WaitForSeconds(.05f);
            }
        }
        #endregion


        #region PRIVATE METHODS

        private void Init()
        {
            foreach (var block in collectables)
            {
                totalHealth += block.Health;

                for (int i = 0; i < block.FracturePieces.Length; i++)
                    _piecesOriginalPosDict.Add(block.FracturePieces[i],
                        block.FracturePieces[i].transform.localPosition);
            }

            currentHealth = collectables[currentPieceIndex].Health;
            UpdateHeaderText();
        }


        private void UpdateHeaderText()
        {
            headerText.text = Mathf.Max(totalHealth, 0).ToString("0");
        }


        private void ProcessBreathEffect()
        {
            foreach (var piece in collectables[currentPieceIndex].FracturePieces)
            {
                var dir = (piece.transform.position - new Vector3(transform.position.x,
                    piece.transform.position.y, transform.position.z)).normalized;

                var originalPos = _piecesOriginalPosDict[piece];
                var ratio = currentHealth / (float)collectables[currentPieceIndex].Health;
                var currentStrength = Mathf.Lerp(breathEffectStrength, 0, ratio);

                piece.transform.DOKill();
                piece.transform.DOMove(dir * currentStrength, breathEffectTime).SetEase(Ease.Linear)
                    .SetRelative().OnComplete(() =>
                    {
                        piece.transform.DOLocalMove(originalPos, breathEffectTime * 5f).SetEase(Ease.Linear)
                            .SetSpeedBased();
                    });
            }
        }


        private void ProcessFirstImpact()
        {
            if (collectables[currentPieceIndex].WholeModel.activeSelf)
            {
                collectables[currentPieceIndex].WholeModel.SetActive(false);
                currentPieceMaterial = collectables[currentPieceIndex].WholeModel.GetComponent<Renderer>().material;
            }
        }


        private IEnumerator ProcessCollectingPieces()
        {
            if (_piecesToBeCollect.Count <= 0) yield return null;

            foreach (var piece in _piecesToBeCollect)
            {
                var pos = Helpers.GetWorldPositionOfRectTransform(uiManager.MagazineBarUI);

                piece.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
                piece.DOMove(pos, 0.5f).SetEase(Ease.Linear)
                    .OnComplete(() => Destroy(piece.gameObject));

                yield return null;
            }

            _piecesToBeCollect.Clear();
        }


        private void KillCurrentBlock()
        {
            currentRewardExperience += collectables[currentPieceIndex].ExperienceRewardAmount;

            foreach (var piece in collectables[currentPieceIndex].FracturePieces)
            {
                var jumpPos = Helpers.GenerateRandomVector3(pieceMinJumpPos.position, pieceMaxJumpPos.position);

                piece.transform.DOKill();
                piece.transform.parent = null;
                piece.transform.DOJump(jumpPos, 1, 1, 0.4f);
                piece.transform.DOScale(piece.transform.localScale * 0.5f, 0.4f);
                _piecesToBeCollect.Add(piece.transform);
            }

            MoveDownAllModels();

            // Process effects
            var vfxPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z - 1f);
            VFXSpawner.Instance.PlayVFX("MagazineCollectableBreak", vfxPos);
            //cameraManager.ShakeCamera(0.4f, 0.4f, 0.3f);
        }


        private void MoveDownAllModels()
        {
            parentTransform.DOComplete();
            parentTransform.DOLocalMoveY(-0.6f, 0.5f).SetRelative();
            parentTransform.DOPunchScale(new Vector3(1,1,0)*.2f,.2f,1,1);
        }


        private void Kill() => isKilled = true;

        #endregion;
    }
}