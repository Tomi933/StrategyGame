using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Code.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        public event Action OnDied;

        public Image Slider;
        public TextMeshProUGUI HealthText;

        private float _maxHealth;
        private float _currentHealth;

        public void Init(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
            UpdateView();
        }

        public void Reduce(float damage)
        {
            if (damage < 0) return;

            _currentHealth = Mathf.Clamp(_currentHealth - damage, 0f, _maxHealth);
            UpdateView();

            if (_currentHealth <= 0)
                OnDied?.Invoke();
        }

        private void UpdateView()
        {
            Slider.fillAmount = Mathf.Clamp01(_currentHealth / _maxHealth);
            HealthText.text = $"{_currentHealth} / {_maxHealth}";
        }
    }
}
