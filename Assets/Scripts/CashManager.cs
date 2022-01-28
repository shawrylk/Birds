using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Contracts;
using Assets.Scripts.Utilities;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    [DefaultExecutionOrder(Global.CASH_MANAGER_ORDER)]
    public class CashManager : BaseScript
    {
        private static CashManager _instance = null;
        public static CashManager Instance => _instance;

        [SerializeField]
        private TextMeshProUGUI _scoreTMP;

        private const float castThickness = 0.4f;
        private IInputManager _input;
        private int _score;
        private void Awake()
        {
            int.TryParse(_scoreTMP.text, out _score);
            _instance = FindObjectOfType<CashManager>();
            _input = InputManager.Instance;
            _input.OnStartTouch += Touch;
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnStartTouch -= Touch;
            }
        }

        private void Touch(InputContext input)
        {
            var position = input.ScreenPosition.ToWorldCoord();
            var hits = Physics2D.CircleCastAll(position, castThickness, Vector2.zero);
            if (hits != null)
            {
                foreach (var hit in hits)
                {
                    if (hit.transform != null
                        && hit.transform.CompareTag(Global.CASH_TAG))
                    {
                        //if (int.TryParse(ScoreTMP.text, out int currentCash))
                        //{
                        var value = hit.transform.gameObject.GetComponent<Cash>().GetValue();
                        //    Destroy(hit.transform.gameObject, 2f);
                        //    currentCash += value;
                        //    ScoreTMP.text = currentCash.ToString();
                        //}
                        input.Handled = true;
                        break;
                    }
                }
            }
        }

        public (bool isDone, int currentScore) Add(int value)
        {
            _scoreTMP.text = (_score += value).ToString("#,##0");
            return (true, _score);
        }
        public (bool isDone, int currentScore) Minus(int value)
        {
            if (_score >= value)
            {
                _scoreTMP.text = (_score -= value).ToString("#,##0");
                return (true, _score);
            }
            return (false, _score);
        }
    }
}
