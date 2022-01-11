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
    public class CashManager : LifeTimeBase.SingletonScript<FoodManager>
    {
        private const float castThickness = 0.4f;
        private IInputManager _input;
        public TextMeshProUGUI ScoreTMP;
        private void Awake()
        {
            _input = InputManager.Instance;
            _input.OnStartTouch += Touch;
        }

        private void OnDisable()
        {
            _input.OnEndTouch -= Touch;
        }

        private void Touch(InputContext input)
        {
            var position = input.Position.ToWorldCoord();
            var hits = Physics2D.CircleCastAll(position, castThickness, Vector2.zero);
            if (hits != null)
            {
                foreach (var hit in hits)
                {
                    if (hit.transform != null
                        && hit.transform.CompareTag(Global.CASH_TAG))
                    {
                        if (int.TryParse(ScoreTMP.text, out int currentCash))
                        {
                            var value = hit.transform.gameObject.GetComponent<Cash>().Value;
                            Destroy(hit.transform.gameObject);
                            currentCash += value;
                            ScoreTMP.text = currentCash.ToString();
                            input.Handled = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
