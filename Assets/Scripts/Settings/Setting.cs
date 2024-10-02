using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

namespace Settings
{
    public class Setting<T>
    {
        public event Action onChanged;

        Func<OptionSet<T>> m_OptionsGetter;
        Option<T> m_DefaultOption;
        OptionSet<T> m_OptionSet;
        Option<T> m_SelectedOption;

        public OptionSet<T> optionSet
        {
            get => m_OptionSet ?? m_OptionsGetter?.Invoke();
        }

        public IReadOnlyList<Option<T>> options
        {
            get => optionSet.options;
        }

        public string name
        {
            get => m_SelectedOption.name;
        }

        public T value
        {
            get => m_SelectedOption.value;
        }

        public Option<T> defaultOption
        {
            get => m_DefaultOption;
        }

        Option<T> selectedOption
        {
            get => m_SelectedOption;
            set
            {
                var previousOption = m_SelectedOption;
                if (previousOption != value)
                {
                    m_SelectedOption = value;
                    onChanged?.Invoke();
                }
            }
        }

        public List<string> choices
        {
            get
            {
                var choices = new List<string>();
                foreach (var option in options)
                {
                    choices.Add(option.name);
                }

                return choices;
            }
        }

        public Setting(OptionSet<T> optionSet)
        {
            m_OptionSet = optionSet;
            m_DefaultOption = options[optionSet.defaultOptionIndex];
            m_SelectedOption = m_DefaultOption;
        }

        public Setting(Func<OptionSet<T>> optionsSetGetter)
        {
            m_OptionsGetter = optionsSetGetter;
            m_DefaultOption = options[optionSet.defaultOptionIndex];
            m_SelectedOption = m_DefaultOption;
        }

        public void SetValueByName(string name)
        {
            foreach (var option in options)
            {
                if (option.name == name)
                {
                    selectedOption = option;
                    return;
                }
            }

            Reset();
        }

        public void SetValue(T value)
        {
            foreach (var option in options)
            {
                if (option.value.Equals(value))
                {
                    selectedOption = option;
                    return;
                }
            }

            Reset();
        }

        public void Reset()
        {
            selectedOption = m_DefaultOption;
        }

        public static implicit operator T(Setting<T> settings) => settings.value;
    }
}
